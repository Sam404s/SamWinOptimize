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
            AutoEllipsis = false,
            AutoSize = false,
            WordWrap = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(24, 58),
            Size = new Size(200, 48),
            BackColor = Color.Transparent
        };

        _hintLabel = new BufferedLabel
        {
            Text = "\u6b63\u5728\u68c0\u6d4b\u8bbe\u5907",
            Font = Theme.Font(9),
            ForeColor = Theme.TextSecondary,
            AutoEllipsis = false,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(25, 108),
            Size = new Size(200, 28),
            BackColor = Color.Transparent
        };

        Controls.Add(icon);
        Controls.Add(titleLabel);
        Controls.Add(_valueLabel);
        Controls.Add(_hintLabel);
        Resize += (_, _) =>
        {
            var textWidth = AvailableTextWidth;
            _valueLabel.Width = textWidth;
            _hintLabel.Width = textWidth;
            FitTypography(textWidth);
        };
    }

    public void SetValue(string value, string hint)
    {
        _valueLabel.Text = value;
        _hintLabel.Text = hint;
        FitTypography(AvailableTextWidth);
        _valueLabel.Invalidate();
        _hintLabel.Invalidate();
    }

    private int AvailableTextWidth => Math.Max(80, Width - 50);

    private void FitTypography(int textWidth)
    {
        _valueLabel.Font = FitFont(
            _valueLabel.Text,
            textWidth,
            15.5f,
            9.25f,
            bold: true,
            display: true);
        _hintLabel.Font = FitFont(
            _hintLabel.Text,
            textWidth,
            9f,
            7.8f,
            bold: false,
            display: false);
    }

    private static Font FitFont(
        string text,
        int width,
        float preferredSize,
        float minimumSize,
        bool bold,
        bool display)
    {
        var style = bold ? FontStyle.Bold : FontStyle.Regular;
        for (var size = preferredSize; size >= minimumSize; size -= 0.25f)
        {
            var font = display
                ? Theme.DisplayFont(size, style)
                : Theme.Font(size, style);
            var measured = TextRenderer.MeasureText(
                text,
                font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine).Width;
            if (measured <= width)
            {
                return font;
            }

            font.Dispose();
        }

        return display
            ? Theme.DisplayFont(minimumSize, style)
            : Theme.Font(minimumSize, style);
    }
}
