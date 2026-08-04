using System.Runtime.InteropServices;

namespace SamWinOptimize.UI.Controls;

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
        // Clear the complete surface in one pass. The default Panel background
        // path can leave the previous child-window frame visible during the
        // first layout pass, which is the source of the sidebar ghosting.
        var background = BackColor;
        if (background == Color.Transparent && Parent is not null)
        {
            background = Parent.BackColor;
        }

        eventArgs.Graphics.Clear(background);
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