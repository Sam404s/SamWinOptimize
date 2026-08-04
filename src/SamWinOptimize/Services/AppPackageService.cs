using System.Text.Json;
using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public sealed class AppPackageService(CommandRunner commandRunner)
{
    private const string Query =
        "@(Get-AppxPackage | Where-Object { -not $_.IsFramework -and -not $_.NonRemovable } | " +
        "Select-Object Name,PackageFullName | Sort-Object Name) | ConvertTo-Json -Compress";

    public async Task<IReadOnlyList<AppPackageInfo>> GetPackagesAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await commandRunner.ExecuteAsync(
            "appx-query", "读取应用包", Query, CommandShell.PowerShell, false, cancellationToken);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(result.Error) ? "应用包列表读取失败。" : result.Error);
        }

        if (string.IsNullOrWhiteSpace(result.Output))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(result.Output);
            var packages = new List<AppPackageInfo>();
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    AddPackage(item, packages);
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                AddPackage(root, packages);
            }

            return packages
                .Where(package => !ProtectedPackagePolicy.TryGetProtectionReason(package, out _))
                .DistinctBy(package => package.PackageFullName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("应用包列表格式无效，无法解析系统返回的数据。", exception);
        }
    }

    public Task<CommandResult> UninstallAsync(
        AppPackageInfo package,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        if (ProtectedPackagePolicy.TryGetProtectionReason(package, out var reason))
        {
            return Task.FromResult(new CommandResult(
                package.PackageFullName,
                package.Name,
                false,
                -4,
                string.Empty,
                $"卸载请求已被组件保护策略拦截：{reason}",
                DateTimeOffset.UtcNow));
        }

        var escapedPackageName = package.PackageFullName.Replace("'", "''", StringComparison.Ordinal);
        var command = $"Remove-AppxPackage -Package '{escapedPackageName}' -ErrorAction Stop";
        return commandRunner.ExecuteAsync(
            package.PackageFullName, package.Name, command, CommandShell.PowerShell, false, cancellationToken);
    }

    private static void AddPackage(JsonElement item, ICollection<AppPackageInfo> packages)
    {
        if (item.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty("Name", out var nameElement)
            || !item.TryGetProperty("PackageFullName", out var fullNameElement))
        {
            return;
        }

        var name = nameElement.GetString();
        var fullName = fullNameElement.GetString();
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(fullName))
        {
            packages.Add(new AppPackageInfo(name.Trim(), fullName.Trim()));
        }
    }
}
