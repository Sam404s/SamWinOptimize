using System.Reflection;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class AboutPage : AppPage
{
    public AboutPage()
        : base("\uE946", "\u5173\u4e8e SamWinOptimize", "\u4e00\u5957\u6709\u6001\u5ea6\u7684\u5de5\u5177\uff0c\u4e5f\u662f\u503c\u5f97\u4fe1\u8d56\u7684 Windows \u65e5\u5e38\u7ef4\u62a4\u4f19\u4f34\u3002", allowBodyScroll: true)
    {
        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.Canvas
        };
        Body.Controls.Add(content);

        var hero = BuildHero();
        var principles = BuildPrinciplesPanel();
        var migration = BuildMigrationPanel();
        content.Controls.Add(hero);
        content.Controls.Add(principles);
        content.Controls.Add(migration);

        void ResizeAll()
        {
            var width = ContentWidth(Body);
            content.Width = width;
            hero.Width = width;
            principles.Width = width;
            migration.Width = width;
        }
        content.Resize += (_, _) => ResizeAll();
        Body.Resize += (_, _) => ResizeAll();
    }

    private static SurfacePanel BuildHero()
    {
        var hero = new SurfacePanel
        {
            Width = 980,
            Height = 296,
            SurfaceStyle = SurfaceStyle.Accent,
            Padding = new Padding(36),
            Margin = new Padding(0, 0, 0, 28)
        };
        var markTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusXl,
            Size = new Size(84, 84),
            Location = new Point(36, 38),
            Padding = new Padding(1)
        };
        markTile.Controls.Add(new BufferedLabel
        {
            Text = "S",
            Font = Theme.DisplayFont(31, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Theme.Accent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });
        var title = new BufferedLabel
        {
            Text = "SamWinOptimize",
            Font = Theme.DisplayFont(26, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = false,
            AutoEllipsis = true,
            Location = new Point(144, 28),
            Size = new Size(700, 58),
            BackColor = Color.Transparent
        };
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        var metadata = new BufferedLabel
        {
            Text = $"VERSION {version}  /  .NET {Environment.Version.Major}  /  WINDOWS X64 \u00b7 ARM64",
            Font = Theme.MonoFont(8.2f, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = false,
            AutoEllipsis = true,
            Location = new Point(146, 98),
            Size = new Size(700, 22),
            BackColor = Color.Transparent
        };
        var description = new BufferedLabel
        {
            Text = "\u7b80\u5355\u4e0d\u610f\u5473\u7740\u7c97\u7cd9\u3002\u6bcf\u4e2a\u7cfb\u7edf\u64cd\u4f5c\u90fd\u6709\u660e\u786e\u610f\u56fe\u3001\u98ce\u9669\u7b49\u7ea7\u3001\u7ba1\u7406\u5458\u8fb9\u754c\u548c\u9000\u51fa\u51b2\u7a81\u539f\u8def\u5f84\u3002",
            Font = Theme.Font(10.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = false,
            WordWrap = true,
            Location = new Point(38, 152),
            Size = new Size(840, 68),
            BackColor = Color.Transparent
        };
        var promise = new BufferedLabel
        {
            Text = "\u672c\u5730\u8fd0\u884c  \u00b7  \u4e0d\u6267\u884c\u8fdc\u7a0b\u811a\u672c  \u00b7  \u6bcf\u4e00\u6b65\u53ef\u8bb0\u5f55",
            Font = Theme.Font(9.5f, FontStyle.Bold),
            ForeColor = Theme.Success,
            AutoSize = true,
            Location = new Point(39, 238),
            BackColor = Color.Transparent
        };
        hero.Controls.Add(markTile);
        hero.Controls.Add(title);
        hero.Controls.Add(metadata);
        hero.Controls.Add(description);
        hero.Controls.Add(promise);
        hero.Resize += (_, _) =>
        {
            var textWidth = Math.Max(420, hero.ClientSize.Width - 76);
            title.Width = Math.Max(240, hero.ClientSize.Width - 180);
            metadata.Width = Math.Max(240, hero.ClientSize.Width - 184);
            description.Width = textWidth;
        };
        return hero;
    }

    private static SurfacePanel BuildPrinciplesPanel()
    {
        var panel = new SurfacePanel
        {
            Width = 980,
            Height = 308,
            Padding = new Padding(32),
            SurfaceStyle = SurfaceStyle.Raised,
            Margin = new Padding(0, 0, 0, 28)
        };
        panel.Controls.Add(CreateTitle("\u4ea7\u54c1\u539f\u5219", 28));
        panel.Controls.Add(new BufferedLabel
        {
            Text = "\u514b\u5236\u7684\u6743\u9650\u3001\u900f\u660e\u7684\u6267\u884c\u3001\u6e05\u6670\u7684\u8fb9\u754c\u3002",
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = true,
            Location = new Point(33, 63),
            BackColor = Color.Transparent
        });
        var rows = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 1,
            Location = new Point(32, 106),
            Size = new Size(920, 172),
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        for (var index = 0; index < 3; index++)
        {
            rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        }
        rows.Controls.Add(CreatePrinciple("01", "\u6700\u5c0f\u6743\u9650", "\u5e94\u7528\u9ed8\u8ba4\u4ee5\u5f53\u524d\u7528\u6237\u8fd0\u884c\uff0c\u53ea\u5728\u786e\u8ba4\u540e\u7cfb\u7edf\u64cd\u4f5c\u624d\u5f39\u51fa UAC\u3002"), 0, 0);
        rows.Controls.Add(CreatePrinciple("02", "\u53ef\u8bb0\u5f55", "\u9000\u51fa\u7801\u3001\u8f93\u51fa\u3001\u5931\u8d25\u539f\u56e0\u90fd\u4f1a\u5199\u5165\u672c\u5730\u8bb0\u5f55\uff0c\u968f\u65f6\u53ef\u67e5\u3002"), 1, 0);
        rows.Controls.Add(CreatePrinciple("03", "\u5b89\u5168\u8fb9\u754c", "\u4e0d\u78b0\u9632\u75c5\u6bd2\u7cfb\u7edf\u5b89\u5168\u914d\u7f6e\uff0c\u4e0d\u5728\u786e\u5b9a\u8fb9\u754c\u5185\u505a\u6b63\u5f0f\u4fee\u6539\u3002"), 2, 0);
        panel.Controls.Add(rows);
        panel.Resize += (_, _) => rows.Width = Math.Max(620, panel.ClientSize.Width - 64);
        return panel;
    }

    private static SurfacePanel BuildMigrationPanel()
    {
        var panel = new SurfacePanel
        {
            Width = 980,
            Height = 240,
            Padding = new Padding(32),
            SurfaceStyle = SurfaceStyle.Quiet,
            Margin = new Padding(0, 0, 0, 20)
        };
        panel.Controls.Add(CreateTitle("\u4ece\u811a\u672c\u5230\u7ef4\u62a4\u4f19\u4f34", 28));
        var text = new BufferedLabel
        {
            Text = "\u7cfb\u7edf\u4f18\u5316\u3001\u7a7a\u95f4\u6e05\u7406\u3001Appx \u7ba1\u7406\u3001\u72b6\u6001\u68c0\u67e5\u3001\u8bb0\u5f55\u8fd8\u539f\u4e0e\u5de5\u5177\u7bb1\u516d\u5927\u6a21\u5757\uff0c\u8986\u76d6\u65e5\u5e38\u7ef4\u62a4\u7684\u5b8c\u6574\u94fe\u8def\u3002\u6bcf\u4e2a\u64cd\u4f5c\u90fd\u7ecf\u8fc7\u900f\u660e\u5ba1\u67e5\u3002",
            Font = Theme.Font(10),
            ForeColor = Theme.TextSecondary,
            Location = new Point(33, 72),
            Size = new Size(850, 74),
            BackColor = Color.Transparent
        };
        var docsButton = new ActionButton
        {
            Text = "\u6253\u5f00\u9879\u76ee\u76ee\u5f55",
            Kind = ActionButtonKind.Secondary,
            Size = new Size(152, 48),
            Location = new Point(32, 164)
        };
        docsButton.Click += (_, _) => SettingsLauncher.Open(AppContext.BaseDirectory);
        panel.Controls.Add(text);
        panel.Controls.Add(docsButton);
        panel.Resize += (_, _) => text.Width = Math.Max(520, panel.ClientSize.Width - 66);
        return panel;
    }

    private static BufferedLabel CreateTitle(string text, int top) => new()
    {
        Text = text,
        Font = Theme.DisplayFont(15.5f, FontStyle.Bold),
        ForeColor = Theme.TextPrimary,
        AutoSize = true,
        Location = new Point(32, top),
        BackColor = Color.Transparent
    };

    private static SurfacePanel CreatePrinciple(string index, string title, string description)
    {
        var card = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 16, 0),
            Padding = new Padding(22),
            SurfaceStyle = SurfaceStyle.Quiet,
            Radius = Theme.RadiusLg
        };
        card.Controls.Add(new BufferedLabel
        {
            Text = index,
            Font = Theme.MonoFont(8.5f, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            Location = new Point(22, 20),
            BackColor = Color.Transparent
        });
        card.Controls.Add(new BufferedLabel
        {
            Text = title,
            Font = Theme.DisplayFont(13, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(22, 52),
            BackColor = Color.Transparent
        });
        var body = new BufferedLabel
        {
            Text = description,
            Font = Theme.Font(9.2f),
            ForeColor = Theme.TextSecondary,
            Location = new Point(22, 90),
            Size = new Size(250, 62),
            BackColor = Color.Transparent
        };
        card.Controls.Add(body);
        card.Resize += (_, _) => body.Width = Math.Max(140, card.ClientSize.Width - 44);
        return card;
    }
}
