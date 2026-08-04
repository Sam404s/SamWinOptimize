using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

internal static class SecurityPageLayout
{
    public static SurfacePanel BuildCard(string glyph, string title, string description, int height)
    {
        var card = new SurfacePanel
        {
            Width = 980,
            Height = height,
            Margin = new Padding(0, 0, 0, 24),
            Padding = Padding.Empty,
            SurfaceStyle = SurfaceStyle.Raised,
            Radius = Theme.RadiusLg
        };
        var iconTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusMd,
            Size = new Size(60, 60),
            Location = new Point(30, 30),
            Padding = new Padding(1)
        };
        iconTile.Controls.Add(new BufferedLabel
        {
            Text = glyph,
            Font = Theme.IconFont(19),
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });
        var titleLabel = new BufferedLabel
        {
            Text = title,
            Font = Theme.DisplayFont(14.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = false,
            AutoEllipsis = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(110, 24),
            Size = new Size(700, 38),
            BackColor = Color.Transparent
        };
        var descriptionLabel = new BufferedLabel
        {
            Text = description,
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = false,
            AutoEllipsis = false,
            WordWrap = true,
            TextAlign = ContentAlignment.TopLeft,
            Location = new Point(111, 62),
            Size = new Size(690, 46),
            BackColor = Color.Transparent
        };
        card.Controls.Add(iconTile);
        card.Controls.Add(titleLabel);
        card.Controls.Add(descriptionLabel);
        card.Resize += (_, _) =>
        {
            var textWidth = Math.Max(300, card.ClientSize.Width - 142);
            titleLabel.Width = textWidth;
            descriptionLabel.Width = textWidth;
        };
        return card;
    }

    public static BufferedLabel CreateStatusLabel(Point location, Size size) => new()
    {
        Text = "正在检测…",
        Font = Theme.Font(9.5f, FontStyle.Bold),
        ForeColor = Theme.Info,
        AutoSize = false,
        AutoEllipsis = false,
        WordWrap = true,
        TextAlign = ContentAlignment.TopLeft,
        Location = location,
        Size = size,
        BackColor = Color.Transparent
    };

    public static BufferedLabel CreateFixedStatus(string text, Color color)
    {
        var label = CreateStatusLabel(new Point(110, 114), new Size(430, 38));
        label.Text = text;
        label.ForeColor = color;
        return label;
    }

    public static void FitStatus(
        Control card,
        BufferedLabel status,
        int top,
        int reservedBottom = 22,
        int reservedRight = 0)
    {
        var width = Math.Max(300, card.ClientSize.Width - 142 - reservedRight);
        status.Location = new Point(110, top);
        status.Size = new Size(width, Math.Max(28, card.ClientSize.Height - top - reservedBottom));
    }

    public static void AddLauncherButton(Control card, string text, string target, int top)
    {
        var button = new ActionButton
        {
            Text = text,
            Kind = ActionButtonKind.Secondary,
            Size = new Size(190, 46),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        card.Resize += (_, _) =>
            button.Location = new Point(Math.Max(110, card.ClientSize.Width - button.Width - 30), top);
        button.Click += (_, _) => SettingsLauncher.Open(target);
        card.Controls.Add(button);
    }

    public static void ResizeCards(FlowLayoutPanel flow, Control body)
    {
        var width = ContentWidth(body);
        flow.Width = width;
        foreach (Control control in flow.Controls)
        {
            control.Width = width;
        }
    }

    private static int ContentWidth(Control body)
    {
        var scrollBar = body is ScrollableControl { AutoScroll: true }
            ? SystemInformation.VerticalScrollBarWidth
            : 0;
        return Math.Max(720, body.ClientSize.Width - scrollBar - 16);
    }
}
