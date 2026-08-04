using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class AppsPage : AppPage
{
    private readonly AppPackageService _appPackageService;
    private readonly ReceiptStore _receiptStore;
    private readonly BufferedDataGridView _grid;
    private readonly GlassTextBox _searchBox;
    private readonly BufferedLabel _statusLabel;
    private readonly ActionButton _uninstallButton;
    private IReadOnlyList<AppPackageInfo> _packages = [];

    public AppsPage(AppPackageService appPackageService, ReceiptStore receiptStore)
        : base("\uE71D", "\u5e94\u7528\u7ba1\u7406", "\u67e5\u770b\u5f53\u524d\u7528\u6237\u53ef\u79fb\u9664\u5e94\u7528\uff0c\u7cfb\u7edf\u7ec4\u4ef6\u548c\u53d7\u4fdd\u62a4\u5305\u4e0d\u5728\u5b89\u5168\u8fb9\u754c\u4e4b\u5916\u3002")
    {
        _appPackageService = appPackageService;
        _receiptStore = receiptStore;

        var refreshButton = new ActionButton
        {
            Text = "\u5237\u65b0\u5217\u8868",
            Width = 122,
            Kind = ActionButtonKind.Secondary
        };
        refreshButton.Click += async (_, _) => await RefreshPackagesAsync();
        Header.ActionHost.Controls.Add(refreshButton);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.Canvas,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var commandDeck = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Radius = Theme.RadiusLg,
            SurfaceStyle = SurfaceStyle.Raised,
            Padding = new Padding(28, 24, 28, 24)
        };
        _searchBox = new GlassTextBox
        {
            PlaceholderText = "\u6309\u5e94\u7528\u540d\u79f0\u6216\u6807\u8bc6\u641c\u7d22",
            AccessibleName = "搜索应用",
            AccessibleDescription = "按应用名称或包标识筛选应用列表",
            Location = new Point(28, 31),
            Size = new Size(380, 42)
        };
        _searchBox.TextChanged += (_, _) => RenderPackages();
        _statusLabel = new BufferedLabel
        {
            Text = "\u5c1a\u672a\u8bfb\u53d6\u5e94\u7528\u5217\u8868",
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = false,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 10, 0),
            Size = new Size(290, 36),
            BackColor = Color.Transparent
        };
        _uninstallButton = new ActionButton
        {
            Text = "\u5378\u8f7d\u9009\u4e2d\u5e94\u7528",
            Kind = ActionButtonKind.Danger,
            Size = new Size(170, 50),
            Enabled = false
        };
        _uninstallButton.Click += async (_, _) => await UninstallSelectedAsync();
        commandDeck.Resize += (_, _) => LayoutCommandDeck(layout, commandDeck);
        commandDeck.Controls.Add(_searchBox);
        commandDeck.Controls.Add(_statusLabel);
        commandDeck.Controls.Add(_uninstallButton);

        _grid = new BufferedDataGridView { Dock = DockStyle.Fill };
        Theme.StyleDataGrid(_grid);
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "\u5e94\u7528\u540d\u79f0",
            DataPropertyName = nameof(AppPackageInfo.Name),
            Width = 320
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "\u5305\u6807\u8bc6",
            DataPropertyName = nameof(AppPackageInfo.PackageFullName),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _grid.SelectionChanged += (_, _) => _uninstallButton.Enabled = _grid.SelectedRows.Count == 1;

        var gridHost = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(2),
            Radius = Theme.RadiusLg,
            SurfaceStyle = SurfaceStyle.Raised
        };
        gridHost.Controls.Add(_grid);
        layout.Controls.Add(commandDeck, 0, 0);
        layout.Controls.Add(gridHost, 0, 2);
        Body.Controls.Add(layout);
        LayoutCommandDeck(layout, commandDeck);

        Load += async (_, _) => await RefreshPackagesAsync();
    }

    private void LayoutCommandDeck(TableLayoutPanel layout, Control commandDeck)
    {
        var compact = commandDeck.ClientSize.Width < 900;
        layout.RowStyles[0].Height = compact ? 150 : 104;
        _searchBox.Width = compact ? Math.Max(320, commandDeck.ClientSize.Width - 56) : 380;
        if (compact)
        {
            _statusLabel.Location = new Point(28, 96);
            _statusLabel.Width = Math.Max(240, commandDeck.ClientSize.Width - 240);
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.Padding = new Padding(0);
            _uninstallButton.Location = new Point(commandDeck.ClientSize.Width - 194, 86);
        }
        else
        {
            _uninstallButton.Location = new Point(commandDeck.ClientSize.Width - 194, 27);
            _statusLabel.Location = new Point(_uninstallButton.Left - 314, 34);
            _statusLabel.Width = 298;
            _statusLabel.TextAlign = ContentAlignment.MiddleRight;
            _statusLabel.Padding = new Padding(0, 0, 10, 0);
        }
    }

    private async Task RefreshPackagesAsync()
    {
        _statusLabel.Text = "\u6b63\u5728\u8bfb\u53d6\u5e94\u7528\u5305\u2026";
        _statusLabel.ForeColor = Theme.Info;
        _uninstallButton.Enabled = false;
        try
        {
            _packages = await _appPackageService.GetPackagesAsync();
            _statusLabel.Text = $"\u5df2\u8bfb\u53d6 {_packages.Count} \u4e2a\u53ef\u79fb\u9664\u5e94\u7528";
            _statusLabel.ForeColor = Theme.Success;
            RenderPackages();
        }
        catch (Exception exception)
        {
            _packages = [];
            _statusLabel.Text = $"\u8bfb\u53d6\u5931\u8d25\uff1a{exception.Message}";
            _statusLabel.ForeColor = Theme.Danger;
            RenderPackages();
        }
    }

    private void RenderPackages()
    {
        var query = _searchBox.Text.Trim();
        var filtered = _packages.Where(package => query.Length == 0
            || package.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || package.PackageFullName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        _grid.BeginUpdate();
        _grid.SuspendLayout();
        try
        {
            _grid.DataSource = null;
            _grid.DataSource = filtered;
        }
        finally
        {
            _grid.ResumeLayout(false);
            _grid.EndUpdate();
        }
        _uninstallButton.Enabled = _grid.SelectedRows.Count == 1;
    }

    private async Task UninstallSelectedAsync()
    {
        if (_grid.SelectedRows.Count != 1 || _grid.SelectedRows[0].DataBoundItem is not AppPackageInfo package)
        {
            return;
        }

        var message = $"\u5c06\u4e3a\u5f53\u524d\u7528\u6237\u5378\u8f7d\uff1a\n\n{package.Name}\n\n\u5305\u6807\u8bc6\uff1a{package.PackageFullName}\n\n\u8be5\u5e94\u7528\u53ef\u4ece Microsoft Store \u91cd\u65b0\u5b89\u88c5\uff0c\u662f\u5426\u7ee7\u7eed\uff1f";
        if (MessageBox.Show(this, message, "\u786e\u8ba4\u5378\u8f7d Appx \u5e94\u7528",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        _uninstallButton.Enabled = false;
        _uninstallButton.Text = "\u6b63\u5728\u5378\u8f7d\u2026";
        try
        {
            var startedAt = DateTimeOffset.Now;
            var result = await _appPackageService.UninstallAsync(package);
            await _receiptStore.AppendAsync(
                new ExecutionReceipt(Guid.NewGuid(), startedAt, "\u5e94\u7528\u5378\u8f7d", [result]));
            using var dialog = new ExecutionResultDialog("\u5e94\u7528\u5378\u8f7d\u7ed3\u679c", [result]);
            dialog.ShowDialog(this);
            if (result.Succeeded)
            {
                await RefreshPackagesAsync();
            }
        }
        finally
        {
            _uninstallButton.Text = "\u5378\u8f7d\u9009\u4e2d\u5e94\u7528";
            _uninstallButton.Enabled = _grid.SelectedRows.Count == 1;
        }
    }
}
