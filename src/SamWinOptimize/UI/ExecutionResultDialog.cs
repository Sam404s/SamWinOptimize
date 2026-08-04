using SamWinOptimize.Models;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI;

public sealed class ExecutionResultDialog : Form
{
    public ExecutionResultDialog(string title, IReadOnlyList<CommandResult> results)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(880, 640);
        MinimumSize = new Size(720, 540);
        BackColor = Theme.Canvas;
        ForeColor = Theme.TextPrimary;
        Font = Theme.Font(9.5f);
        FormBorderStyle = FormBorderStyle.Sizable;
        ShowIcon = false;
        Padding = new Padding(32, 28, 32, 28);

        var successCount = results.Count(result => result.Succeeded);
        var allSucceeded = successCount == results.Count;
        var header = new SurfacePanel
        {
            Dock = DockStyle.Top,
            Height = 132,
            Padding = new Padding(28),
            SurfaceStyle = allSucceeded ? SurfaceStyle.Accent : SurfaceStyle.Danger,
            Radius = Theme.RadiusLg
        };
        var iconTile = new SurfacePanel
        {
            SurfaceStyle = allSucceeded ? SurfaceStyle.Accent : SurfaceStyle.Danger,
            Radius = Theme.RadiusMd,
            Size = new Size(56, 56),
            Location = new Point(28, 28),
            Padding = new Padding(1)
        };
        iconTile.Controls.Add(new Label
        {
            Text = allSucceeded ? "\uE73E" : "\uE7BA",
            Font = Theme.IconFont(18),
            ForeColor = allSucceeded ? Theme.Success : Theme.Warning,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });
        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.DisplayFont(19, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(102, 26),
            BackColor = Color.Transparent
        };
        var summaryLabel = new Label
        {
            Text = $"{successCount}/{results.Count} \u9879\u6210\u529f  \u00b7  \u6bcf\u9879\u7ed3\u679c\u5df2\u5199\u5165\u672c\u5730\u6267\u884c\u8bb0\u5f55",
            Font = Theme.Font(9.5f, FontStyle.Bold),
            ForeColor = allSucceeded ? Theme.Success : Theme.Warning,
            AutoSize = true,
            Location = new Point(104, 64),
            BackColor = Color.Transparent
        };
        var hintLabel = new Label
        {
            Text = "\u672c\u5730\u4fdd\u5b58\u5b8c\u6574\u9000\u51fa\u7801\u3001\u8f93\u51fa\u4f7f\u7528\u4fe1\u606f\uff0c\u7528\u4e8e\u590d\u6838\u4e0e\u590d\u5ba1\u3002",
            Font = Theme.Font(9),
            ForeColor = Theme.TextSecondary,
            AutoSize = true,
            Location = new Point(104, 92),
            BackColor = Color.Transparent
        };
        header.Controls.Add(iconTile);
        header.Controls.Add(titleLabel);
        header.Controls.Add(summaryLabel);
        header.Controls.Add(hintLabel);

        var outputHost = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            SurfaceStyle = SurfaceStyle.Raised,
            Radius = Theme.RadiusLg
        };
        var output = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Theme.SurfaceRaised,
            ForeColor = Theme.TextSecondary,
            Font = Theme.MonoFont(9.2f),
            DetectUrls = false,
            Text = FormatResults(results)
        };
        outputHost.Controls.Add(output);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 82,
            Padding = new Padding(0, 22, 0, 10),
            BackColor = Theme.Canvas
        };
        var closeButton = new ActionButton
        {
            Text = "\u5173\u95ed",
            Kind = ActionButtonKind.Primary,
            Dock = DockStyle.Right,
            Width = 128,
            DialogResult = DialogResult.OK
        };
        footer.Controls.Add(closeButton);

        var spacer = new Panel
        {
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Theme.Canvas
        };
        Controls.Add(outputHost);
        Controls.Add(footer);
        Controls.Add(spacer);
        Controls.Add(header);
        AcceptButton = closeButton;
    }

    private static string FormatResults(IEnumerable<CommandResult> results)
    {
        var lines = new List<string>();
        foreach (var result in results)
        {
            lines.Add($"[{(result.Succeeded ? "OK" : "FAIL")}] {result.Title}");
            lines.Add($"ExitCode: {result.ExitCode}  \u00b7  {result.CompletedAt:yyyy-MM-dd HH:mm:ss}");
            if (!string.IsNullOrWhiteSpace(result.Output))
            {
                lines.Add(result.Output);
            }
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                lines.Add($"Error: {result.Error}");
            }
            lines.Add(new string('\u2500', 78));
        }
        return string.Join(Environment.NewLine, lines);
    }
}