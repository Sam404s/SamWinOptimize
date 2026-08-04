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
            Padding = new Padding(30),
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
            AutoSize = true,
            Location = new Point(110, 28),
            BackColor = Color.Transparent
        };
        var descriptionLabel = new BufferedLabel
        {
            Text = description,
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(111, 62),
            Size = new Size(690, 36),
            BackColor = Color.Transparent
        };
        card.Controls.Add(iconTile);
        card.Controls.Add(titleLabel);
        card.Controls.Add(descriptionLabel);
        card.Resize += (_, _) => descriptionLabel.Width = Math.Max(320, card.ClientSize.Width - 340);
        return card;
    }

    public static BufferedLabel CreateStatusLabel(Point location, Size size) => new()
    {
        Text = "\u6b63\u5728\u68c0\u6d4b\u2026",
        Font = Theme.Font(9.5f, FontStyle.Bold),
        ForeColor = Theme.Info,
        AutoEllipsis = true,
        Location = location,
        Size = size,
        BackColor = Color.Transparent
    };

    public static Label CreateFixedStatus(string text, Color color)
    {
        var label = CreateStatusLabel(new Point(110, 114), new Size(620, 32));
        label.Text = text;
        label.ForeColor = color;
        return label;
    }

    public static void AddLauncherButton(Control card, string text, string target, int top)
    {
        var button = new ActionButton
        {
            Text = text,
            Kind = ActionButtonKind.Secondary,
            Size = new Size(190, 48),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        card.Resize += (_, _) =>
            button.Location = new Point(card.ClientSize.Width - 224, top);
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
