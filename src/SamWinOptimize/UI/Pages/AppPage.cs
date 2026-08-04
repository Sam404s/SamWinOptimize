using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public abstract class AppPage : UserControl
{
    protected AppPage(string glyph, string title, string description, bool allowBodyScroll = false)
    {
        Dock = DockStyle.Fill;
        AccessibleName = title;
        AccessibleDescription = description;
        BackColor = Theme.Canvas;
        Padding = new Padding(44, 34, 44, 38);

        Header = new PageHeader(glyph, title, description);
        Body = new BufferedPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = allowBodyScroll,
            BackColor = Theme.Canvas,
            Padding = new Padding(0, 6, allowBodyScroll ? 14 : 0, 0)
        };

        Controls.Add(Body);
        Controls.Add(Header);
    }

    protected PageHeader Header { get; }
    protected Panel Body { get; }

    protected static int ContentWidth(Panel body, int minimum = 720) =>
        Math.Max(minimum, body.ClientSize.Width - (body.AutoScroll ? SystemInformation.VerticalScrollBarWidth : 0) - 16);
}