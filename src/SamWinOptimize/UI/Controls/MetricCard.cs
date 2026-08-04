namespace SamWinOptimize.UI.Controls;

public sealed class MetricCard : SurfacePanel
{
    private readonly BufferedLabel _valueLabel;
    private readonly BufferedLabel _hintLabel;

    public MetricCard(string glyph, string title)
    {
        Size = new Size(230, 168);
        Padding = new Padding(26);
        Margin = new Padding(0, 0, 12, 0);
        SurfaceStyle = SurfaceStyle.Quiet;
        Hoverable = true;

        var icon = new BufferedLabel
        {
            Text = glyph,
            Font = Theme.IconFont(14),
            ForeColor = Theme.Accent,
            AutoSize = false,
            Size = new Size(32, 30),
            Location = new Point(24, 20),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };

        var titleLabel = new BufferedLabel
        {
            Text = title,
            Font = Theme.Font(9, FontStyle.Bold),
            ForeColor = Theme.TextMuted,
            AutoSize = false,
            Location = new Point(58, 20),
            Size = new Size(140, 30),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };

        _valueLabel = new BufferedLabel
        {
            Text = "\u8bfb\u53d6\u4e2d\u2026",
            Font = Theme.DisplayFont(15, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            AutoEllipsis = true,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(24, 58),
            Size = new Size(200, 38),
            BackColor = Color.Transparent
        };

        _hintLabel = new BufferedLabel
        {
            Text = "\u6b63\u5728\u68c0\u6d4b\u8bbe\u5907",
            Font = Theme.Font(9),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = true,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(25, 108),
            Size = new Size(200, 30),
            BackColor = Color.Transparent
        };

        Controls.Add(icon);
        Controls.Add(titleLabel);
        Controls.Add(_valueLabel);
        Controls.Add(_hintLabel);
        Resize += (_, _) =>
        {
            var textWidth = Math.Max(80, ClientSize.Width - 50);
            _valueLabel.Width = textWidth;
            _hintLabel.Width = textWidth;
            _valueLabel.Font = Theme.DisplayFont(ClientSize.Width < 190 ? 13.5f : 15.5f, FontStyle.Bold);
        };
    }

    public void SetValue(string value, string hint)
    {
        _valueLabel.Text = value;
        _hintLabel.Text = hint;
        _valueLabel.Invalidate();
        _hintLabel.Invalidate();
    }
}
