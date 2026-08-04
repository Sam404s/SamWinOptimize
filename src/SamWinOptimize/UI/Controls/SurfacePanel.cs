using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI.Controls;

public enum SurfaceStyle
{
    Default,
    Raised,
    Quiet,
    Accent,
    Danger
}

public class SurfacePanel : Panel
{
    private int _radius = Theme.RadiusLg;
    private bool _accentEdge;
    private bool _hoverable;
    private bool _hovered;
    private SurfaceStyle _surfaceStyle;

    public SurfacePanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.Surface;
        Padding = new Padding(1);
        Margin = new Padding(0);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
    }

    [DefaultValue(Theme.RadiusLg)]
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = Math.Max(6, value);
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool AccentEdge
    {
        get => _accentEdge;
        set
        {
            _accentEdge = value;
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool Hoverable
    {
        get => _hoverable;
        set
        {
            _hoverable = value;
            if (!value)
            {
                _hovered = false;
            }
            Invalidate();
        }
    }

    [DefaultValue(SurfaceStyle.Default)]
    public SurfaceStyle SurfaceStyle
    {
        get => _surfaceStyle;
        set
        {
            _surfaceStyle = value;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        if (Hoverable)
        {
            _hovered = true;
            Invalidate();
        }
        base.OnMouseEnter(eventArgs);
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        if (Hoverable && !ClientRectangle.Contains(PointToClient(Cursor.Position)))
        {
            _hovered = false;
            Invalidate();
        }
        base.OnMouseLeave(eventArgs);
    }

    protected override void OnControlAdded(ControlEventArgs eventArgs)
    {
        if (Hoverable && eventArgs.Control is { } control)
        {
            HookHover(control);
        }
        base.OnControlAdded(eventArgs);
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.Clear(ResolveParentBackColor());
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = Theme.RoundedRectangle(bounds, Radius);
        var fillColor = ResolveFill();
        var borderColor = _hovered ? Theme.BorderStrong : ResolveBorder();
        using var background = new SolidBrush(fillColor);
        using var border = new Pen(borderColor);
        eventArgs.Graphics.FillPath(background, path);
        eventArgs.Graphics.DrawPath(border, path);

        // Subtle inner highlight for depth
        var highlightBounds = new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3));
        using var highlightPath = Theme.RoundedRectangle(highlightBounds, Math.Max(6, Radius - 1));
        using var highlightPen = new Pen(Color.FromArgb(18, Color.White));
        eventArgs.Graphics.DrawPath(highlightPen, highlightPath);

        if (AccentEdge || SurfaceStyle == SurfaceStyle.Accent)
        {
            using var accentBorder = new Pen(Theme.AccentSoft, 1.5f);
            eventArgs.Graphics.DrawPath(accentBorder, path);
        }
    }

    private Color ResolveFill()
    {
        var fill = SurfaceStyle switch
        {
            SurfaceStyle.Raised => Theme.SurfaceRaised,
            SurfaceStyle.Quiet => Theme.CanvasSoft,
            SurfaceStyle.Accent => Theme.Surface,
            SurfaceStyle.Danger => Theme.Blend(Theme.Surface, Theme.DangerSurface, 0.42f),
            _ => Theme.Surface
        };
        return _hovered ? Theme.Blend(fill, Theme.SurfaceHover, 0.48f) : fill;
    }

    private Color ResolveBorder() => SurfaceStyle switch
    {
        SurfaceStyle.Accent => Theme.AccentSoft,
        SurfaceStyle.Danger => Theme.Blend(Theme.Border, Theme.Danger, 0.45f),
        SurfaceStyle.Raised => Theme.BorderStrong,
        _ => Theme.Border
    };

    private void HookHover(Control control)
    {
        control.MouseEnter += (_, _) =>
        {
            _hovered = true;
            Invalidate();
        };
        control.MouseLeave += (_, _) =>
        {
            if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                _hovered = false;
                Invalidate();
            }
        };
    }

    private Color ResolveParentBackColor()
    {
        for (var parent = Parent; parent is not null; parent = parent.Parent)
        {
            if (parent.BackColor != Color.Transparent)
            {
                return parent.BackColor;
            }
        }

        return Theme.Canvas;
    }
}