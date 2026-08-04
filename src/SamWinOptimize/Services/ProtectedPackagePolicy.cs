using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public static class ProtectedPackagePolicy
{
    private static readonly (string Identifier, string Reason)[] Rules =
    [
        ("Microsoft.MicrosoftEdge", "Microsoft Edge 属于受支持的系统浏览器组件。"),
        ("Microsoft.Edge", "Edge 与系统 Web 体验组件受保护。"),
        ("Microsoft.WebView2", "WebView2 是多个桌面应用依赖的运行时。"),
        ("Microsoft.WindowsStore", "Microsoft Store 用于受支持的应用安装与恢复。"),
        ("Microsoft.StorePurchaseApp", "Store 购买与许可证组件受保护。"),
        ("Microsoft.Windows.SecHealthUI", "Windows 安全中心界面受保护。"),
        ("Microsoft.SecHealthUI", "Windows 安全中心界面受保护。"),
        ("Microsoft.DesktopAppInstaller", "应用安装器与 winget 组件受保护。"),
        ("Microsoft.WindowsShellExperienceHost", "Windows Shell 体验主机受保护。"),
        ("Microsoft.ShellExperienceHost", "Windows Shell 体验主机受保护。"),
        ("Microsoft.Windows.StartMenuExperienceHost", "开始菜单体验组件受保护。"),
        ("MicrosoftWindows.Client.CBS", "Windows 客户端核心体验组件受保护。"),
        ("MicrosoftWindows.Client.Core", "Windows 客户端核心组件受保护。"),
        ("MicrosoftWindows.Client.WebExperience", "Windows Web 体验组件受保护。"),
        ("Microsoft.Windows.Search", "Windows 搜索组件受保护。"),
        ("Microsoft.AAD.BrokerPlugin", "Windows 账户身份代理受保护。"),
        ("Microsoft.AccountsControl", "Windows 账户控制组件受保护。"),
        ("Microsoft.LockApp", "Windows 锁屏组件受保护。"),
        ("Microsoft.VCLibs", "应用运行时框架受保护。"),
        ("Microsoft.NET.Native", ".NET Native 运行时框架受保护。"),
        ("Microsoft.UI.Xaml", "Windows UI 运行时框架受保护。")
    ];

    public static bool TryGetProtectionReason(AppPackageInfo package, out string reason) =>
        TryGetProtectionReason(package.Name, package.PackageFullName, out reason);

    public static bool TryGetProtectionReason(string name, string packageFullName, out string reason)
    {
        foreach (var rule in Rules)
        {
            if (name.Contains(rule.Identifier, StringComparison.OrdinalIgnoreCase)
                || packageFullName.Contains(rule.Identifier, StringComparison.OrdinalIgnoreCase))
            {
                reason = rule.Reason;
                return true;
            }
        }

        reason = string.Empty;
        return false;
    }
}
