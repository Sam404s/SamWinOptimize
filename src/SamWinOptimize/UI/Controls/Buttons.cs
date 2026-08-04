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
        BackColor = Theme.Canvas;
        Height = 48;
        Padding = new Padding(22, 0, 22, 0);
        Cursor = Cursors.Hand;
        TabStop = true;
        UseMnemonic = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

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
        // Clear stale rounded selection pixels before drawing the current state.
        eventArgs.Graphics.Clear(BackColor);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var pressedInset = _pressed ? 2 : 0;
        var bounds = new Rectangle(pressedInset, pressedInset, Width - 1 - (pressedInset * 2), Height - 1 - (pressedInset * 2));
        using var path = Theme.RoundedRectangle(bounds, Theme.RadiusMd);
        var background = ResolveBackground();
        using var brush = new SolidBrush(background);
        using var border = new Pen(ResolveBorder(background));
        eventArgs.Graphics.FillPath(brush, path);
        eventArgs.Graphics.DrawPath(border, path);

        var textColor = ResolveTextColor();
        TextRenderer.DrawText(eventArgs.Graphics, Text, Font, bounds,
            Enabled ? textColor : Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        if (Focused && ShowFocusCues)
        {
            var focusBounds = Rectangle.Inflate(bounds, -4, -4);
            using var focusPath = Theme.RoundedRectangle(focusBounds, Theme.RadiusSm);
            using var focusPen = new Pen(Kind == ActionButtonKind.Primary ? Color.White : Theme.Accent)
            {
                DashStyle = DashStyle.Dot
            };
            eventArgs.Graphics.DrawPath(focusPen, focusPath);
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
            return Theme.SurfaceRaised;
        }

        var normal = Kind switch
        {
            ActionButtonKind.Primary => Theme.Accent,
            ActionButtonKind.Danger => Theme.DangerSurface,
            _ => Theme.SurfaceRaised
        };
        var hovered = Kind switch
        {
            ActionButtonKind.Primary => Theme.Blend(Theme.Accent, Color.White, 0.16f),
            ActionButtonKind.Danger => Theme.Blend(Theme.DangerSurface, Theme.Danger, 0.26f),
            _ => Theme.SurfaceHover
        };
        var pressed = Kind switch
        {
            ActionButtonKind.Primary => Theme.AccentStrong,
            ActionButtonKind.Danger => Theme.Blend(Theme.DangerSurface, Color.Black, 0.18f),
            _ => Theme.SurfaceStrong
        };
        return _pressed ? pressed : Theme.Blend(normal, hovered, _hoverProgress);
    }

    private Color ResolveBorder(Color background) => Kind switch
    {
        ActionButtonKind.Secondary => _hoverProgress > 0.2f ? Theme.BorderStrong : Theme.Border,
        ActionButtonKind.Danger => Theme.Blend(background, Theme.Danger, 0.36f),
        _ => background
    };

    private Color ResolveTextColor() => Kind == ActionButtonKind.Primary
        ? Color.White
        : Theme.TextPrimary;
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
        AccessibleDescription = $"\u5bfc\u822a\u5230{label}";
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
    }

    public string Glyph { get; }
    public string Label { get; }

    [DefaultValue(false)]
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
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
        // Clear stale rounded selection pixels before drawing the current state.
        eventArgs.Graphics.Clear(BackColor);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var inset = _pressed ? 2 : 0;
        var bounds = new Rectangle(4 + inset, inset, Width - 8 - (inset * 2), Height - 1 - (inset * 2));

        if (Selected || _hovered || _pressed)
        {
            using var path = Theme.RoundedRectangle(bounds, Theme.RadiusMd);
            var fill = Selected
                ? Theme.AccentWash
                : _pressed ? Theme.SurfaceStrong : Theme.SurfaceHover;
            using var background = new SolidBrush(fill);
            eventArgs.Graphics.FillPath(background, path);

            if (Selected)
            {
                // Left accent indicator bar
                var barBounds = new Rectangle(bounds.Left + 2, bounds.Top + 14, 4, bounds.Height - 28);
                using var barPath = Theme.RoundedRectangle(barBounds, 2);
                using var barBrush = new SolidBrush(Theme.Accent);
                eventArgs.Graphics.FillPath(barBrush, barPath);
            }
        }

        using var iconFont = Theme.IconFont(14);
        var iconRect = new Rectangle(26, 0, 36, Height);
        TextRenderer.DrawText(eventArgs.Graphics, Glyph, iconFont, iconRect,
            Selected ? Theme.Accent : Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        using var labelFont = Theme.Font(10.5f, Selected ? FontStyle.Bold : FontStyle.Regular);
        var textRect = new Rectangle(76, 0, Width - 96, Height);
        TextRenderer.DrawText(eventArgs.Graphics, Label, labelFont, textRect,
            Selected ? Theme.TextPrimary : Theme.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);

        if (Focused && ShowFocusCues)
        {
            var focusBounds = Rectangle.Inflate(bounds, -4, -4);
            using var focusPath = Theme.RoundedRectangle(focusBounds, Theme.RadiusSm);
            using var focusPen = new Pen(Theme.Accent) { DashStyle = DashStyle.Dot };
            eventArgs.Graphics.DrawPath(focusPen, focusPath);
        }
    }
}