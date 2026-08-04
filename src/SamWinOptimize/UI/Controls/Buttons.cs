using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI.Controls;

public enum ActionButtonKind
{
    Primary,
    Secondary,
    Danger
}

public sealed class ActionButton : Button
{
    private readonly System.Windows.Forms.Timer _transitionTimer;
    private bool _hovered;
    private bool _pressed;
    private float _hoverProgress;
    private ActionButtonKind _kind;

    public ActionButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = Theme.Font(10, FontStyle.Bold);
        ForeColor = Theme.TextPrimary;
        BackColor = Color.Transparent;
        Height = 48;
        Padding = new Padding(22, 0, 22, 0);
        Cursor = Cursors.Hand;
        TabStop = true;
        UseMnemonic = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        UpdateStyles();
        Text = string.Empty;

        _transitionTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _transitionTimer.Tick += (_, _) => AdvanceTransition();
    }

    [DefaultValue(ActionButtonKind.Primary)]
    public ActionButtonKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            Invalidate();
        }
    }

    protected override void OnEnabledChanged(EventArgs eventArgs)
    {
        Invalidate();
        base.OnEnabledChanged(eventArgs);
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        _hovered = true;
        _transitionTimer.Start();
        base.OnMouseEnter(eventArgs);
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        _hovered = false;
        _pressed = false;
        _transitionTimer.Start();
        base.OnMouseLeave(eventArgs);
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
        base.OnMouseDown(eventArgs);
    }

    protected override void OnMouseUp(MouseEventArgs eventArgs)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(eventArgs);
    }

    protected override void OnGotFocus(EventArgs eventArgs)
    {
        Invalidate();
        base.OnGotFocus(eventArgs);
    }

    protected override void OnLostFocus(EventArgs eventArgs)
    {
        Invalidate();
        base.OnLostFocus(eventArgs);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        // Preserve the glass surface painted by the parent. A solid canvas
        // clear here creates dark rectangles around buttons placed on cards.
        if (BackColor == Color.Transparent)
        {
            base.OnPaintBackground(eventArgs);
            return;
        }

        eventArgs.Graphics.Clear(BackColor);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        var graphics = eventArgs.Graphics;
        // Repaint the current parent backdrop before drawing the button state.
        // This keeps hover/pressed transitions single-pass and prevents stale
        // glyphs or previous glow frames from surviving an invalidation.
        if (BackColor == Color.Transparent)
        {
            base.OnPaintBackground(eventArgs);
        }
        else
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.Clear(BackColor);
            graphics.CompositingMode = CompositingMode.SourceOver;
        }
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var pressedInset = _pressed ? 2 : 0;
        var bounds = new Rectangle(
            pressedInset,
            pressedInset,
            Math.Max(1, Width - 1 - (pressedInset * 2)),
            Math.Max(1, Height - 1 - (pressedInset * 2)));
        using var path = Theme.RoundedRectangle(bounds, Theme.RadiusMd);
        var background = ResolveBackground();

        using var brush = new SolidBrush(background);
        graphics.FillPath(brush, path);

        using var border = new Pen(ResolveBorder(background), Kind == ActionButtonKind.Primary ? 1f : 1.1f);
        graphics.DrawPath(border, path);

        var textColor = ResolveTextColor();
        TextRenderer.DrawText(graphics, Text, Font, bounds,
            Enabled ? textColor : Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        if (Focused && ShowFocusCues)
        {
            var focusBounds = Rectangle.Inflate(bounds, -4, -4);
            using var focusPath = Theme.RoundedRectangle(focusBounds, Theme.RadiusSm);
            using var focusPen = new Pen(
                Color.FromArgb(190, Kind == ActionButtonKind.Primary ? Color.White : Theme.Accent), 1f);
            graphics.DrawPath(focusPen, focusPath);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transitionTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    private void AdvanceTransition()
    {
        var target = _hovered ? 1f : 0f;
        _hoverProgress += (target - _hoverProgress) * 0.34f;
        if (Math.Abs(target - _hoverProgress) < 0.025f)
        {
            _hoverProgress = target;
            _transitionTimer.Stop();
        }
        Invalidate();
    }

    private Color ResolveBackground()
    {
        if (!Enabled)
        {
            return Theme.CanvasRaised;
        }

        var normal = Kind switch
        {
            ActionButtonKind.Primary => Theme.AccentStrong,
            ActionButtonKind.Danger => Theme.DangerSurface,
            _ => Theme.SurfaceRaised
        };
        var hovered = Kind switch
        {
            ActionButtonKind.Primary => Theme.Blend(Theme.Accent, Color.White, 0.08f),
            ActionButtonKind.Danger => Theme.Blend(Theme.DangerSurface, Theme.Danger, 0.26f),
            _ => Theme.SurfaceHover
        };
        var pressed = Kind switch
        {
            ActionButtonKind.Primary => Theme.AccentDeep,
            ActionButtonKind.Danger => Theme.Blend(Theme.DangerSurface, Color.Black, 0.18f),
            _ => Theme.SurfaceStrong
        };
        return _pressed ? pressed : Theme.Blend(normal, hovered, _hoverProgress);
    }

    private Color ResolveBorder(Color background) => Kind switch
    {
        ActionButtonKind.Primary => Theme.AccentSoft,
        ActionButtonKind.Secondary => _hoverProgress > 0.2f ? Theme.BorderStrong : Theme.Border,
        ActionButtonKind.Danger => Theme.Blend(background, Theme.Danger, 0.62f),
        _ => background
    };

    private Color ResolveTextColor() => Kind switch
    {
        ActionButtonKind.Primary => Color.White,
            ActionButtonKind.Danger => Theme.Danger,
            _ => Theme.TextPrimary
    };
}

public sealed class SelectionCheckBox : CheckBox
{
    private bool _hovered;
    private bool _pressed;

    public SelectionCheckBox()
    {
        AutoSize = false;
        Appearance = Appearance.Normal;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Text = string.Empty;
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        TabStop = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        UpdateStyles();
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(eventArgs);
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(eventArgs);
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
        base.OnMouseDown(eventArgs);
    }

    protected override void OnMouseUp(MouseEventArgs eventArgs)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(eventArgs);
    }

    protected override void OnCheckedChanged(EventArgs eventArgs)
    {
        Invalidate();
        base.OnCheckedChanged(eventArgs);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        if (BackColor == Color.Transparent)
        {
            base.OnPaintBackground(eventArgs);
            return;
        }

        eventArgs.Graphics.Clear(BackColor);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        var graphics = eventArgs.Graphics;
        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var size = Math.Min(22, Math.Max(16, Math.Min(ClientSize.Width, ClientSize.Height) - 8));
        var bounds = new Rectangle(
            Math.Max(2, (ClientSize.Width - size) / 2),
            Math.Max(2, (ClientSize.Height - size) / 2),
            size,
            size);
        using var path = Theme.RoundedRectangle(bounds, 5);
        var fill = Checked
            ? (_pressed ? Theme.AccentDeep : Theme.AccentStrong)
            : (_hovered ? Theme.SurfaceHover : Theme.SurfaceRaised);
        using var brush = new SolidBrush(fill);
        graphics.FillPath(brush, path);
        using var border = new Pen(Checked || _hovered ? Theme.Accent : Theme.BorderStrong, 1f);
        graphics.DrawPath(border, path);

        if (Checked)
        {
            using var tick = new Pen(Color.White, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            graphics.DrawLines(tick,
            [
                new Point(bounds.Left + 5, bounds.Top + bounds.Height / 2),
                new Point(bounds.Left + 9, bounds.Bottom - 5),
                new Point(bounds.Right - 4, bounds.Top + 5)
            ]);
        }

        if (Focused && ShowFocusCues)
        {
            using var focusPen = new Pen(Color.FromArgb(190, Theme.AccentSoft), 1f);
            graphics.DrawPath(focusPen, Theme.RoundedRectangle(Rectangle.Inflate(bounds, 3, 3), 7));
        }
    }
}

public sealed class NavButton : Button
{
    private bool _selected;
    private bool _hovered;
    private bool _pressed;

    public NavButton(string glyph, string label)
    {
        Glyph = glyph;
        Label = label;
        Text = label;
        AccessibleName = label;
        AccessibleDescription = $"导航到{label}";
        AccessibleRole = AccessibleRole.PushButton;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Theme.Sidebar;
        Height = 64;
        Dock = DockStyle.Top;
        Cursor = Cursors.Hand;
        TabStop = true;
        Margin = new Padding(0, 0, 0, 14);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        UpdateStyles();
        // NavButton paints its own label through TextRenderer. Keeping Button.Text
        // empty prevents the native Button paint path from contributing a second
        // glyph layer during focus, resize, or selection transitions.
        Text = string.Empty;
    }

    public string Glyph { get; }
    public string Label { get; }

    [DefaultValue(false)]
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value)
            {
                return;
            }

            _selected = value;
            Invalidate();
            Update();
        }
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(eventArgs);
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(eventArgs);
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
        base.OnMouseDown(eventArgs);
    }

    protected override void OnMouseUp(MouseEventArgs eventArgs)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(eventArgs);
    }

    protected override void OnGotFocus(EventArgs eventArgs)
    {
        Invalidate();
        base.OnGotFocus(eventArgs);
    }

    protected override void OnLostFocus(EventArgs eventArgs)
    {
        Invalidate();
        base.OnLostFocus(eventArgs);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.Clear(BackColor);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        var graphics = eventArgs.Graphics;
        // Clear inside the paint pass as well as OnPaintBackground. This makes
        // selection changes deterministic even when Windows coalesces the erase
        // message for a custom-painted Button.
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.Clear(BackColor);
        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var inset = _pressed ? 2 : 0;
        var bounds = new Rectangle(4 + inset, inset,
            Math.Max(1, Width - 8 - (inset * 2)), Math.Max(1, Height - 1 - (inset * 2)));

        if (Selected || _hovered || _pressed)
        {
            using var path = Theme.RoundedRectangle(bounds, Theme.RadiusMd);
            var fill = Selected ? Theme.AccentWash : Theme.SurfaceHover;
            using var brush = new SolidBrush(fill);
            graphics.FillPath(brush, path);
            using var border = new Pen(Selected ? Theme.Accent : Theme.BorderStrong);
            graphics.DrawPath(border, path);

            if (Selected)
            {
                var barBounds = new Rectangle(bounds.Left + 2, bounds.Top + 12, 4, Math.Max(8, bounds.Height - 24));
                using var barPath = Theme.RoundedRectangle(barBounds, 2);
                using var barBrush = new SolidBrush(Theme.Accent);
                graphics.FillPath(barBrush, barPath);
            }
        }

        using var iconFont = Theme.IconFont(15);
        var iconRect = new Rectangle(26, 0, 38, Height);
        TextRenderer.DrawText(graphics, Glyph, iconFont, iconRect,
            Selected ? Theme.Accent : Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        using var labelFont = Theme.Font(10.5f, Selected ? FontStyle.Bold : FontStyle.Regular);
        var textRect = new Rectangle(78, 0, Math.Max(80, Width - 100), Height);
        TextRenderer.DrawText(graphics, Label, labelFont, textRect,
            Selected ? Theme.TextPrimary : Theme.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        if (Focused && ShowFocusCues)
        {
            var focusBounds = Rectangle.Inflate(bounds, -4, -4);
            using var focusPath = Theme.RoundedRectangle(focusBounds, Theme.RadiusSm);
            using var focusPen = new Pen(Color.FromArgb(190, Theme.Accent), 1f);
            graphics.DrawPath(focusPen, focusPath);
        }
    }
}
