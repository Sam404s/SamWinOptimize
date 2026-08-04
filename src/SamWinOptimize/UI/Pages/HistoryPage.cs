using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class HistoryPage : AppPage
{
    private readonly ReceiptStore _receiptStore;
    private readonly CommandRunner _commandRunner;
    private readonly DataGridView _grid;
    private readonly RichTextBox _details;
    private readonly Label _statusLabel;
    private readonly ActionButton _restoreButton;
    private IReadOnlyList<ExecutionReceipt> _receipts = [];

    public HistoryPage(ReceiptStore receiptStore, CommandRunner commandRunner)
        : base("\uE777", "\u8bb0\u5f55\u4e0e\u8fd8\u539f", "\u540c\u4e00\u5957\u672c\u5730\u53ef\u67e5\u6267\u884c\u8bb0\u5f55\uff0c\u8fd8\u539f\u652f\u6301\u7cfb\u7edf\u4f18\u5316\u9879\u3002")
    {
        _receiptStore = receiptStore;
        _commandRunner = commandRunner;

        var clearButton = new ActionButton
        {
            Text = "\u6e05\u7a7a\u8bb0\u5f55",
            Width = 120,
            Kind = ActionButtonKind.Danger
        };
        clearButton.Click += async (_, _) => await ClearHistoryAsync();
        var refreshButton = new ActionButton
        {
            Text = "\u5237\u65b0\u8bb0\u5f55",
            Width = 122,
            Kind = ActionButtonKind.Secondary
        };
        refreshButton.Click += async (_, _) => await LoadHistoryAsync();
        Header.ActionHost.Controls.Add(clearButton);
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
            Padding = new Padding(28)
        };
        var commandTitle = new Label
        {
            Text = "\u672c\u5730\u6267\u884c\u8bb0\u5f55",
            Font = Theme.DisplayFont(13.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(28, 24),
            BackColor = Color.Transparent
        };
        _statusLabel = new Label
        {
            Text = "\u5c1a\u672a\u8bfb\u53d6\u6267\u884c\u8bb0\u5f55",
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoSize = true,
            Location = new Point(29, 58),
            BackColor = Color.Transparent
        };
        _restoreButton = new ActionButton
        {
            Text = "\u8fd8\u539f\u9009\u4e2d\u4f18\u5316",
            Kind = ActionButtonKind.Primary,
            Size = new Size(172, 50),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        commandDeck.Resize += (_, _) =>
            _restoreButton.Location = new Point(commandDeck.ClientSize.Width - 196, 27);
        _restoreButton.Click += async (_, _) => await RestoreSelectedAsync();
        commandDeck.Controls.Add(commandTitle);
        commandDeck.Controls.Add(_statusLabel);
        commandDeck.Controls.Add(_restoreButton);

        _grid = CreateGrid();
        _details = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Theme.SurfaceRaised,
            ForeColor = Theme.TextSecondary,
            Font = Theme.MonoFont(9),
            DetectUrls = false,
            Text = "\u9009\u62e9\u4e00\u6761\u8bb0\u5f55\u67e5\u770b\u5b8c\u6574\u8f93\u51fa",
            Margin = new Padding(0)
        };
        _grid.SelectionChanged += (_, _) => ShowSelectedReceipt();

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 18,
            BackColor = Theme.Canvas
        };
        split.SizeChanged += (_, _) => UpdateSplitLayout(split);
        var gridHost = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(2),
            Radius = Theme.RadiusLg,
            SurfaceStyle = SurfaceStyle.Raised
        };
        gridHost.Controls.Add(_grid);
        var detailHost = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            Radius = Theme.RadiusLg,
            SurfaceStyle = SurfaceStyle.Raised
        };
        var detailTitle = new Label
        {
            Text = "\u6267\u884c\u8f93\u51fa",
            Font = Theme.DisplayFont(13, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.Transparent
        };
        detailHost.Controls.Add(_details);
        detailHost.Controls.Add(detailTitle);
        split.Panel1.Controls.Add(gridHost);
        split.Panel2.Controls.Add(detailHost);
        layout.Controls.Add(commandDeck, 0, 0);
        layout.Controls.Add(split, 0, 2);
        Body.Controls.Add(layout);
        UpdateSplitLayout(split);
        Load += async (_, _) => await LoadHistoryAsync();
    }

    private static DataGridView CreateGrid()
    {
        var grid = new BufferedDataGridView { Dock = DockStyle.Fill };
        Theme.StyleDataGrid(grid);
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "\u65f6\u95f4",
            DataPropertyName = nameof(ReceiptRow.Time),
            Width = 168
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "\u64cd\u4f5c",
            DataPropertyName = nameof(ReceiptRow.Operation),
            Width = 128
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "\u6458\u8981",
            DataPropertyName = nameof(ReceiptRow.Summary),
            Width = 108
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "\u9879\u76ee",
            DataPropertyName = nameof(ReceiptRow.Items),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        return grid;
    }

    private static void UpdateSplitLayout(SplitContainer split)
    {
        split.SplitterDistance = Math.Max(380, (int)(split.ClientSize.Width * 0.52));
    }

    private async Task LoadHistoryAsync()
    {
        _receipts = await _receiptStore.LoadAsync();
        _grid.SuspendLayout();
        try
        {
            _grid.DataSource = _receipts.Select((receipt, index) => new ReceiptRow(
                index,
                receipt.StartedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                receipt.Operation,
                $"{receipt.Results.Count(result => result.Succeeded)}/{receipt.Results.Count} \u6210\u529f",
                string.Join("\u3001", receipt.Results.Select(result => result.Title)))).ToList();
        }
        finally
        {
            _grid.ResumeLayout(true);
        }
        _grid.Invalidate();
        _statusLabel.Text = _receipts.Count == 0 ? "\u6682\u65e0\u6267\u884c\u8bb0\u5f55" : $"\u5171 {_receipts.Count} \u6761\u672c\u5730\u6267\u884c\u8bb0\u5f55";
        _statusLabel.ForeColor = _receipts.Count == 0 ? Theme.TextMuted : Theme.Success;
        ShowSelectedReceipt();
    }

    private void ShowSelectedReceipt()
    {
        if (_grid.SelectedRows.Count != 1 || _grid.SelectedRows[0].DataBoundItem is not ReceiptRow row
            || row.Index < 0 || row.Index >= _receipts.Count)
        {
            _details.Text = "\u9009\u62e9\u4e00\u6761\u8bb0\u5f55\u67e5\u770b\u5b8c\u6574\u8f93\u51fa";
            _restoreButton.Enabled = false;
            return;
        }

        var receipt = _receipts[row.Index];
        _details.Text = FormatReceipt(receipt);
        _restoreButton.Enabled = receipt.Operation == "\u7cfb\u7edf\u4f18\u5316"
            && receipt.Results.Any(result => result.Succeeded && OptimizationCatalog.Find(result.ItemId) is not null);
    }

    private async Task RestoreSelectedAsync()
    {
        if (_grid.SelectedRows.Count != 1 || _grid.SelectedRows[0].DataBoundItem is not ReceiptRow row
            || row.Index < 0 || row.Index >= _receipts.Count)
        {
            return;
        }

        var sourceReceipt = _receipts[row.Index];
        var actions = sourceReceipt.Results
            .Where(result => result.Succeeded)
            .Select(result => OptimizationCatalog.Find(result.ItemId))
            .Where(action => action is not null)
            .Cast<SystemAction>()
            .ToList();
        if (actions.Count == 0)
        {
            return;
        }

        var message = $"\u5c06\u6309\u8bb0\u5f55\u8fd8\u539f {actions.Count} \u9879\u914d\u7f6e\uff1a\n\n" +
                      string.Join("\n", actions.Select(action => $"\u2022 {action.Title}")) +
                      "\n\n\u8fd8\u539f\u64cd\u4f5c\u4f1a\u751f\u6210\u4e00\u6761\u65b0\u7684\u6267\u884c\u8bb0\u5f55\uff0c\u662f\u5426\u7ee7\u7eed\uff1f";
        if (MessageBox.Show(this, message, "\u786e\u8ba4\u8fd8\u539f\u4f18\u5316",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        _restoreButton.Enabled = false;
        _restoreButton.Text = "\u6b63\u5728\u8fd8\u539f\u2026";
        var startedAt = DateTimeOffset.Now;
        var results = new List<CommandResult>();
        try
        {
            foreach (var action in actions)
            {
                results.Add(await _commandRunner.RevertAsync(action));
            }
            await _receiptStore.AppendAsync(
                new ExecutionReceipt(Guid.NewGuid(), startedAt, "\u4f18\u5316\u8fd8\u539f", results));
            using var dialog = new ExecutionResultDialog("\u4f18\u5316\u8fd8\u539f\u7ed3\u679c", results);
            dialog.ShowDialog(this);
            await LoadHistoryAsync();
        }
        finally
        {
            _restoreButton.Text = "\u8fd8\u539f\u9009\u4e2d\u4f18\u5316";
        }
    }

    private async Task ClearHistoryAsync()
    {
        if (_receipts.Count == 0)
        {
            return;
        }

        if (MessageBox.Show(this, "\u5c06\u5220\u9664\u672c\u5730\u5168\u90e8\u6267\u884c\u8bb0\u5f55\uff0c\u8be5\u64cd\u4f5c\u4e0d\u53ef\u6062\u590d\uff0c\u4e14\u4e0d\u5f71\u54cd\u7cfb\u7edf\u914d\u7f6e\u3002",
                "\u786e\u8ba4\u6e05\u7a7a\u8bb0\u5f55", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        await _receiptStore.ClearAsync();
        await LoadHistoryAsync();
    }

    private static string FormatReceipt(ExecutionReceipt receipt)
    {
        var lines = new List<string>
        {
            $"Receipt: {receipt.Id}",
            $"Time: {receipt.StartedAt.LocalDateTime:yyyy-MM-dd HH:mm:ss}",
            $"Operation: {receipt.Operation}",
            new string('\u2500', 72)
        };
        foreach (var result in receipt.Results)
        {
            lines.Add($"[{(result.Succeeded ? "OK" : "FAIL")}] {result.Title} ({result.ItemId})");
            lines.Add($"ExitCode: {result.ExitCode}");
            if (!string.IsNullOrWhiteSpace(result.Output))
            {
                lines.Add(result.Output);
            }
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                lines.Add($"Error: {result.Error}");
            }
            lines.Add(new string('\u2500', 72));
        }
        return string.Join(Environment.NewLine, lines);
    }

    private sealed record ReceiptRow(int Index, string Time, string Operation, string Summary, string Items);
}