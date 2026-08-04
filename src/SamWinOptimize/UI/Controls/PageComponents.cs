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
        base.OnPaintBackground(eventArgs);
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = Theme.RoundedRectangle(bounds, Theme.RadiusSm);
        using var fill = new SolidBrush(Color.FromArgb(30, ForeColor));
        using var border = new Pen(Color.FromArgb(80, ForeColor));
        eventArgs.Graphics.FillPath(fill, path);
        eventArgs.Graphics.DrawPath(border, path);
    }
}

public sealed class PageHeader : Panel
{
    private readonly Label _descriptionLabel;

    public PageHeader(string glyph, string title, string description)
    {
        Height = 124;
        Dock = DockStyle.Top;
        BackColor = Theme.Canvas;
        Padding = new Padding(0, 0, 0, 28);

        var iconTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Raised,
            Radius = Theme.RadiusLg,
            Size = new Size(62, 62),
            Location = new Point(0, 4),
            Padding = new Padding(1)
        };
        iconTile.Controls.Add(new Label
        {
            Text = glyph,
            Font = Theme.IconFont(20),
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });

        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.DisplayFont(23, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(84, 2),
            BackColor = Color.Transparent
        };

        _descriptionLabel = new Label
        {
            Text = description,
            Font = Theme.Font(10.2f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(86, 46),
            Size = new Size(700, 32),
            BackColor = Color.Transparent
        };

        ActionHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 440,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 0),
            BackColor = Theme.Canvas
        };

        Controls.Add(ActionHost);
        Controls.Add(iconTile);
        Controls.Add(titleLabel);
        Controls.Add(_descriptionLabel);
        Resize += (_, _) =>
        {
            var rightLimit = Math.Max(260, Width - ActionHost.Width - 108);
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