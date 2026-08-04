using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class SecurityPage : AppPage
{
    private readonly SystemInfoService _systemInfoService;
    private readonly SecurityDiagnosticsService _securityDiagnosticsService;
    private readonly ReceiptStore _receiptStore;
    private readonly List<ActionButton> _defenderActionButtons = [];
    private readonly ActionButton _refreshButton;
    private readonly BufferedLabel _defenderStatus;
    private readonly BufferedLabel _licenseStatus;
    private readonly BufferedLabel _officeStatus;
    private readonly BufferedLabel _adminStatus;

    public SecurityPage(
        SystemInfoService systemInfoService,
        SecurityDiagnosticsService securityDiagnosticsService,
        ReceiptStore receiptStore)
        : base("\uE72E", "安全与许可证", "读取 Windows 官方状态，提供安全中心、许可证设置和 Defender 受支持操作入口。", allowBodyScroll: true)
    {
        _systemInfoService = systemInfoService;
        _securityDiagnosticsService = securityDiagnosticsService;
        _receiptStore = receiptStore;

        _refreshButton = new ActionButton
        {
            Text = "刷新状态",
            Width = 108,
            Kind = ActionButtonKind.Secondary
        };
        _refreshButton.Click += async (_, _) => await RefreshStatusAsync();
        Header.ActionHost.Controls.Add(_refreshButton);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.Canvas
        };
        Body.Controls.Add(flow);

        var defenderCard = SecurityPageLayout.BuildCard(
            "\uE83D", "Microsoft Defender",
            "读取服务、实时保护和安全智能版本；更新与扫描均调用 Windows 官方 cmdlet。", 276);
        _defenderStatus = SecurityPageLayout.CreateStatusLabel(new Point(110, 108), new Size(720, 58));
        defenderCard.Controls.Add(_defenderStatus);
        var defenderActions = BuildDefenderActions();
        defenderCard.Controls.Add(defenderActions);
        defenderCard.Resize += (_, _) =>
        {
            SecurityPageLayout.FitStatus(defenderCard, _defenderStatus, 118, 78);
            defenderActions.Width = Math.Max(420, defenderCard.ClientSize.Width - 136);
            defenderActions.Location = new Point(106, defenderCard.ClientSize.Height - 66);
        };
        flow.Controls.Add(defenderCard);

        var activationCard = SecurityPageLayout.BuildCard(
            "\uE73E", "Windows 许可证",
            "只读显示 Windows 授权状态、渠道和部分产品密钥；疑难解答由系统设置处理。", 222);
        _licenseStatus = SecurityPageLayout.CreateStatusLabel(new Point(110, 116), new Size(430, 48));
        activationCard.Controls.Add(_licenseStatus);
        activationCard.Resize += (_, _) => SecurityPageLayout.FitStatus(activationCard, _licenseStatus, 116, 48, 230);
        SecurityPageLayout.AddLauncherButton(activationCard, "打开激活设置", "ms-settings:activation", 124);
        flow.Controls.Add(activationCard);

        var officeCard = SecurityPageLayout.BuildCard(
            "\uE8D2", "Office 许可证",
            "读取本机 Software Licensing Service 中可见的 Office 许可证条目，不安装密钥或修改授权。", 218);
        _officeStatus = SecurityPageLayout.CreateStatusLabel(new Point(110, 116), new Size(760, 48));
        officeCard.Controls.Add(_officeStatus);
        officeCard.Resize += (_, _) => SecurityPageLayout.FitStatus(officeCard, _officeStatus, 116, 22);
        flow.Controls.Add(officeCard);

        var appControlCard = SecurityPageLayout.BuildCard(
            "\uE8A7", "应用与浏览器控制",
            "管理 SmartScreen、基于声誉的保护和漏洞防护。", 202);
        var appControlStatus = SecurityPageLayout.CreateFixedStatus(
            "建议保持 SmartScreen 和基于声誉的保护开启", Theme.Success);
        appControlCard.Controls.Add(appControlStatus);
        appControlCard.Resize += (_, _) => SecurityPageLayout.FitStatus(appControlCard, appControlStatus, 114, 56, 230);
        SecurityPageLayout.AddLauncherButton(appControlCard, "打开应用控制", "windowsdefender://appbrowser", 104);
        flow.Controls.Add(appControlCard);

        var adminCard = SecurityPageLayout.BuildCard(
            "\uE7EF", "权限与执行透明度",
            "默认使用当前用户权限；受支持的系统操作会单独申请 UAC，并保留退出代码与错误记录。", 202);
        _adminStatus = SecurityPageLayout.CreateStatusLabel(new Point(110, 114), new Size(430, 38));
        adminCard.Controls.Add(_adminStatus);
        adminCard.Resize += (_, _) => SecurityPageLayout.FitStatus(adminCard, _adminStatus, 114, 56, 230);
        SecurityPageLayout.AddLauncherButton(adminCard, "打开账户设置", "ms-settings:yourinfo", 104);
        flow.Controls.Add(adminCard);

        flow.Resize += (_, _) => SecurityPageLayout.ResizeCards(flow, Body);
        Body.Resize += (_, _) => SecurityPageLayout.ResizeCards(flow, Body);
        Load += async (_, _) => await RefreshStatusAsync();
    }

    private Control BuildDefenderActions()
    {
        var actions = new FlowLayoutPanel
        {
            Location = new Point(106, 208),
            Size = new Size(760, 50),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        actions.Controls.Add(CreateDefenderActionButton(
            "打开安全中心", 142, (_, _) => SettingsLauncher.Open("windowsdefender:")));
        actions.Controls.Add(CreateDefenderActionButton(
            "更新安全智能", 142, async (_, _) => await RunDefenderActionAsync(
                "更新 Defender 安全智能",
                "将调用 Windows Defender 官方更新命令。过程会申请 UAC，退出代码和错误会写入本地记录。是否继续？",
                _securityDiagnosticsService.UpdateSignaturesAsync)));
        actions.Controls.Add(CreateDefenderActionButton(
            "快速扫描", 116, async (_, _) => await RunDefenderActionAsync(
                "Defender 快速扫描",
                "将启动 Microsoft Defender 快速扫描。扫描可能持续数分钟，结果会保留在 Windows 安全中心和本地执行记录中。是否继续？",
                _securityDiagnosticsService.StartQuickScanAsync)));
        actions.Controls.Add(CreateDefenderActionButton(
            "完整扫描", 116, async (_, _) => await RunDefenderActionAsync(
                "Defender 完整扫描",
                "完整扫描可能持续较长时间并增加磁盘与 CPU 使用率。是否立即启动？",
                _securityDiagnosticsService.StartFullScanAsync),
            ActionButtonKind.Primary));
        return actions;
    }

    private ActionButton CreateDefenderActionButton(
        string text,
        int width,
        EventHandler clickHandler,
        ActionButtonKind kind = ActionButtonKind.Secondary)
    {
        var button = new ActionButton { Text = text, Width = width, Kind = kind };
        button.Click += clickHandler;
        _defenderActionButtons.Add(button);
        return button;
    }

    private async Task RunDefenderActionAsync(
        string title,
        string confirmation,
        Func<CancellationToken, Task<CommandResult>> action)
    {
        if (MessageBox.Show(this, confirmation, title,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        SetBusy(true);
        try
        {
            var startedAt = DateTimeOffset.Now;
            var result = await action(CancellationToken.None);
            await _receiptStore.AppendAsync(
                new ExecutionReceipt(Guid.NewGuid(), startedAt, "Defender 操作", [result]));
            using var dialog = new ExecutionResultDialog(title, [result]);
            dialog.ShowDialog(this);
            await LoadStatusAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RefreshStatusAsync()
    {
        SetBusy(true);
        try
        {
            await LoadStatusAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadStatusAsync()
    {
        try
        {
            var diagnosticsTask = _securityDiagnosticsService.GetSnapshotAsync();
            var systemTask = _systemInfoService.GetSnapshotAsync();
            await Task.WhenAll(diagnosticsTask, systemTask);
            RenderDiagnostics(await diagnosticsTask);

            var system = await systemTask;
            _adminStatus.Text = system.IsAdministrator
                ? "当前会话：管理员权限，系统级动作将直接执行"
                : "当前会话：标准权限，系统级动作会单独申请 UAC";
            _adminStatus.ForeColor = system.IsAdministrator ? Theme.Warning : Theme.Success;
        }
        catch (Exception exception)
        {
            _defenderStatus.Text = $"状态读取失败：{exception.Message}";
            _defenderStatus.ForeColor = Theme.Danger;
            _licenseStatus.Text = "许可证状态读取失败。";
            _licenseStatus.ForeColor = Theme.Danger;
            _officeStatus.Text = "Office 许可证状态读取失败。";
            _officeStatus.ForeColor = Theme.Danger;
        }
    }

    private void RenderDiagnostics(SecurityDiagnosticsSnapshot snapshot)
    {
        var defender = snapshot.Defender;
        if (!defender.Available)
        {
            _defenderStatus.Text = defender.Message;
            _defenderStatus.ForeColor = Theme.Warning;
        }
        else
        {
            var updated = defender.SignatureUpdatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "未知";
            _defenderStatus.Text =
                $"服务：{OnOff(defender.ServiceEnabled)} · 防病毒：{OnOff(defender.AntivirusEnabled)} · 实时保护：{OnOff(defender.RealTimeProtectionEnabled)}\n" +
                $"安全智能：{defender.SignatureVersion} · 更新时间：{updated}";
            _defenderStatus.ForeColor = defender.ServiceEnabled && defender.RealTimeProtectionEnabled
                ? Theme.Success
                : Theme.Warning;
        }

        var license = snapshot.License;
        _licenseStatus.Text = license.Windows is { } windows
            ? $"{windows.Name}\n状态：{windows.State} · 渠道：{windows.Channel} · 部分密钥：{windows.PartialProductKey}"
            : license.Message;
        _licenseStatus.ForeColor = license.Windows?.State == "已授权" ? Theme.Success : Theme.Warning;

        _officeStatus.Text = license.Office.Count == 0
            ? (string.IsNullOrWhiteSpace(license.Message) ? "未找到 Office 许可证条目。" : license.Message)
            : $"已发现 {license.Office.Count} 个许可证 · {license.Office[0].Name}\n" +
              $"状态：{license.Office[0].State} · 渠道：{license.Office[0].Channel} · 部分密钥：{license.Office[0].PartialProductKey}";
        _officeStatus.ForeColor = license.Office.Any(product => product.State == "已授权")
            ? Theme.Success
            : Theme.Warning;
    }

    private void SetBusy(bool busy)
    {
        _refreshButton.Enabled = !busy;
        _refreshButton.Text = busy ? "正在处理…" : "刷新状态";
        foreach (var button in _defenderActionButtons)
        {
            button.Enabled = !busy;
        }
    }

    private static string OnOff(bool enabled) => enabled ? "开启" : "关闭";
}
