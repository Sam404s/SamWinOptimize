using SamWinOptimize.Models;
using SamWinOptimize.Services;
using SamWinOptimize.UI.Controls;

namespace SamWinOptimize.UI.Pages;

public sealed class CleanupPage : AppPage
{
    private readonly CommandRunner _commandRunner;
    private readonly ReceiptStore _receiptStore;
    private readonly HashSet<string> _selectedIds = [];
    private readonly Panel _taskList;
    private readonly Label _selectionLabel;
    private readonly ActionButton _runButton;

    public CleanupPage(CommandRunner commandRunner, ReceiptStore receiptStore)
        : base("\uE74D", "\u7a7a\u95f4\u6e05\u7406", "\u4f18\u5148\u5904\u7406\u53ef\u5b89\u5168\u56de\u6536\u7684\u7f13\u5b58\uff0c\u91ca\u653e\u7a7a\u95f4\u4e14\u98ce\u9669\u53ef\u63a7\u3002")
    {
        _commandRunner = commandRunner;
        _receiptStore = receiptStore;

        foreach (var task in CleanupCatalog.All.Where(task => task.Recommended))
        {
            _selectedIds.Add(task.Id);
        }

        var selectSafeButton = new ActionButton
        {
            Text = "\u9009\u62e9\u5b89\u5168\u9879",
            Width = 134,
            Kind = ActionButtonKind.Secondary
        };
        selectSafeButton.Click += (_, _) =>
        {
            _selectedIds.Clear();
            foreach (var task in CleanupCatalog.All.Where(task => task.Recommended))
            {
                _selectedIds.Add(task.Id);
            }
            RenderTasks();
        };
        Header.ActionHost.Controls.Add(selectSafeButton);

        var commandDeck = new SurfacePanel
        {
            Dock = DockStyle.Top,
            Height = 124,
            SurfaceStyle = SurfaceStyle.Accent,
            Padding = new Padding(30),
            Radius = Theme.RadiusLg
        };
        var noticeIcon = new BufferedLabel
        {
            Text = "\uE74D",
            Font = Theme.IconFont(17),
            ForeColor = Theme.Accent,
            Location = new Point(30, 28),
            Size = new Size(38, 38),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var noticeTitle = new BufferedLabel
        {
            Text = "\u672c\u5730\u7f13\u5b58\u6e05\u7406",
            Font = Theme.DisplayFont(13.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(78, 26),
            BackColor = Color.Transparent
        };
        var noticeText = new BufferedLabel
        {
            Text = "\u9ed8\u8ba4\u4e0d\u9009\u62e9\u56de\u6536\u7ad9\u3001\u4e0d\u8f6c\u79fb\u4e2a\u4eba\u5b58\u50a8\uff0c\u5220\u9664\u524d\u5148\u68c0\u67e5\u3002",
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(79, 60),
            Size = new Size(560, 38),
            BackColor = Color.Transparent
        };
        _selectionLabel = new BufferedLabel
        {
            Font = Theme.Font(9.5f, FontStyle.Bold),
            ForeColor = Theme.TextSecondary,
            TextAlign = ContentAlignment.MiddleRight,
            Size = new Size(196, 36),
            BackColor = Color.Transparent
        };
        _runButton = new ActionButton
        {
            Text = "\u5f00\u59cb\u6e05\u7406",
            Kind = ActionButtonKind.Primary,
            Size = new Size(150, 50)
        };
        _runButton.Click += async (_, _) => await RunCleanupAsync();
        commandDeck.Resize += (_, _) => LayoutCommandDeck(commandDeck, noticeText);
        commandDeck.Controls.Add(noticeIcon);
        commandDeck.Controls.Add(noticeTitle);
        commandDeck.Controls.Add(noticeText);
        commandDeck.Controls.Add(_selectionLabel);
        commandDeck.Controls.Add(_runButton);

        _taskList = new BufferedScrollablePanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Theme.Canvas,
            Padding = new Padding(0, 24, 0, 0)
        };
        Body.Controls.Add(_taskList);
        Body.Controls.Add(commandDeck);
        LayoutCommandDeck(commandDeck, noticeText);
        RenderTasks();
    }

    private void LayoutCommandDeck(Control commandDeck, Label noticeText)
    {
        var compact = commandDeck.ClientSize.Width < 900;
        commandDeck.Height = compact ? 168 : 124;
        if (compact)
        {
            noticeText.Width = Math.Max(420, commandDeck.ClientSize.Width - 120);
            _selectionLabel.Location = new Point(30, 116);
            _selectionLabel.TextAlign = ContentAlignment.MiddleLeft;
            _selectionLabel.Width = Math.Max(220, commandDeck.ClientSize.Width - 230);
            _runButton.Location = new Point(commandDeck.ClientSize.Width - 176, 106);
        }
        else
        {
            _runButton.Location = new Point(commandDeck.ClientSize.Width - 176, 37);
            _selectionLabel.Location = new Point(_runButton.Left - 212, 44);
            _selectionLabel.TextAlign = ContentAlignment.MiddleRight;
            _selectionLabel.Width = 196;
            noticeText.Width = Math.Max(320, _selectionLabel.Left - noticeText.Left - 28);
        }
    }

    private void RenderTasks()
    {
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
            foreach (var task in CleanupCatalog.All.Reverse())
            {
                var card = new TaskCard(task.Id, task.Title, task.Description, task.Category,
                    task.Risk, task.RequiresAdministrator, false, _selectedIds.Contains(task.Id));
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
        var selected = CleanupCatalog.All.Where(task => _selectedIds.Contains(task.Id)).ToList();
        var cautionCount = selected.Count(task => task.Risk != RiskLevel.Low);
        _selectionLabel.Text = cautionCount > 0
            ? $"\u5df2\u9009 {selected.Count} \u9879  \u00b7  {cautionCount} \u9879\u9700\u786e\u8ba4"
            : $"\u5df2\u9009 {selected.Count} \u9879\uff0c\u5747\u4e3a\u5b89\u5168\u8303\u56f4";
        _selectionLabel.ForeColor = cautionCount > 0 ? Theme.Warning : Theme.TextSecondary;
        _runButton.Enabled = selected.Count > 0;
    }

    private async Task RunCleanupAsync()
    {
        var selected = CleanupCatalog.All.Where(task => _selectedIds.Contains(task.Id)).ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var message = "\u5373\u5c06\u6e05\u7406\u4ee5\u4e0b\u76ee\u6807\uff1a\n\n" +
                      string.Join("\n", selected.Select(task => $"\u2022 {task.Title}")) +
                      "\n\n\u5220\u9664\u7684\u7f13\u5b58\u4f1a\u6309\u672c\u5730\u7f13\u5b58\u5904\u7406\uff0c\u90e8\u5206\u6587\u4ef6\u901a\u8fc7\u56de\u6536\u7ad9\u65e0\u6cd5\u6062\u590d\uff0c\u662f\u5426\u7ee7\u7eed\uff1f";
        if (MessageBox.Show(this, message, "\u786e\u8ba4\u7a7a\u95f4\u6e05\u7406",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        _runButton.Enabled = false;
        _runButton.Text = "\u6b63\u5728\u6e05\u7406\u2026";
        var startedAt = DateTimeOffset.Now;
        var results = new List<CommandResult>();
        try
        {
            foreach (var task in selected)
            {
                _selectionLabel.Text = $"\u6b63\u5728\u6e05\u7406\uff1a{task.Title}";
                results.Add(await _commandRunner.CleanupAsync(task));
            }

            await _receiptStore.AppendAsync(
                new ExecutionReceipt(Guid.NewGuid(), startedAt, "\u7a7a\u95f4\u6e05\u7406", results));
            using var dialog = new ExecutionResultDialog("\u7a7a\u95f4\u6e05\u7406\u7ed3\u679c", results);
            dialog.ShowDialog(this);
        }
        finally
        {
            _runButton.Text = "\u5f00\u59cb\u6e05\u7406";
            UpdateSelectionState();
        }
    }
}
