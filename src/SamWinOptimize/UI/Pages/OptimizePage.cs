using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class OptimizePage : AppPage
{
    private readonly CommandRunner _commandRunner;
    private readonly ReceiptStore _receiptStore;
    private readonly HashSet<string> _selectedIds = [];
    private readonly Panel _taskList;
    private readonly TextBox _searchBox;
    private readonly ComboBox _categoryBox;
    private readonly Label _selectionLabel;
    private readonly ActionButton _applyButton;

    public OptimizePage(CommandRunner commandRunner, ReceiptStore receiptStore)
        : base("\uE9D9", "\u7cfb\u7edf\u4f18\u5316", "\u4ece\u53ef\u8fd8\u539f\u7684\u4f4e\u98ce\u9669\u5efa\u8bae\u5f00\u59cb\uff0c\u6bcf\u4e00\u6b21\u6539\u53d8\u90fd\u5728\u53ef\u8ffd\u8e2a\u8303\u56f4\u5185\u3002")
    {
        _commandRunner = commandRunner;
        _receiptStore = receiptStore;

        foreach (var action in OptimizationCatalog.All.Where(action => action.Recommended))
        {
            _selectedIds.Add(action.Id);
        }

        var clearButton = new ActionButton
        {
            Text = "\u6e05\u7a7a\u9009\u62e9",
            Width = 120,
            Kind = ActionButtonKind.Secondary
        };
        clearButton.Click += (_, _) =>
        {
            _selectedIds.Clear();
            RenderTasks();
        };
        var recommendedButton = new ActionButton
        {
            Text = "\u9009\u62e9\u63a8\u8350\u9879",
            Width = 136,
            Kind = ActionButtonKind.Secondary
        };
        recommendedButton.Click += (_, _) =>
        {
            _selectedIds.Clear();
            foreach (var action in OptimizationCatalog.All.Where(action => action.Recommended))
            {
                _selectedIds.Add(action.Id);
            }
            RenderTasks();
        };
        Header.ActionHost.Controls.Add(clearButton);
        Header.ActionHost.Controls.Add(recommendedButton);

        var commandDeck = new SurfacePanel
        {
            Dock = DockStyle.Top,
            Height = 104,
            Padding = new Padding(28, 24, 28, 24),
            Radius = Theme.RadiusLg,
            SurfaceStyle = SurfaceStyle.Raised
        };
        _searchBox = new TextBox
        {
            PlaceholderText = "\u641c\u7d22\u540d\u79f0\u6216\u8bf4\u660e",
            AccessibleName = "\u641c\u7d22\u4f18\u5316\u9879",
            AccessibleDescription = "\u6309\u540d\u79f0\u6216\u8bf4\u660e\u7b5b\u9009\u4f18\u5316\u9879\u76ee",
            Font = Theme.Font(10),
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Location = new Point(28, 32),
            Size = new Size(350, 38)
        };
        Theme.StyleTextInput(_searchBox);
        _searchBox.TextChanged += (_, _) => RenderTasks();
        _categoryBox = new ComboBox
        {
            AccessibleName = "\u4f18\u5316\u5206\u7c7b",
            AccessibleDescription = "\u6309\u5206\u7c7b\u7b5b\u9009\u4f18\u5316\u9879\u76ee",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.Surface,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(394, 31),
            Size = new Size(180, 40)
        };
        Theme.StyleDropDown(_categoryBox);
        _categoryBox.Items.Add("\u5168\u90e8\u5206\u7c7b");
        _categoryBox.Items.AddRange(OptimizationCatalog.All.Select(action => action.Category).Distinct().Cast<object>().ToArray());
        _categoryBox.SelectedIndex = 0;
        _categoryBox.SelectedIndexChanged += (_, _) => RenderTasks();
        _selectionLabel = new BufferedLabel
        {
            Font = Theme.Font(9.5f, FontStyle.Bold),
            ForeColor = Theme.TextSecondary,
            TextAlign = ContentAlignment.MiddleRight,
            Size = new Size(196, 36),
            BackColor = Color.Transparent
        };
        _applyButton = new ActionButton
        {
            Text = "\u5e94\u7528\u9009\u4e2d\u4f18\u5316",
            Kind = ActionButtonKind.Primary,
            Size = new Size(170, 50)
        };
        _applyButton.Click += async (_, _) => await ApplySelectedAsync();
        commandDeck.Resize += (_, _) => LayoutCommandDeck(commandDeck);
        commandDeck.Controls.Add(_searchBox);
        commandDeck.Controls.Add(_categoryBox);
        commandDeck.Controls.Add(_selectionLabel);
        commandDeck.Controls.Add(_applyButton);

        _taskList = new BufferedScrollablePanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Theme.Canvas,
            Padding = new Padding(0, 24, 0, 0)
        };

        Body.Controls.Add(_taskList);
        Body.Controls.Add(commandDeck);
        LayoutCommandDeck(commandDeck);
        RenderTasks();
    }

    private void LayoutCommandDeck(Control commandDeck)
    {
        var compact = commandDeck.ClientSize.Width < 900;
        commandDeck.Height = compact ? 152 : 104;
        if (compact)
        {
            _searchBox.Width = Math.Max(280, commandDeck.ClientSize.Width - 260);
            _categoryBox.Location = new Point(_searchBox.Right + 16, 31);
            _categoryBox.Width = Math.Max(140, commandDeck.ClientSize.Width - _categoryBox.Left - 24);
            _selectionLabel.Location = new Point(28, 96);
            _selectionLabel.TextAlign = ContentAlignment.MiddleLeft;
            _selectionLabel.Width = Math.Max(200, commandDeck.ClientSize.Width - 230);
            _applyButton.Location = new Point(commandDeck.ClientSize.Width - 194, 86);
        }
        else
        {
            _applyButton.Location = new Point(commandDeck.ClientSize.Width - 194, 27);
            _selectionLabel.Location = new Point(_applyButton.Left - 210, 34);
            _selectionLabel.TextAlign = ContentAlignment.MiddleRight;
            _selectionLabel.Width = 196;
        }
    }

    private void RenderTasks()
    {
        var query = _searchBox.Text.Trim();
        var category = _categoryBox.SelectedItem?.ToString() ?? "\u5168\u90e8\u5206\u7c7b";
        var matches = OptimizationCatalog.All.Where(action =>
            (category == "\u5168\u90e8\u5206\u7c7b" || action.Category == category)
            && (query.Length == 0
                || action.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || action.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase)))
            .ToList();

        _taskList.SuspendLayout();
        try
        {
            var existingCards = _taskList.Controls.Cast<Control>().ToArray();
            _taskList.Controls.Clear();
            foreach (var existingCard in existingCards)
            {
                existingCard.Dispose();
            }
            _taskList.AutoScrollPosition = Point.Empty;
            foreach (var action in matches.AsEnumerable().Reverse())
            {
                var card = new TaskCard(action.Id, action.Title, action.Description, action.Category,
                    action.Risk, action.RequiresAdministrator, action.RequiresRestart,
                    _selectedIds.Contains(action.Id));
                card.SelectionChanged += (_, _) =>
                {
                    if (card.Selected)
                    {
                        _selectedIds.Add(card.ItemId);
                    }
                    else
                    {
                        _selectedIds.Remove(card.ItemId);
                    }
                    UpdateSelectionState();
                };
                _taskList.Controls.Add(card);
            }
        }
        finally
        {
            _taskList.ResumeLayout(true);
        }
        _taskList.Invalidate(true);
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        var selected = OptimizationCatalog.All.Where(action => _selectedIds.Contains(action.Id)).ToList();
        var highRiskCount = selected.Count(action => action.Risk == RiskLevel.High);
        _selectionLabel.Text = highRiskCount > 0
            ? $"\u5df2\u9009 {selected.Count} \u9879  \u00b7  {highRiskCount} \u9879\u9ad8\u98ce\u9669"
            : $"\u5df2\u9009 {selected.Count} \u9879\uff0c\u5747\u4e3a\u4f4e\u98ce\u9669";
        _selectionLabel.ForeColor = highRiskCount > 0 ? Theme.Danger : Theme.TextSecondary;
        _applyButton.Enabled = selected.Count > 0;
    }

    private async Task ApplySelectedAsync()
    {
        var selected = OptimizationCatalog.All.Where(action => _selectedIds.Contains(action.Id)).ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var adminCount = selected.Count(action => action.RequiresAdministrator);
        var restartCount = selected.Count(action => action.RequiresRestart);
        var highRiskCount = selected.Count(action => action.Risk == RiskLevel.High);
        var message = $"\u5373\u5c06\u5e94\u7528 {selected.Count} \u9879\u4f18\u5316\u3002\n\n" +
                      $"\u9700\u7ba1\u7406\u5458\uff1a{adminCount} \u9879\n\u9700\u91cd\u542f\uff1a{restartCount} \u9879\n\u9ad8\u98ce\u9669\uff1a{highRiskCount} \u9879\n\n" +
                      "\u6bcf\u9879\u5355\u72ec\u6267\u884c\u5e76\u751f\u6210\u8bb0\u5f55\uff0c\u662f\u5426\u7ee7\u7eed\uff1f";
        if (MessageBox.Show(this, message, "\u786e\u8ba4\u5e94\u7528\u4f18\u5316",
                MessageBoxButtons.OKCancel, highRiskCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information)
            != DialogResult.OK)
        {
            return;
        }

        _applyButton.Enabled = false;
        _applyButton.Text = "\u6b63\u5728\u6267\u884c\u2026";
        var startedAt = DateTimeOffset.Now;
        var results = new List<CommandResult>();
        try
        {
            foreach (var action in selected)
            {
                _selectionLabel.Text = $"\u6b63\u5728\u6267\u884c\uff1a{action.Title}";
                results.Add(await _commandRunner.ApplyAsync(action));
            }

            var receipt = new ExecutionReceipt(Guid.NewGuid(), startedAt, "\u7cfb\u7edf\u4f18\u5316", results);
            await _receiptStore.AppendAsync(receipt);
            using var dialog = new ExecutionResultDialog("\u4f18\u5316\u6267\u884c\u7ed3\u679c", results);
            dialog.ShowDialog(this);
        }
        finally
        {
            _applyButton.Text = "\u5e94\u7528\u9009\u4e2d\u4f18\u5316";
            UpdateSelectionState();
        }
    }
}
