using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI.Controls;

public sealed class StatusPill : Label
{
    public StatusPill(string text, Color color)
    {
        AutoSize = true;
        Text = $"  {text}  ";
        Font = Theme.Font(8.5f, FontStyle.Bold);
        ForeColor = color;
        BackColor = Color.Transparent;
        Padding = new Padding(10, 6, 10, 6);
        Margin = new Padding(0, 0, 10, 0);
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = Theme.RoundedRectangle(bounds, Theme.RadiusSm);
        using var fill = new SolidBrush(Color.FromArgb(34, ForeColor));
        using var border = new Pen(Color.FromArgb(100, ForeColor));
        eventArgs.Graphics.FillPath(fill, path);
        eventArgs.Graphics.DrawPath(border, path);
    }
}

public sealed class PageHeader : Panel
{
    private readonly Label _descriptionLabel;

    public PageHeader(string glyph, string title, string description)
    {
        Height = 146;
        Dock = DockStyle.Top;
        BackColor = Color.Transparent;
        Padding = new Padding(0, 0, 0, 30);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        var iconTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusXl,
            Size = new Size(70, 70),
            Location = new Point(0, 8),
            Padding = new Padding(1)
        };
        iconTile.Controls.Add(new Label
        {
            Text = glyph,
            Font = Theme.IconFont(22),
            ForeColor = Theme.AccentSoft,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });

        var eyebrow = new Label
        {
            Text = "SAM / CONTROL PLANE",
            Font = Theme.MonoFont(8, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            Location = new Point(96, 2),
            BackColor = Color.Transparent
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.DisplayFont(26, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(94, 20),
            BackColor = Color.Transparent
        };

        _descriptionLabel = new Label
        {
            Text = description,
            Font = Theme.Font(10.5f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(96, 66),
            Size = new Size(700, 34),
            BackColor = Color.Transparent
        };

        ActionHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 440,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 20, 0, 0),
            BackColor = Color.Transparent
        };

        Controls.Add(ActionHost);
        Controls.Add(eyebrow);
        Controls.Add(iconTile);
        Controls.Add(titleLabel);
        Controls.Add(_descriptionLabel);
        Resize += (_, _) =>
        {
            var rightLimit = Math.Max(260, Width - ActionHost.Width - 118);
            _descriptionLabel.Width = rightLimit;
        };
    }

    public FlowLayoutPanel ActionHost { get; }
}

public sealed class EmptyState : SurfacePanel
{
    public EmptyState(string glyph, string title, string description)
    {
        Height = 300;
        Dock = DockStyle.Top;
        SurfaceStyle = SurfaceStyle.Quiet;
        Padding = new Padding(48);

        var icon = new Label
        {
            Text = glyph,
            Font = Theme.IconFont(32),
            ForeColor = Theme.Accent,
            TextAlign = ContentAlignment.BottomCenter,
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.Transparent
        };
        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.DisplayFont(16, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = Color.Transparent
        };
        var descriptionLabel = new Label
        {
            Text = description,
            Font = Theme.Font(10.2f),
            ForeColor = Theme.TextSecondary,
            TextAlign = ContentAlignment.TopCenter,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(72, 0, 72, 28)
        };

        Controls.Add(descriptionLabel);
        Controls.Add(titleLabel);
        Controls.Add(icon);
    }
}
