namespace SamWinOptimize.Models;

public enum AppRoute
{
    Dashboard,
    Optimize,
    Cleanup,
    Apps,
    Security,
    Restore,
    Toolbox,
    About
}

public enum RiskLevel
{
    Low,
    Medium,
    High
}

public enum CommandShell
{
    CommandPrompt,
    PowerShell
}

public sealed record SystemAction(
    string Id,
    string Category,
    string Title,
    string Description,
    string ApplyCommand,
    string RevertCommand,
    CommandShell Shell,
    RiskLevel Risk,
    bool RequiresAdministrator,
    bool RequiresRestart,
    bool Recommended);

public sealed record CleanupTask(
    string Id,
    string Category,
    string Title,
    string Description,
    string Command,
    CommandShell Shell,
    RiskLevel Risk,
    bool RequiresAdministrator,
    bool Recommended);

public sealed record CommandResult(
    string ItemId,
    string Title,
    bool Succeeded,
    int ExitCode,
    string Output,
    string Error,
    DateTimeOffset CompletedAt);

public sealed record ExecutionReceipt(
    Guid Id,
    DateTimeOffset StartedAt,
    string Operation,
    IReadOnlyList<CommandResult> Results);

public sealed record SystemSnapshot(
    string WindowsName,
    string WindowsVersion,
    string Architecture,
    string MachineName,
    string ProcessorSummary,
    string MemorySummary,
    string SystemDiskSummary,
    string UptimeSummary,
    bool IsAdministrator,
    bool DefenderRunning);

public sealed record AppPackageInfo(string Name, string PackageFullName);

public sealed record DefenderSnapshot(
    bool Available,
    bool ServiceEnabled,
    bool AntivirusEnabled,
    bool RealTimeProtectionEnabled,
    string SignatureVersion,
    DateTimeOffset? SignatureUpdatedAt,
    string Message);

public sealed record LicenseProductSnapshot(
    string Name,
    string State,
    string Channel,
    string PartialProductKey);

public sealed record LicenseSnapshot(
    bool Available,
    LicenseProductSnapshot? Windows,
    IReadOnlyList<LicenseProductSnapshot> Office,
    string Message);

public sealed record SecurityDiagnosticsSnapshot(
    DefenderSnapshot Defender,
    LicenseSnapshot License);

public sealed record TrustedFileReport(
    string FilePath,
    long Size,
    string Sha256,
    string SignatureStatus,
    string Signer,
    string SignatureMessage);

public sealed record TrustedDownloadReport(
    Uri Source,
    TrustedFileReport File);
