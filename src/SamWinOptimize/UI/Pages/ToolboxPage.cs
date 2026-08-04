using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class ToolboxPage : AppPage
{
    private readonly CommandRunner _commandRunner;
    private readonly ReceiptStore _receiptStore;

    public ToolboxPage(CommandRunner commandRunner, ReceiptStore receiptStore)
        : base("\uE713", "\u7cfb\u7edf\u5de5\u5177\u7bb1", "\u628a\u5e38\u7528\u914d\u7f6e\u548c\u53ea\u8bfb\u8bca\u65ad\u653e\u5728\u4e00\u8d77\uff0c\u9700\u8981\u65f6\u518d\u6253\u5f00\uff0c\u4e0d\u4e3b\u52a8\u4fee\u6539\u3002")
    {
        _commandRunner = commandRunner;
        _receiptStore = receiptStore;

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

        content.Controls.Add(BuildSectionHeader("\u6253\u5f00 Windows \u8bbe\u7f6e", "\u76f4\u63a5\u6253\u5f00\u7cfb\u7edf\u539f\u751f\u914d\u7f6e\u9875\u9762\u3002"));
        var launchers = CreateGrid();
        launchers.Controls.Add(BuildLauncher("\uE895", "Windows \u66f4\u65b0", "\u68c0\u67e5\u66f4\u65b0\u3001\u6682\u505c\u66f4\u65b0\u3001\u67e5\u770b\u66f4\u65b0\u5386\u53f2\u3002", "\u6253\u5f00\u8bbe\u7f6e", "ms-settings:windowsupdate"));
        launchers.Controls.Add(BuildLauncher("\uEDA2", "\u5b58\u50a8\u611f\u77e5", "\u67e5\u770b\u78c1\u76d8\u5360\u7528\u548c\u5b58\u50a8\u611f\u77e5\u914d\u7f6e\u3002", "\u6253\u5f00\u8bbe\u7f6e", "ms-settings:storage"));
        launchers.Controls.Add(BuildLauncher("\uE968", "\u7f51\u7edc\u72b6\u6001", "\u67e5\u770b\u8fde\u63a5\u3001\u5c5e\u6027\u548c\u9ad8\u7ea7\u7f51\u7edc\u914d\u7f6e\u3002", "\u6253\u5f00\u8bbe\u7f6e", "ms-settings:network-status"));
        launchers.Controls.Add(BuildLauncher("\uE7F8", "\u542f\u52a8\u5e94\u7528", "\u7ba1\u7406\u767b\u5f55\u65f6\u81ea\u52a8\u8fd0\u884c\u7684\u5e94\u7528\u3002", "\u6253\u5f00\u8bbe\u7f6e", "ms-settings:startupapps"));
        launchers.Controls.Add(BuildLauncher("\uE770", "\u8bbe\u5907\u7ba1\u7406\u5668", "\u7ba1\u7406\u786c\u4ef6\u8bbe\u5907\u548c\u9a71\u52a8\u7a0b\u5e8f\u3002", "\u6253\u5f00\u5de5\u5177", "devmgmt.msc"));
        launchers.Controls.Add(BuildLauncher("\uE9D9", "\u7cfb\u7edf\u670d\u52a1", "\u67e5\u770b\u548c\u7ba1\u7406\u7cfb\u7edf\u670d\u52a1\u8fd0\u884c\u72b6\u6001\u3002", "\u6253\u5f00\u5de5\u5177", "services.msc"));
        content.Controls.Add(launchers);

        content.Controls.Add(BuildSectionHeader("\u53ea\u8bfb\u7cfb\u7edf\u8bca\u65ad", "\u9a8c\u8bc1\u7cfb\u7edf\u6587\u4ef6\u548c\u7ec4\u4ef6\u6620\u50cf\uff0c\u4e0d\u505a\u626b\u63cf\u4ee5\u5916\u7684\u81ea\u52a8\u4fee\u590d\u3002"));
        var diagnostics = CreateGrid();
        diagnostics.Controls.Add(BuildDiagnostic("\uE90F", "\u7cfb\u7edf\u6587\u4ef6\u9a8c\u8bc1", "\u8fd0\u884c SFC /verifyonly\uff0c\u53ea\u8bfb\u68c0\u67e5\u4e0d\u4fee\u590d\u7cfb\u7edf\u6587\u4ef6\u3002",
            "\u5f00\u59cb\u9a8c\u8bc1", "sfc /verifyonly", CommandShell.CommandPrompt, true));
        diagnostics.Controls.Add(BuildDiagnostic("\uE9CE", "\u7ec4\u4ef6\u6620\u50cf\u626b\u63cf", "\u8fd0\u884c DISM ScanHealth \u68c0\u67e5\u7ec4\u4ef6\u5b58\u50a8\u662f\u5426\u635f\u574f\u3002",
            "\u5f00\u59cb\u626b\u63cf", "DISM /Online /Cleanup-Image /ScanHealth", CommandShell.CommandPrompt, true));
        content.Controls.Add(diagnostics);

        void ResizeAll()
        {
            var width = ContentWidth(Body);
            content.Width = width;
            foreach (Control child in content.Controls)
            {
                child.Width = width;
            }
            ResizeCards(launchers, width);
            ResizeCards(diagnostics, width);
        }
        content.Resize += (_, _) => ResizeAll();
        Body.Resize += (_, _) => ResizeAll();
    }

    private static Panel BuildSectionHeader(string title, string description)
    {
        var header = new Panel
        {
            Width = 980,
            Height = 88,
            BackColor = Theme.Canvas,
            Margin = new Padding(0)
        };
        header.Controls.Add(new Label
        {
            Text = title,
            Font = Theme.DisplayFont(15.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(0, 8),
            BackColor = Color.Transparent
        });
        header.Controls.Add(new Label
        {
            Text = description,
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = true,
            Location = new Point(1, 44),
            BackColor = Color.Transparent
        });
        return header;
    }

    private static FlowLayoutPanel CreateGrid() => new()
    {
        Width = 980,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        WrapContents = true,
        BackColor = Theme.Canvas,
        Padding = new Padding(0),
        Margin = new Padding(0, 0, 0, 30)
    };

    private static SurfacePanel BuildLauncher(
        string glyph,
        string title,
        string description,
        string actionText,
        string target)
    {
        return BuildToolCard(glyph, title, description, actionText, () => SettingsLauncher.Open(target));
    }

    private SurfacePanel BuildDiagnostic(
        string glyph,
        string title,
        string description,
        string actionText,
        string command,
        CommandShell shell,
        bool requiresAdministrator)
    {
        return BuildToolCard(glyph, title, description, actionText, async () =>
        {
            var startedAt = DateTimeOffset.Now;
            var result = await _commandRunner.ExecuteAsync(title, title, command, shell, requiresAdministrator);
            await _receiptStore.AppendAsync(
                new ExecutionReceipt(Guid.NewGuid(), startedAt, "\u7cfb\u7edf\u8bca\u65ad", [result]));
            using var dialog = new ExecutionResultDialog(title, [result]);
            dialog.ShowDialog(this);
        });
    }

    private static SurfacePanel BuildToolCard(
        string glyph,
        string title,
        string description,
        string actionText,
        Action action)
    {
        var card = new SurfacePanel
        {
            Size = new Size(490, 200),
            Margin = new Padding(0, 0, 18, 18),
            Padding = new Padding(28),
            SurfaceStyle = SurfaceStyle.Raised,
            Hoverable = true,
            Radius = Theme.RadiusLg
        };
        var iconTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusMd,
            Size = new Size(56, 56),
            Location = new Point(28, 28),
            Padding = new Padding(1)
        };
        iconTile.Controls.Add(new Label
        {
            Text = glyph,
            Font = Theme.IconFont(18),
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });
        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.DisplayFont(13.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(102, 28),
            BackColor = Color.Transparent
        };
        var descriptionLabel = new Label
        {
            Text = description,
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(103, 64),
            Size = new Size(340, 50),
            BackColor = Color.Transparent
        };
        var button = new ActionButton
        {
            Text = actionText,
            Kind = ActionButtonKind.Secondary,
            Size = new Size(132, 46),
            Location = new Point(102, 132)
        };
        button.Click += (_, _) => action();
        card.Resize += (_, _) => descriptionLabel.Width = Math.Max(180, card.ClientSize.Width - 134);
        card.Controls.Add(iconTile);
        card.Controls.Add(titleLabel);
        card.Controls.Add(descriptionLabel);
        card.Controls.Add(button);
        return card;
    }

    private static void ResizeCards(FlowLayoutPanel flow, int available)
    {
        flow.Width = available;
        var cardWidth = available >= 840 ? (available - 18) / 2 : available;
        foreach (Control card in flow.Controls)
        {
            card.Width = cardWidth;
        }
    }
}
