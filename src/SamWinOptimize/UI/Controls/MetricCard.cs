namespace SamWinOptimize.UI.Controls;

public sealed class MetricCard : SurfacePanel
{
    private readonly Label _valueLabel;
    private readonly Label _hintLabel;

    public MetricCard(string glyph, string title)
    {
        Size = new Size(230, 168);
        Padding = new Padding(26);
        Margin = new Padding(0, 0, 12, 0);
        SurfaceStyle = SurfaceStyle.Quiet;
        Hoverable = true;

        var icon = new Label
        {
            Text = glyph,
            Font = Theme.IconFont(14),
            ForeColor = Theme.Accent,
            Size = new Size(32, 30),
            Location = new Point(24, 22),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.Font(9, FontStyle.Bold),
            ForeColor = Theme.TextMuted,
            AutoSize = true,
            Location = new Point(58, 26),
            BackColor = Color.Transparent
        };

        _valueLabel = new Label
        {
            Text = "\u8bfb\u53d6\u4e2d\u2026",
            Font = Theme.DisplayFont(19, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoEllipsis = true,
            Location = new Point(24, 62),
            Size = new Size(200, 40),
            BackColor = Color.Transparent
        };

        _hintLabel = new Label
        {
            Text = "\u6b63\u5728\u68c0\u6d4b\u8bbe\u5907",
            Font = Theme.Font(9),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            Location = new Point(25, 114),
            Size = new Size(200, 28),
            BackColor = Color.Transparent
        };

        Controls.Add(icon);
        Controls.Add(titleLabel);
        Controls.Add(_valueLabel);
        Controls.Add(_hintLabel);
        Resize += (_, _) =>
        {
            _valueLabel.Width = Math.Max(80, ClientSize.Width - 50);
            _hintLabel.Width = Math.Max(80, ClientSize.Width - 50);
        };
    }

    public void SetValue(string value, string hint)
    {
        _valueLabel.Text = value;
        _hintLabel.Text = hint;
    }
}
