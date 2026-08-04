using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class DashboardPage : AppPage
{
    private readonly SystemInfoService _systemInfoService;
    private readonly Action<AppRoute> _navigate;
    private readonly Label _machineLabel;
    private readonly Label _systemLabel;
    private readonly StatusPill _adminPill;
    private readonly StatusPill _defenderPill;
    private readonly MetricCard _processorCard;
    private readonly MetricCard _memoryCard;
    private readonly MetricCard _diskCard;
    private readonly MetricCard _uptimeCard;

    public DashboardPage(SystemInfoService systemInfoService, Action<AppRoute> navigate)
        : base("\uE80F", "\u8bbe\u5907\u603b\u89c8", "\u5148\u770b\u6e05\u8bbe\u5907\u72b6\u6001\uff0c\u518d\u9009\u62e9\u9002\u5408\u7684\u7ef4\u62a4\u64cd\u4f5c\u3002", allowBodyScroll: true)
    {
        _systemInfoService = systemInfoService;
        _navigate = navigate;

        var refreshButton = new ActionButton
        {
            Text = "\u5237\u65b0\u72b6\u6001",
            Width = 122,
            Kind = ActionButtonKind.Secondary
        };
        refreshButton.Click += async (_, _) => await RefreshSnapshotAsync();
        Header.ActionHost.Controls.Add(refreshButton);

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.Canvas,
            Padding = new Padding(0)
        };
        Body.Controls.Add(content);

        var hero = new SurfacePanel
        {
            Height = 280,
            Width = 980,
            Padding = new Padding(36),
            SurfaceStyle = SurfaceStyle.Accent,
            Margin = new Padding(0, 0, 0, 24)
        };
        var overline = new Label
        {
            Text = "DEVICE STATUS  /  READY",
            Font = Theme.MonoFont(8.2f, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            Location = new Point(36, 36),
            BackColor = Color.Transparent
        };
        _machineLabel = new Label
        {
            Text = Environment.MachineName,
            Font = Theme.DisplayFont(29, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoEllipsis = true,
            Location = new Point(35, 72),
            Size = new Size(650, 56),
            BackColor = Color.Transparent
        };
        _systemLabel = new Label
        {
            Text = "\u6b63\u5728\u8bfb\u53d6 Windows \u7248\u672c\u4e0e\u8bbe\u5907\u72b6\u6001\u2026",
            Font = Theme.Font(10.5f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(38, 140),
            Size = new Size(650, 34),
            BackColor = Color.Transparent
        };
        var statusFlow = new FlowLayoutPanel
        {
            Location = new Point(38, 192),
            AutoSize = true,
            BackColor = Color.Transparent,
            WrapContents = false
        };
        _adminPill = new StatusPill("\u68c0\u6d4b\u6743\u9650", Theme.Info);
        _defenderPill = new StatusPill("\u68c0\u6d4b\u5b89\u5168\u72b6\u6001", Theme.Info);
        statusFlow.Controls.Add(_adminPill);
        statusFlow.Controls.Add(_defenderPill);

        var actionPanel = new FlowLayoutPanel
        {
            Size = new Size(196, 132),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        var optimizeButton = new ActionButton
        {
            Text = "\u67e5\u770b\u7cfb\u7edf\u4f18\u5316",
            Kind = ActionButtonKind.Primary,
            Size = new Size(194, 52),
            Margin = new Padding(0, 0, 0, 16)
        };
        optimizeButton.Click += (_, _) => _navigate(AppRoute.Optimize);
        var cleanupButton = new ActionButton
        {
            Text = "\u91ca\u653e\u78c1\u76d8\u7a7a\u95f4",
            Kind = ActionButtonKind.Secondary,
            Size = new Size(194, 52),
            Margin = new Padding(0)
        };
        cleanupButton.Click += (_, _) => _navigate(AppRoute.Cleanup);
        actionPanel.Controls.Add(optimizeButton);
        actionPanel.Controls.Add(cleanupButton);
        hero.Resize += (_, _) =>
        {
            actionPanel.Location = new Point(hero.ClientSize.Width - 234, 68);
            var textWidth = Math.Max(360, actionPanel.Left - 76);
            _machineLabel.Width = textWidth;
            _systemLabel.Width = textWidth;
        };
        hero.Controls.Add(overline);
        hero.Controls.Add(_machineLabel);
        hero.Controls.Add(_systemLabel);
        hero.Controls.Add(statusFlow);
        hero.Controls.Add(actionPanel);
        content.Controls.Add(hero);

        var metricsRow = new FlowLayoutPanel
        {
            Width = 980,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            BackColor = Theme.Canvas,
            Margin = new Padding(0, 0, 0, 24)
        };
        _processorCard = new MetricCard("\uE950", "\u5904\u7406\u5668");
        _memoryCard = new MetricCard("\uEDA2", "\u5185\u5b58");
        _diskCard = new MetricCard("\uE7B8", "\u7cfb\u7edf\u78c1\u76d8");
        _uptimeCard = new MetricCard("\uE823", "\u8fd0\u884c\u65f6\u957f");
        metricsRow.Controls.Add(_processorCard);
        metricsRow.Controls.Add(_memoryCard);
        metricsRow.Controls.Add(_diskCard);
        metricsRow.Controls.Add(_uptimeCard);
        content.Controls.Add(metricsRow);

        content.Controls.Add(BuildGuidancePanel());

        void ResizeAll() => ResizeContent(content, hero, metricsRow);
        content.Resize += (_, _) => ResizeAll();
        Body.Resize += (_, _) => ResizeAll();
        Load += async (_, _) => await RefreshSnapshotAsync();
    }

    private Control BuildGuidancePanel()
    {
        var panel = new SurfacePanel
        {
            Width = 980,
            Height = 280,
            Padding = new Padding(30),
            SurfaceStyle = SurfaceStyle.Raised,
            Margin = new Padding(0, 0, 0, 20)
        };
        panel.Controls.Add(new Label
        {
            Text = "\u5feb\u901f\u4e0a\u624b",
            Font = Theme.DisplayFont(15, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(30, 26),
            BackColor = Color.Transparent
        });
        panel.Controls.Add(new Label
        {
            Text = "\u4e09\u6b65\u5b8c\u6210\u4e00\u6b21\u5b89\u5168\u7ef4\u62a4\u3002",
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = true,
            Location = new Point(31, 60),
            BackColor = Color.Transparent
        });
        var rows = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 3,
            Location = new Point(30, 100),
            Size = new Size(920, 148),
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        for (var index = 0; index < 3; index++)
        {
            rows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
        }
        rows.Controls.Add(BuildGuidanceRow(1, "\u67e5\u770b\u8bbe\u5907\u603b\u89c8", "\u786e\u8ba4\u7cfb\u7edf\u7248\u672c\u3001\u6743\u9650\u548c\u5b89\u5168\u72b6\u6001\u3002", AppRoute.Dashboard), 0, 0);
        rows.Controls.Add(BuildGuidanceRow(2, "\u6267\u884c\u7a7a\u95f4\u6e05\u7406", "\u4f18\u5148\u4f7f\u7528\u4f4e\u98ce\u9669\u6e05\u7406\u91ca\u653e\u78c1\u76d8\u3002", AppRoute.Cleanup), 0, 1);
        rows.Controls.Add(BuildGuidanceRow(3, "\u67e5\u770b\u6267\u884c\u8bb0\u5f55\u4e0e\u8fd8\u539f", "\u786e\u8ba4\u7ed3\u679c\uff0c\u5fc5\u8981\u65f6\u8fd8\u539f\u7ba1\u7406\u5458\u7ea7\u4f18\u5316\u9879\u3002", AppRoute.Restore), 0, 2);
        panel.Controls.Add(rows);
        panel.Resize += (_, _) => rows.Width = Math.Max(620, panel.ClientSize.Width - 60);
        return panel;
    }

    private Control BuildGuidanceRow(int index, string title, string description, AppRoute route)
    {
        var row = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(18),
            SurfaceStyle = SurfaceStyle.Quiet,
            Hoverable = true,
            Cursor = Cursors.Hand,
            Radius = Theme.RadiusMd
        };
        var number = new Label
        {
            Text = index.ToString("00"),
            Font = Theme.MonoFont(9, FontStyle.Bold),
            ForeColor = Theme.Accent,
            Size = new Size(40, 34),
            Location = new Point(20, 14),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };
        var text = new Label
        {
            Text = title,
            Font = Theme.Font(10.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Location = new Point(68, 12),
            Size = new Size(240, 30),
            BackColor = Color.Transparent
        };
        var detail = new Label
        {
            Text = description,
            Font = Theme.Font(9.2f),
            ForeColor = Theme.TextSecondary,
            Location = new Point(310, 13),
            Size = new Size(500, 30),
            BackColor = Color.Transparent
        };
        var arrow = new Label
        {
            Text = "\uE72A",
            Font = Theme.IconFont(11),
            ForeColor = Theme.Accent,
            Size = new Size(36, 34),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        row.Resize += (_, _) =>
        {
            arrow.Location = new Point(row.ClientSize.Width - 56, 13);
            detail.Width = Math.Max(160, arrow.Left - detail.Left - 20);
        };
        void Navigate(object? _, EventArgs __) => _navigate(route);
        row.Click += Navigate;
        foreach (var control in new Control[] { number, text, detail, arrow })
        {
            control.Click += Navigate;
            row.Controls.Add(control);
        }
        return row;
    }

    private async Task RefreshSnapshotAsync()
    {
        try
        {
            var snapshot = await _systemInfoService.GetSnapshotAsync();
            if (IsDisposed)
            {
                return;
            }

            _machineLabel.Text = snapshot.MachineName;
            _systemLabel.Text = $"{snapshot.WindowsName} \u00b7 {snapshot.WindowsVersion} \u00b7 {snapshot.Architecture}";
            SetPill(_adminPill, snapshot.IsAdministrator ? "\u7ba1\u7406\u5458\u4f1a\u8bdd" : "\u5efa\u8bae\u4f7f\u7528\u7ba1\u7406\u5458", snapshot.IsAdministrator ? Theme.Warning : Theme.Success);
            SetPill(_defenderPill, snapshot.DefenderRunning ? "\u5b89\u5168\u9632\u62a4\u5df2\u5f00\u542f" : "\u672a\u68c0\u6d4b\u5230 Defender \u8fd0\u884c", snapshot.DefenderRunning ? Theme.Success : Theme.Warning);
            _processorCard.SetValue(TrimProcessor(snapshot.ProcessorSummary), $"{Environment.ProcessorCount} \u4e2a\u903b\u8f91\u5904\u7406\u5668");
            _memoryCard.SetValue(snapshot.MemorySummary, "\u5f53\u524d\u4f7f\u7528 / \u603b\u7269\u7406\u5185\u5b58");
            _diskCard.SetValue(snapshot.SystemDiskSummary, "\u5df2\u4f7f\u7528 / \u603b\u5bb9\u91cf");
            _uptimeCard.SetValue(snapshot.UptimeSummary, "\u8ddd\u79bb\u4e0a\u6b21\u91cd\u542f\u7684\u65f6\u95f4");
        }
        catch (Exception exception)
        {
            _systemLabel.Text = $"\u72b6\u6001\u8bfb\u53d6\u5931\u8d25\uff1a{exception.Message}";
            _systemLabel.ForeColor = Theme.Danger;
        }
    }

    private static string TrimProcessor(string processor)
    {
        var cleaned = processor.Replace("(R)", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("(TM)", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
        return cleaned.Length > 28 ? cleaned[..28] + "\u2026" : cleaned;
    }

    private static void SetPill(StatusPill pill, string text, Color color)
    {
        pill.Text = $"  {text}  ";
        pill.ForeColor = color;
        pill.Invalidate();
    }

    private void ResizeContent(FlowLayoutPanel content, params Control[] controls)
    {
        var width = ContentWidth(Body);
        content.Width = width;
        foreach (var control in controls)
        {
            control.Width = width;
        }

        // Dynamically size metric cards to prevent wrapping
        var metricsRow = controls.Length > 1 ? controls[1] : null;
        if (metricsRow is FlowLayoutPanel row && row.Controls.Count > 0)
        {
            var gap = 12;
            var cardWidth = (row.ClientSize.Width - (row.Controls.Count - 1) * gap) / row.Controls.Count;
            foreach (Control card in row.Controls)
            {
                card.Width = Math.Max(160, cardWidth);
                card.Margin = new Padding(0, 0, gap, 0);
            }
        }
    }
}