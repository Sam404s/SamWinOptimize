using System.ComponentModel;
using SamWinOptimize.Models;

namespace SamWinOptimize.UI.Controls;

public sealed class TaskCard : SurfacePanel
{
    private readonly CheckBox _checkBox;
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly BufferedFlowLayoutPanel _tags;

    public TaskCard(
        string itemId,
        string title,
        string description,
        string category,
        RiskLevel risk,
        bool requiresAdministrator,
        bool requiresRestart,
        bool selected)
    {
        ItemId = itemId;
        Height = 124;
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 16);
        Padding = new Padding(24, 18, 24, 18);
        Hoverable = true;

        _checkBox = new CheckBox
        {
            Checked = selected,
            AutoSize = false,
            Size = new Size(38, 38),
            Location = new Point(24, 43),
            Cursor = Cursors.Hand,
            BackColor = Color.Transparent,
            ForeColor = Theme.Accent,
            AccessibleName = title,
            AccessibleDescription = description
        };
        _checkBox.CheckedChanged += (_, _) =>
        {
            AccentEdge = _checkBox.Checked;
            SurfaceStyle = _checkBox.Checked ? SurfaceStyle.Raised : SurfaceStyle.Default;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        };

        _titleLabel = new Label
        {
            Text = title,
            Font = Theme.Font(11.5f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Location = new Point(78, 22),
            Size = new Size(520, 30),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        _descriptionLabel = new Label
        {
            Text = description,
            Font = Theme.Font(9.5f),
            ForeColor = Theme.TextSecondary,
            Location = new Point(78, 58),
            Size = new Size(620, 48),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        _tags = new BufferedFlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 310,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(4, 26, 4, 4),
            BackColor = Color.Transparent
        };
        _tags.Controls.Add(new StatusPill(Theme.RiskLabel(risk), Theme.RiskColor(risk)));
        if (requiresAdministrator)
        {
            _tags.Controls.Add(new StatusPill("\u7ba1\u7406\u5458", Theme.Info));
        }
        if (requiresRestart)
        {
            _tags.Controls.Add(new StatusPill("\u9700\u91cd\u542f", Theme.Warning));
        }
        _tags.Controls.Add(new StatusPill(category, Theme.TextSecondary));

        Controls.Add(_tags);
        Controls.Add(_checkBox);
        Controls.Add(_titleLabel);
        Controls.Add(_descriptionLabel);
        Resize += (_, _) => UpdateTextBounds();
        Layout += (_, _) => UpdateTextBounds();
        AccentEdge = selected;
        SurfaceStyle = selected ? SurfaceStyle.Raised : SurfaceStyle.Default;
        UpdateTextBounds();
    }

    public string ItemId { get; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Selected
    {
        get => _checkBox.Checked;
        set => _checkBox.Checked = value;
    }

    public event EventHandler? SelectionChanged;

    private void UpdateTextBounds()
    {
        var textWidth = Math.Max(220, _tags.Left - _titleLabel.Left - 28);
        _titleLabel.Width = textWidth;
        _descriptionLabel.Width = textWidth;
    }
}
