using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace SamWinOptimize.UI.Controls;

/// <summary>
/// A single-pass, double-buffered text surface for the glass UI.
/// Standard transparent labels can repaint their parent between layout passes;
/// drawing the glyphs once through TextRenderer keeps the frame stable while
/// the surrounding glass surface is resized or scrolled.
/// </summary>
public class BufferedLabel : Label
{
    public BufferedLabel()
    {
        AutoSize = true;
        BackColor = Color.Transparent;
        UseCompatibleTextRendering = false;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        DoubleBuffered = true;
    }

    [DefaultValue(false)]
    public bool WordWrap { get; set; }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaintBackground(eventArgs);

        if (string.IsNullOrEmpty(Text) || ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        var multiline = Text.Contains('\n');
        var flags = TextFormatFlags.NoPrefix;
        if (!multiline && !WordWrap)
        {
            flags |= TextFormatFlags.SingleLine;
        }
        flags |= TextAlign switch
        {
            ContentAlignment.TopLeft => TextFormatFlags.Top | TextFormatFlags.Left,
            ContentAlignment.TopCenter => TextFormatFlags.Top | TextFormatFlags.HorizontalCenter,
            ContentAlignment.TopRight => TextFormatFlags.Top | TextFormatFlags.Right,
            ContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
            ContentAlignment.MiddleCenter => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
            ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter | TextFormatFlags.Right,
            ContentAlignment.BottomLeft => TextFormatFlags.Bottom | TextFormatFlags.Left,
            ContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter,
            ContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right,
            _ => TextFormatFlags.VerticalCenter | TextFormatFlags.Left
        };

        if (WordWrap && !AutoEllipsis)
        {
            flags |= TextFormatFlags.WordBreak;
        }
        else if (AutoEllipsis && !multiline)
        {
            flags |= TextFormatFlags.EndEllipsis;
        }

        var bounds = new Rectangle(
            Padding.Left,
            Padding.Top,
            Math.Max(1, ClientSize.Width - Padding.Horizontal),
            Math.Max(1, ClientSize.Height - Padding.Vertical));
        TextRenderer.DrawText(eventArgs.Graphics, Text, Font, bounds, Enabled ? ForeColor : SystemColors.GrayText, flags);
    }

    protected override void OnTextChanged(EventArgs eventArgs)
    {
        base.OnTextChanged(eventArgs);
        Invalidate();
    }

    protected override void OnFontChanged(EventArgs eventArgs)
    {
        base.OnFontChanged(eventArgs);
        Invalidate();
    }
}

public class BufferedPanel : Panel
{
    private const int WmEraseBkgnd = 0x0014;

    public BufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        // Let WinForms ask the painted parent for the transparent backdrop.
        // Clearing with the parent's BackColor would flatten a glass gradient
        // into a dark rectangle behind nested content.
        if (BackColor == Color.Transparent)
        {
            base.OnPaintBackground(eventArgs);
            return;
        }

        eventArgs.Graphics.Clear(BackColor);
    }

    protected override void WndProc(ref Message message)
    {
        // The buffered paint pass owns the background. Swallowing the separate
        // erase message prevents a stale intermediate frame from being copied
        // over the new sidebar layout while child controls are settling.
        if (message.Msg == WmEraseBkgnd)
        {
            message.Result = new IntPtr(1);
            return;
        }

        base.WndProc(ref message);
    }
}


public sealed class BackdropPanel : BufferedPanel
{
    [DefaultValue(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SidebarMode { get; set; }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        var graphics = eventArgs.Graphics;
        graphics.Clear(SidebarMode ? Theme.Sidebar : Theme.Canvas);

        using var hairline = new Pen(Theme.Border);
        if (SidebarMode)
        {
            graphics.DrawLine(hairline, Width - 1, 0, Width - 1, Height);
        }
        else
        {
            graphics.DrawLine(hairline, 0, 0, Width, 0);
        }
    }
}

public sealed class BufferedTableLayoutPanel : TableLayoutPanel
{
    public BufferedTableLayoutPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
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
}

public sealed class BufferedScrollablePanel : BufferedPanel
{
    protected override void OnScroll(ScrollEventArgs eventArgs)
    {
        base.OnScroll(eventArgs);

        // Do not use WS_EX_COMPOSITED for this container. Windows can retain the
        // previous child-window frame while AutoScroll moves nested controls. A
        // complete invalidation after the scroll gives every exposed pixel a
        // current paint pass without leaving card-shaped trails behind.
        Invalidate(invalidateChildren: true);
        Update();
    }
}

public sealed class BufferedFlowLayoutPanel : FlowLayoutPanel
{
    public BufferedFlowLayoutPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
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
}

public sealed class BufferedDataGridView : DataGridView
{
    private const int WmSetRedraw = 0x000B;

    public BufferedDataGridView()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
    }

    public void BeginUpdate()
    {
        if (IsHandleCreated)
        {
            _ = SendMessage(Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public void EndUpdate()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        _ = SendMessage(Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
        Invalidate(invalidateChildren: true);
        Update();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr windowHandle,
        int message,
        IntPtr wParam,
        IntPtr lParam);
}
