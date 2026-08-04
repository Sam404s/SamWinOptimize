using System.Runtime.InteropServices;
using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI;
using SamWinOptimize.UI.Controls;
using SamWinOptimize.UI.Pages;

namespace SamWinOptimize;

public sealed class MainForm : Form
{
    private readonly CommandRunner _commandRunner = new();
    private readonly ReceiptStore _receiptStore = new();
    private readonly SystemInfoService _systemInfoService = new();
    private readonly Dictionary<AppRoute, NavButton> _navButtons = [];
    private readonly Dictionary<AppRoute, UserControl> _pages = [];
    private readonly Panel _pageHost;
    private Label _routeStatus = null!;

    public MainForm()
    {
        Text = "SamWinOptimize";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1520, 960);
        MinimumSize = new Size(1240, 800);
        BackColor = Theme.Canvas;
        ForeColor = Theme.TextPrimary;
        Font = Theme.Font(9.5f);
        FormBorderStyle = FormBorderStyle.Sizable;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
        HandleCreated += (_, _) => EnableDarkTitleBar();

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Theme.Canvas
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 344));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var sidebar = BuildSidebar();
        var workspace = new BackdropPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Canvas,
            Padding = new Padding(0)
        };
        _pageHost = new BackdropPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Canvas
        };
        var statusBar = BuildStatusBar();
        workspace.Controls.Add(_pageHost);
        workspace.Controls.Add(statusBar);

        shell.Controls.Add(sidebar, 0, 0);
        shell.Controls.Add(workspace, 1, 0);
        Controls.Add(shell);
        Navigate(AppRoute.Dashboard);
    }

    private Panel BuildSidebar()
    {
        var sidebar = new BackdropPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Sidebar,
            SidebarMode = true,
            Padding = new Padding(28, 0, 28, 28)
        };
        var brand = BuildBrand();
        var nav = new BufferedFlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.SidebarRaised,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        AddNavigation(nav, AppRoute.Dashboard, "\uE80F", "\u8bbe\u5907\u603b\u89c8");
        AddNavigation(nav, AppRoute.Optimize, "\uE9D9", "\u7cfb\u7edf\u4f18\u5316");
        AddNavigation(nav, AppRoute.Cleanup, "\uE74D", "\u7a7a\u95f4\u6e05\u7406");
        AddNavigation(nav, AppRoute.Apps, "\uE71D", "\u5e94\u7528\u7ba1\u7406");
        AddNavigation(nav, AppRoute.Security, "\uE72E", "\u5b89\u5168\u4e0e\u8ba4\u8bc1");
        AddNavigation(nav, AppRoute.Restore, "\uE777", "\u8bb0\u5f55\u4e0e\u8fd8\u539f");
        AddNavigation(nav, AppRoute.Toolbox, "\uE713", "\u7cfb\u7edf\u5de5\u5177\u7bb1");
        AddNavigation(nav, AppRoute.About, "\uE946", "\u5173\u4e8e\u4ea7\u54c1");

        var permissionCard = BuildPermissionCard();
        sidebar.Controls.Add(permissionCard);
        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(brand);

        void UpdateSidebarDensity()
        {
            var compact = sidebar.ClientSize.Height < 780;
            brand.Height = compact ? 108 : 140;
            permissionCard.Height = compact ? 112 : 140;

            for (var index = 0; index < nav.Controls.Count; index++)
            {
                var gap = index == nav.Controls.Count - 1 ? 0 : compact ? 6 : 14;
                nav.Controls[index].Height = compact ? 56 : 64;
                nav.Controls[index].Margin = new Padding(0, 0, 0, gap);
            }

            nav.PerformLayout();
        }

        sidebar.SizeChanged += (_, _) => UpdateSidebarDensity();
        UpdateSidebarDensity();
        return sidebar;
    }

    private static Panel BuildBrand()
    {
        var brand = new BackdropPanel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Theme.Sidebar,
            SidebarMode = true
        };
        var markTile = new SurfacePanel
        {
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusLg,
            Size = new Size(56, 56),
            Location = new Point(4, 32),
            Padding = new Padding(1)
        };
        markTile.Controls.Add(new Label
        {
            Text = "S",
            Font = Theme.DisplayFont(22, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        });
        var name = new Label
        {
            Text = "SamWin",
            Font = Theme.DisplayFont(18, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(74, 30),
            Size = new Size(214, 34),
            BackColor = Color.Transparent
        };
        var tagline = new Label
        {
            Text = "OPTIMIZE  ·  GLASS",
            Font = Theme.MonoFont(8.2f),
            ForeColor = Theme.TextMuted,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(76, 64),
            Size = new Size(212, 20),
            BackColor = Color.Transparent
        };
        brand.Controls.Add(markTile);
        brand.Controls.Add(name);
        brand.Controls.Add(tagline);
        return brand;
    }

    private static SurfacePanel BuildPermissionCard()
    {
        var card = new SurfacePanel
        {
            Dock = DockStyle.Bottom,
            Height = 140,
            SurfaceStyle = SurfaceStyle.Accent,
            Radius = Theme.RadiusLg,
            Padding = new Padding(22, 18, 22, 18)
        };
        var glyph = new Label
        {
            Text = "\uE72E",
            Font = Theme.IconFont(16),
            ForeColor = Theme.Success,
            Size = new Size(36, 36),
            Location = new Point(20, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        var title = new Label
        {
            Text = "\u672c\u5730\u6267\u884c\u6a21\u5f0f",
            Font = Theme.Font(10, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(22, 62),
            BackColor = Color.Transparent
        };
        var detail = new Label
        {
            Text = "\u6240\u6709\u64cd\u4f5c\u4ec5\u5728\u6b64\u8bbe\u5907\u8fd0\u884c\uff0c\n\u4e0d\u4e0a\u4f20\u4efb\u4f55\u6570\u636e\u5230\u7f51\u7edc\u3002",
            Font = Theme.Font(8.8f),
            ForeColor = Theme.TextSecondary,
            Location = new Point(22, 88),
            Size = new Size(220, 48),
            BackColor = Color.Transparent
        };
        card.Controls.Add(glyph);
        card.Controls.Add(title);
        card.Controls.Add(detail);
        return card;
    }

    private Panel BuildStatusBar()
    {
        var statusBar = new BackdropPanel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = Theme.CanvasSoft,
            Padding = new Padding(32, 0, 32, 0)
        };
        var privacyStatus = new Label
        {
            Text = "\u25cf  \u672c\u5730\u6267\u884c  \u00b7  \u8bbe\u5907\u6570\u636e\u4e0d\u79bb\u5f00\u672c\u673a",
            Font = Theme.Font(8.5f, FontStyle.Bold),
            ForeColor = Theme.Success,
            Dock = DockStyle.Left,
            Width = 340,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _routeStatus = new Label
        {
            Text = "\u8bbe\u5907\u603b\u89c8",
            Font = Theme.MonoFont(8),
            ForeColor = Theme.TextMuted,
            Dock = DockStyle.Right,
            Width = 340,
            TextAlign = ContentAlignment.MiddleRight
        };
        statusBar.Controls.Add(privacyStatus);
        statusBar.Controls.Add(_routeStatus);
        return statusBar;
    }

    private void AddNavigation(FlowLayoutPanel nav, AppRoute route, string glyph, string label)
    {
        var button = new NavButton(glyph, label)
        {
            Width = 272,
            Height = 64,
            Dock = DockStyle.None
        };
        button.Click += (_, _) => Navigate(route);
        nav.Controls.Add(button);
        _navButtons[route] = button;
    }

    private void Navigate(AppRoute route)
    {
        foreach (var (candidate, button) in _navButtons)
        {
            button.Selected = candidate == route;
        }

        var page = GetOrCreatePage(route);
        _pageHost.SuspendLayout();
        try
        {
            foreach (Control control in _pageHost.Controls)
            {
                control.Visible = ReferenceEquals(control, page);
            }

            if (!_pageHost.Controls.Contains(page))
            {
                page.Dock = DockStyle.Fill;
                _pageHost.Controls.Add(page);
            }

            page.Visible = true;
            page.BringToFront();
            _routeStatus.Text = $"SAM / {RouteLabel(route)}";
        }
        finally
        {
            _pageHost.ResumeLayout(false);
            _pageHost.PerformLayout();
            _pageHost.Invalidate(invalidateChildren: true);
            _pageHost.Update();
        }
    }

    private UserControl GetOrCreatePage(AppRoute route)
    {
        if (_pages.TryGetValue(route, out var existingPage))
        {
            return existingPage;
        }

        UserControl page = route switch
        {
            AppRoute.Dashboard => new DashboardPage(_systemInfoService, Navigate),
            AppRoute.Optimize => new OptimizePage(_commandRunner, _receiptStore),
            AppRoute.Cleanup => new CleanupPage(_commandRunner, _receiptStore),
            AppRoute.Apps => new AppsPage(new AppPackageService(_commandRunner), _receiptStore),
            AppRoute.Security => new SecurityPage(
                _systemInfoService, new SecurityDiagnosticsService(_commandRunner), _receiptStore),
            AppRoute.Restore => new HistoryPage(_receiptStore, _commandRunner),
            AppRoute.Toolbox => new ToolboxPage(_commandRunner, _receiptStore),
            AppRoute.About => new AboutPage(),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, null)
        };

        _pages[route] = page;
        return page;
    }

    private static string RouteLabel(AppRoute route) => route switch
    {
        AppRoute.Dashboard => "\u8bbe\u5907\u603b\u89c8",
        AppRoute.Optimize => "\u7cfb\u7edf\u4f18\u5316",
        AppRoute.Cleanup => "\u7a7a\u95f4\u6e05\u7406",
        AppRoute.Apps => "\u5e94\u7528\u7ba1\u7406",
        AppRoute.Security => "\u5b89\u5168\u4e0e\u8ba4\u8bc1",
        AppRoute.Restore => "\u8bb0\u5f55\u4e0e\u8fd8\u539f",
        AppRoute.Toolbox => "\u7cfb\u7edf\u5de5\u5177\u7bb1",
        AppRoute.About => "\u5173\u4e8e\u4ea7\u54c1",
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, null)
    };

    private void EnableDarkTitleBar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        var enabled = 1;
        var result = DwmSetWindowAttribute(Handle, 20, ref enabled, sizeof(int));
        if (result != 0)
        {
            _ = DwmSetWindowAttribute(Handle, 19, ref enabled, sizeof(int));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Child pages are disposed by the WinForms control tree.
            _receiptStore.Dispose();
        }

        base.Dispose(disposing);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int attributeValue,
        int attributeSize);
}
