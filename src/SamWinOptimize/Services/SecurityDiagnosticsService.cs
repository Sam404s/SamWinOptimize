using System.Text.Json;
using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public sealed class SecurityDiagnosticsService(CommandRunner commandRunner)
{
    private const string DefenderQuery = """
        $status = Get-MpComputerStatus -ErrorAction Stop
        [pscustomobject]@{
            ServiceEnabled = [bool]$status.AMServiceEnabled
            AntivirusEnabled = [bool]$status.AntivirusEnabled
            RealTimeProtectionEnabled = [bool]$status.RealTimeProtectionEnabled
            SignatureVersion = [string]$status.AntivirusSignatureVersion
            SignatureUpdatedAt = $(if ($status.AntivirusSignatureLastUpdated) { $status.AntivirusSignatureLastUpdated.ToString('o') } else { $null })
        } | ConvertTo-Json -Compress
        """;

    private const string LicenseQuery = """
        $windows = Get-CimInstance SoftwareLicensingProduct -Filter "ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f'" -ErrorAction Stop |
            Where-Object { $_.PartialProductKey } |
            Sort-Object LicenseStatus -Descending |
            Select-Object -First 1
        $office = @(Get-CimInstance SoftwareLicensingProduct -Filter "ApplicationID='0ff1ce15-a989-479d-af46-f275c6370663'" -ErrorAction SilentlyContinue |
            Where-Object { $_.PartialProductKey } |
            Sort-Object LicenseStatus -Descending |
            ForEach-Object {
                [pscustomobject]@{
                    Name = [string]$_.Name
                    Description = [string]$_.Description
                    LicenseStatus = [int]$_.LicenseStatus
                    PartialProductKey = [string]$_.PartialProductKey
                }
            })
        [pscustomobject]@{
            Windows = $(if ($null -ne $windows) {
                [pscustomobject]@{
                    Name = [string]$windows.Name
                    Description = [string]$windows.Description
                    LicenseStatus = [int]$windows.LicenseStatus
                    PartialProductKey = [string]$windows.PartialProductKey
                }
            } else { $null })
            Office = $office
        } | ConvertTo-Json -Compress -Depth 4
        """;

    public async Task<SecurityDiagnosticsSnapshot> GetSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        var defenderTask = commandRunner.ExecuteAsync(
            "defender-status", "读取 Defender 状态", DefenderQuery,
            CommandShell.PowerShell, false, cancellationToken, TimeSpan.FromMinutes(2));
        var licenseTask = commandRunner.ExecuteAsync(
            "license-status", "读取许可证状态", LicenseQuery,
            CommandShell.PowerShell, false, cancellationToken, TimeSpan.FromMinutes(2));

        await Task.WhenAll(defenderTask, licenseTask);
        return new SecurityDiagnosticsSnapshot(
            ParseDefender(await defenderTask),
            ParseLicense(await licenseTask));
    }

    public Task<CommandResult> UpdateSignaturesAsync(CancellationToken cancellationToken = default) =>
        commandRunner.ExecuteAsync(
            "defender-signature-update",
            "更新 Defender 安全智能",
            "Update-MpSignature -ErrorAction Stop",
            CommandShell.PowerShell,
            true,
            cancellationToken,
            TimeSpan.FromMinutes(15));

    public Task<CommandResult> StartQuickScanAsync(CancellationToken cancellationToken = default) =>
        commandRunner.ExecuteAsync(
            "defender-quick-scan",
            "Defender 快速扫描",
            "Start-MpScan -ScanType QuickScan -ErrorAction Stop",
            CommandShell.PowerShell,
            true,
            cancellationToken,
            TimeSpan.FromMinutes(45));

    public Task<CommandResult> StartFullScanAsync(CancellationToken cancellationToken = default) =>
        commandRunner.ExecuteAsync(
            "defender-full-scan",
            "Defender 完整扫描",
            "Start-MpScan -ScanType FullScan -ErrorAction Stop",
            CommandShell.PowerShell,
            true,
            cancellationToken,
            TimeSpan.FromHours(4));

    private static DefenderSnapshot ParseDefender(CommandResult result)
    {
        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.Output))
        {
            return new DefenderSnapshot(
                false, false, false, false, "—", null,
                string.IsNullOrWhiteSpace(result.Error) ? "Defender 状态不可用。" : result.Error);
        }

        try
        {
            using var document = JsonDocument.Parse(result.Output);
            var root = document.RootElement;
            var signatureUpdatedAt = TryGetString(root, "SignatureUpdatedAt", out var updatedText)
                && DateTimeOffset.TryParse(updatedText, out var updatedAt)
                    ? (DateTimeOffset?)updatedAt
                    : null;
            return new DefenderSnapshot(
                true,
                TryGetBoolean(root, "ServiceEnabled"),
                TryGetBoolean(root, "AntivirusEnabled"),
                TryGetBoolean(root, "RealTimeProtectionEnabled"),
                TryGetString(root, "SignatureVersion", out var version) ? version : "—",
                signatureUpdatedAt,
                string.Empty);
        }
        catch (JsonException exception)
        {
            return new DefenderSnapshot(false, false, false, false, "—", null, exception.Message);
        }
    }

    private static LicenseSnapshot ParseLicense(CommandResult result)
    {
        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.Output))
        {
            return new LicenseSnapshot(
                false, null, [],
                string.IsNullOrWhiteSpace(result.Error) ? "许可证状态不可用。" : result.Error);
        }

        try
        {
            using var document = JsonDocument.Parse(result.Output);
            var root = document.RootElement;
            LicenseProductSnapshot? windows = null;
            if (root.TryGetProperty("Windows", out var windowsElement)
                && windowsElement.ValueKind == JsonValueKind.Object)
            {
                windows = ParseLicenseProduct(windowsElement);
            }

            var office = new List<LicenseProductSnapshot>();
            if (root.TryGetProperty("Office", out var officeElement))
            {
                if (officeElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in officeElement.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object)
                        {
                            office.Add(ParseLicenseProduct(item));
                        }
                    }
                }
                else if (officeElement.ValueKind == JsonValueKind.Object)
                {
                    office.Add(ParseLicenseProduct(officeElement));
                }
            }

            return new LicenseSnapshot(
                windows is not null || office.Count > 0,
                windows,
                office,
                windows is null && office.Count == 0 ? "未找到带部分产品密钥的许可证条目。" : string.Empty);
        }
        catch (JsonException exception)
        {
            return new LicenseSnapshot(false, null, [], exception.Message);
        }
    }

    private static LicenseProductSnapshot ParseLicenseProduct(JsonElement element)
    {
        var name = TryGetString(element, "Name", out var productName) ? productName : "未知产品";
        var description = TryGetString(element, "Description", out var productDescription)
            ? productDescription
            : string.Empty;
        var status = element.TryGetProperty("LicenseStatus", out var statusElement)
            && statusElement.TryGetInt32(out var statusCode)
                ? MapLicenseStatus(statusCode)
                : "未知";
        var partialKey = TryGetString(element, "PartialProductKey", out var key) ? key : "—";
        return new LicenseProductSnapshot(name, status, ExtractChannel(description), partialKey);
    }

    private static string MapLicenseStatus(int status) => status switch
    {
        0 => "未授权",
        1 => "已授权",
        2 => "初始宽限期",
        3 => "额外宽限期",
        4 => "非正版宽限期",
        5 => "通知模式",
        6 => "延长宽限期",
        _ => $"状态 {status}"
    };

    private static string ExtractChannel(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "通道未知";
        }

        var commaIndex = description.LastIndexOf(',');
        return (commaIndex >= 0 ? description[(commaIndex + 1)..] : description).Trim();
    }

    private static bool TryGetBoolean(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
        && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        && value.GetBoolean();

    private static bool TryGetString(JsonElement element, string name, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return true;
    }
}
