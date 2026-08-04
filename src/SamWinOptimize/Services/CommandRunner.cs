using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public sealed class CommandRunner
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(30);
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private const int OutputLimit = 64 * 1024;

    public async Task<CommandResult> ExecuteAsync(
        string itemId,
        string title,
        string command,
        CommandShell shell,
        bool requiresAdministrator,
        CancellationToken cancellationToken = default,
        TimeSpan? timeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        Process? process = null;
        ElevatedArtifacts? elevatedArtifacts = null;
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout ?? DefaultTimeout);

        try
        {
            if (requiresAdministrator)
            {
                elevatedArtifacts = ElevatedArtifacts.Create(shell, command);
            }

            process = new Process
            {
                StartInfo = CreateStartInfo(command, shell, requiresAdministrator, elevatedArtifacts),
                EnableRaisingEvents = true
            };

            if (!process.Start())
            {
                return Failed(itemId, title, -1, "系统命令未能启动。", string.Empty);
            }

            if (requiresAdministrator)
            {
                await process.WaitForExitAsync(timeoutSource.Token);
                var output = await ReadCaptureAsync(elevatedArtifacts!.OutputPath, timeoutSource.Token);
                var error = await ReadCaptureAsync(elevatedArtifacts.ErrorPath, timeoutSource.Token);
                if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(error))
                {
                    error = "管理员进程返回非零退出代码；请查看执行输出了解详情。";
                }

                return CreateResult(itemId, title, process.ExitCode, output, error);
            }

            var outputTask = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
            var errorTask = process.StandardError.ReadToEndAsync(timeoutSource.Token);
            await process.WaitForExitAsync(timeoutSource.Token);
            var standardOutput = await outputTask;
            var standardError = await errorTask;

            return CreateResult(itemId, title, process.ExitCode, standardOutput, standardError);
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            return Failed(itemId, title, 1223, "用户取消了管理员授权。", string.Empty);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryTerminate(process);
            var duration = timeout ?? DefaultTimeout;
            return Failed(itemId, title, -3, $"操作超过 {duration.TotalMinutes:0.#} 分钟并已终止。", string.Empty);
        }
        catch (OperationCanceledException)
        {
            TryTerminate(process);
            return Failed(itemId, title, -2, "操作已取消，并已请求终止相关进程。", string.Empty);
        }
        catch (Exception exception)
        {
            TryTerminate(process);
            return Failed(itemId, title, -1, exception.Message, string.Empty);
        }
        finally
        {
            process?.Dispose();
            elevatedArtifacts?.Dispose();
        }
    }

    public Task<CommandResult> ApplyAsync(SystemAction action, CancellationToken cancellationToken = default) =>
        ExecuteAsync(action.Id, action.Title, action.ApplyCommand, action.Shell,
            action.RequiresAdministrator, cancellationToken);

    public Task<CommandResult> RevertAsync(SystemAction action, CancellationToken cancellationToken = default) =>
        ExecuteAsync(action.Id, $"还原：{action.Title}", action.RevertCommand, action.Shell,
            action.RequiresAdministrator, cancellationToken);

    public Task<CommandResult> CleanupAsync(CleanupTask task, CancellationToken cancellationToken = default) =>
        ExecuteAsync(task.Id, task.Title, task.Command, task.Shell,
            task.RequiresAdministrator, cancellationToken);

    private static ProcessStartInfo CreateStartInfo(
        string command,
        CommandShell shell,
        bool requiresAdministrator,
        ElevatedArtifacts? elevatedArtifacts)
    {
        if (requiresAdministrator)
        {
            ArgumentNullException.ThrowIfNull(elevatedArtifacts);
            return shell == CommandShell.PowerShell
                ? new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File {QuoteArgument(elevatedArtifacts.ScriptPath)}",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
                : new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/d /s /c \"\"{elevatedArtifacts.ScriptPath}\"\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
        }

        var fileName = shell == CommandShell.PowerShell ? "powershell.exe" : "cmd.exe";
        var arguments = shell == CommandShell.PowerShell
            ? $"-NoLogo -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -EncodedCommand {EncodePowerShell(command)}"
            : $"/d /s /c \"{command}\"";

        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Utf8NoBom,
            StandardErrorEncoding = Utf8NoBom
        };
    }

    private static string EncodePowerShell(string command) =>
        Convert.ToBase64String(Encoding.Unicode.GetBytes(command));

    private static string QuoteArgument(string value) => $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static async Task<string> ReadCaptureAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Utf8NoBom, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static CommandResult CreateResult(
        string itemId,
        string title,
        int exitCode,
        string output,
        string error) =>
        new(
            itemId,
            title,
            exitCode == 0,
            exitCode,
            Normalize(output),
            Normalize(error),
            DateTimeOffset.UtcNow);

    private static CommandResult Failed(
        string itemId,
        string title,
        int exitCode,
        string error,
        string output) =>
        CreateResult(itemId, title, exitCode, output, error);

    private static string Normalize(string value)
    {
        var normalized = CommandOutputRedactor.Redact(value).Trim();
        if (normalized.Length <= OutputLimit)
        {
            return normalized;
        }

        const int tailLength = 12 * 1024;
        var headLength = OutputLimit - tailLength;
        return string.Concat(
            normalized[..headLength],
            Environment.NewLine,
            $"… 输出已截断，原始长度 {normalized.Length:N0} 个字符 …",
            Environment.NewLine,
            normalized[^tailLength..]);
    }

    private static void TryTerminate(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // 进程可能已退出，或提升后的进程不允许当前会话终止。
        }
    }

    private sealed class ElevatedArtifacts : IDisposable
    {
        private ElevatedArtifacts(string directory, string scriptPath, string outputPath, string errorPath)
        {
            Directory = directory;
            ScriptPath = scriptPath;
            OutputPath = outputPath;
            ErrorPath = errorPath;
        }

        public string Directory { get; }
        public string ScriptPath { get; }
        public string OutputPath { get; }
        public string ErrorPath { get; }

        public static ElevatedArtifacts Create(CommandShell shell, string command)
        {
            var directory = Path.Combine(Path.GetTempPath(), "SamWinOptimize", Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);

            var scriptExtension = shell == CommandShell.PowerShell ? ".ps1" : ".cmd";
            var scriptPath = Path.Combine(directory, $"execute{scriptExtension}");
            var outputPath = Path.Combine(directory, "stdout.log");
            var errorPath = Path.Combine(directory, "stderr.log");
            var script = shell == CommandShell.PowerShell
                ? BuildPowerShellScript(command, outputPath, errorPath)
                : BuildCommandScript(command, outputPath, errorPath);
            File.WriteAllText(scriptPath, script, Utf8NoBom);
            return new ElevatedArtifacts(directory, scriptPath, outputPath, errorPath);
        }

        public void Dispose()
        {
            try
            {
                if (System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.Delete(Directory, recursive: true);
                }
            }
            catch
            {
                // 临时文件清理失败不应覆盖原始命令结果。
            }
        }

        private static string BuildCommandScript(string command, string outputPath, string errorPath) =>
            $"@echo off{Environment.NewLine}" +
            $"chcp 65001 >nul{Environment.NewLine}" +
            $"{command} 1>\"{outputPath}\" 2>\"{errorPath}\"{Environment.NewLine}" +
            $"exit /b %errorlevel%{Environment.NewLine}";

        private static string BuildPowerShellScript(string command, string outputPath, string errorPath)
        {
            var escapedOutput = EscapePowerShellLiteral(outputPath);
            var escapedError = EscapePowerShellLiteral(errorPath);
            return $"$ErrorActionPreference = 'Stop'{Environment.NewLine}" +
                "$ProgressPreference = 'SilentlyContinue'" + Environment.NewLine +
                "try {" + Environment.NewLine +
                $"    & {{ {command} }} *> '{escapedOutput}'{Environment.NewLine}" +
                "    if ($LASTEXITCODE -is [int]) { exit $LASTEXITCODE }" + Environment.NewLine +
                "    exit 0" + Environment.NewLine +
                "}" + Environment.NewLine +
                "catch {" + Environment.NewLine +
                $"    $_ | Out-File -LiteralPath '{escapedError}' -Encoding utf8" + Environment.NewLine +
                "    exit 1" + Environment.NewLine +
                "}" + Environment.NewLine;
        }

        private static string EscapePowerShellLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);
    }
}
