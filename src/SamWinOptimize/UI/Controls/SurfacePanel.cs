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

/// <summary>
/// Reusable glass surface. The control intentionally uses painted depth rather than
/// opaque nested panels so the whole shell reads as one atmospheric workspace.
/// </summary>
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

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        var graphics = eventArgs.Graphics;
        graphics.Clear(ResolveParentBackColor());
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.CompositingMode = CompositingMode.SourceOver;

        var bounds = new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3));
        using var path = Theme.RoundedRectangle(bounds, Radius);

        // A compact shadow makes the panels float without looking like generic cards.
        var shadowBounds = new Rectangle(bounds.X + 1, bounds.Y + 5, bounds.Width, bounds.Height);
        using var shadowPath = Theme.RoundedRectangle(shadowBounds, Radius);
        using var shadowBrush = new SolidBrush(Theme.Shadow);
        graphics.FillPath(shadowBrush, shadowPath);

        var fill = ResolveFill();
        graphics.SetClip(path);
        using (var glassBrush = new LinearGradientBrush(
                   bounds,
                   Theme.Blend(fill, Theme.GlassTop, 0.26f),
                   Theme.Blend(fill, Theme.GlassBottom, 0.34f),
                   90f))
        {
            graphics.FillRectangle(glassBrush, bounds);
        }

        // Diffuse cyan bloom: a painted approximation of frosted glass catching light.
        if (AccentEdge || SurfaceStyle == SurfaceStyle.Accent || _hovered)
        {
            var glowBounds = new Rectangle(
                Math.Max(-bounds.Width / 2, bounds.Right - 170),
                -80,
                260,
                220);
            using var glowPath = new GraphicsPath();
            glowPath.AddEllipse(glowBounds);
            using var glowBrush = new PathGradientBrush(glowPath)
            {
                CenterColor = Color.FromArgb(_hovered ? 72 : 48, Theme.Accent),
                SurroundColors = [Color.FromArgb(0, Theme.Accent)]
            };
            graphics.FillPath(glowBrush, glowPath);
        }
        graphics.ResetClip();

        var borderColor = _hovered ? Theme.BorderGlow : ResolveBorder();
        using var border = new Pen(Color.FromArgb(_hovered ? 210 : 165, borderColor), _hovered ? 1.25f : 1f);
        graphics.DrawPath(border, path);

        var highlightBounds = new Rectangle(bounds.X + 1, bounds.Y + 1,
            Math.Max(1, bounds.Width - 2), Math.Max(1, bounds.Height - 2));
        using var highlightPath = Theme.RoundedRectangle(highlightBounds, Math.Max(6, Radius - 1));
        using var highlightPen = new Pen(Color.FromArgb(38, Color.White), 1f);
        graphics.DrawPath(highlightPen, highlightPath);

        if (AccentEdge || SurfaceStyle == SurfaceStyle.Accent)
        {
            using var accentPen = new Pen(Color.FromArgb(205, Theme.AccentSoft), 1.35f);
            graphics.DrawPath(accentPen, path);
        }
    }

    private Color ResolveFill()
    {
        var fill = SurfaceStyle switch
        {
            SurfaceStyle.Raised => Theme.SurfaceRaised,
            SurfaceStyle.Quiet => Theme.CanvasSoft,
            SurfaceStyle.Accent => Theme.AccentWash,
            SurfaceStyle.Danger => Theme.Blend(Theme.Surface, Theme.DangerSurface, 0.62f),
            _ => Theme.Surface
        };
        return _hovered ? Theme.Blend(fill, Theme.SurfaceHover, 0.34f) : fill;
    }

    private Color ResolveBorder() => SurfaceStyle switch
    {
        SurfaceStyle.Accent => Theme.AccentSoft,
        SurfaceStyle.Danger => Theme.Blend(Theme.Border, Theme.Danger, 0.65f),
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
