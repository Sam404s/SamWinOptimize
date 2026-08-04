using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI.Controls;

public sealed class StatusPill : BufferedLabel
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
    private readonly BufferedLabel _descriptionLabel;

    public PageHeader(string glyph, string title, string description)
    {
        Height = 136;
        Dock = DockStyle.Top;
        BackColor = Color.Transparent;
        Padding = new Padding(0, 0, 0, 30);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        var iconTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusXl,
            Size = new Size(64, 64),
            Location = new Point(0, 8),
            Padding = new Padding(1)
        };
        iconTile.Controls.Add(new BufferedLabel
        {
            Text = glyph,
            Font = Theme.IconFont(22),
            ForeColor = Theme.AccentSoft,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });

        var eyebrow = new BufferedLabel
        {
            Text = "SAM / CONTROL PLANE",
            Font = Theme.MonoFont(8, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            Location = new Point(96, 2),
            BackColor = Color.Transparent
        };

        var titleLabel = new BufferedLabel
        {
            Text = title,
            Font = Theme.DisplayFont(20.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = false,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(94, 16),
            Size = new Size(360, 42),
            BackColor = Color.Transparent
        };

        _descriptionLabel = new BufferedLabel
        {
            Text = description,
            Font = Theme.Font(10.5f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(96, 61),
            Size = new Size(700, 30),
            BackColor = Color.Transparent
        };

        ActionHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = 0,
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
        void ResizeTextBounds()
        {
            var rightLimit = Math.Max(220, Width - ActionHost.Width - 118);
            titleLabel.Width = rightLimit;
            _descriptionLabel.Width = rightLimit;
        }

        Resize += (_, _) => ResizeTextBounds();
        ActionHost.SizeChanged += (_, _) => ResizeTextBounds();
        ActionHost.ControlAdded += (_, _) => ResizeTextBounds();
        ActionHost.ControlRemoved += (_, _) => ResizeTextBounds();
        ResizeTextBounds();
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

        var icon = new BufferedLabel
        {
            Text = glyph,
            Font = Theme.IconFont(32),
            ForeColor = Theme.Accent,
            TextAlign = ContentAlignment.BottomCenter,
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.Transparent
        };
        var titleLabel = new BufferedLabel
        {
            Text = title,
            Font = Theme.DisplayFont(15, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.Transparent
        };
        var descriptionLabel = new BufferedLabel
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
