using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI.Controls;

/// <summary>
/// A native-feeling light text field with a quiet focus ring and fixed content
/// bounds. The editor never reaches the trailing search affordance.
/// </summary>
public sealed class GlassTextBox : UserControl
{
    private readonly TextBox _editor;
    private readonly BufferedLabel _placeholder;
    private bool _focused;

    public GlassTextBox()
    {
        Height = 42;
        MinimumSize = new Size(180, 42);
        BackColor = Color.Transparent;
        AccessibleRole = AccessibleRole.Text;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;

        _editor = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            ForeColor = Theme.TextPrimary,
            Font = Theme.Font(9.8f),
            Location = new Point(15, 9),
            Height = 24,
            TabStop = true
        };
        _editor.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _editor.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _editor.KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode == Keys.Escape && !string.IsNullOrEmpty(_editor.Text))
            {
                _editor.Clear();
                eventArgs.SuppressKeyPress = true;
            }
        };

        _placeholder = new BufferedLabel
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.TextMuted,
            Font = Theme.Font(9.8f),
            BackColor = Color.Transparent,
            Text = "搜索名称或说明",
            Location = new Point(15, 8),
            Height = 26,
            Cursor = Cursors.IBeam
        };
        _placeholder.Click += (_, _) => _editor.Focus();
        _placeholder.Visible = true;
        _editor.TextChanged += (_, _) =>
        {
            _placeholder.Visible = string.IsNullOrEmpty(_editor.Text);
            Invalidate();
            OnTextChanged(EventArgs.Empty);
        };

        Controls.Add(_editor);
        Controls.Add(_placeholder);
        Resize += (_, _) => LayoutEditor();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [AllowNull]
    public override string Text
    {
        get => _editor.Text;
        set => _editor.Text = value ?? string.Empty;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => _placeholder.Text;
        set => _placeholder.Text = value ?? string.Empty;
    }

    protected override void OnClick(EventArgs eventArgs)
    {
        _editor.Focus();
        base.OnClick(eventArgs);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        base.OnPaintBackground(eventArgs);
        var graphics = eventArgs.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = Theme.RoundedRectangle(bounds, Theme.RadiusSm);
        using var fill = new SolidBrush(Color.White);
        graphics.FillPath(fill, path);
        using var border = new Pen(_focused ? Theme.Accent : Theme.BorderStrong, _focused ? 1.35f : 1f);
        graphics.DrawPath(border, path);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);
        using var iconPen = new Pen(Theme.TextMuted, 1.25f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        var center = new Point(Width - 20, Height / 2);
        eventArgs.Graphics.DrawEllipse(iconPen, center.X - 5, center.Y - 5, 9, 9);
        eventArgs.Graphics.DrawLine(iconPen, center.X + 2, center.Y + 2, center.X + 6, center.Y + 6);
    }

    private void LayoutEditor()
    {
        _editor.Location = new Point(15, 8);
        _editor.Width = Math.Max(40, ClientSize.Width - 42);
        _editor.Height = Math.Max(22, ClientSize.Height - 16);
        _placeholder.Location = _editor.Location;
        _placeholder.Size = _editor.Size;
    }
}

/// <summary>
/// A compact glass select control with a fixed-width themed popup. The popup
/// owns its width from the longest item so CJK labels remain single-line rather
/// than collapsing into one glyph per row.
/// </summary>
public sealed class GlassSelect : UserControl
{
    private readonly BufferedLabel _valueLabel;
    private readonly ContextMenuStrip _menu;
    private readonly List<string> _items = [];
    private int _selectedIndex = -1;
    private bool _hovered;
    private bool _focused;

    public GlassSelect()
    {
        Height = 42;
        MinimumSize = new Size(140, 42);
        BackColor = Color.Transparent;
        AccessibleRole = AccessibleRole.ComboBox;
        TabStop = true;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;

        _valueLabel = new BufferedLabel
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.TextPrimary,
            Font = Theme.Font(9.5f),
            BackColor = Color.Transparent,
            Padding = new Padding(14, 0, 30, 0),
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand
        };
        _valueLabel.Click += (_, _) => ShowMenu();
        Controls.Add(_valueLabel);

        _menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            ShowCheckMargin = false,
            AutoSize = false,
            BackColor = Color.White,
            ForeColor = Theme.TextPrimary,
            Renderer = new GlassMenuRenderer(),
            Font = Theme.Font(9.5f),
            Padding = new Padding(4),
            DropShadowEnabled = true
        };
        _menu.Closed += (_, _) => Invalidate();
        GotFocus += (_, _) => { _focused = true; Invalidate(); };
        LostFocus += (_, _) => { _focused = false; Invalidate(); };
        MouseEnter += (_, _) => { _hovered = true; Invalidate(); };
        MouseLeave += (_, _) => { _hovered = false; Invalidate(); };
        Resize += (_, _) => Invalidate();
    }

    public event EventHandler? SelectedIndexChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var next = value < 0 || value >= _items.Count ? -1 : value;
            if (_selectedIndex == next)
            {
                return;
            }

            _selectedIndex = next;
            _valueLabel.Text = next >= 0 ? _items[next] : string.Empty;
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public string? SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count
        ? _items[_selectedIndex]
        : null;

    public void AddItem(string item)
    {
        _items.Add(item);
        RebuildMenu();
        if (_selectedIndex < 0)
        {
            SelectedIndex = 0;
        }
    }

    public void AddItems(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            _items.Add(item);
        }
        RebuildMenu();
        if (_selectedIndex < 0 && _items.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    protected override void OnKeyDown(KeyEventArgs eventArgs)
    {
        if (eventArgs.KeyCode is Keys.Enter or Keys.Space or Keys.Down)
        {
            ShowMenu();
            eventArgs.Handled = true;
            return;
        }
        base.OnKeyDown(eventArgs);
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(eventArgs);
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        _hovered = false;
        Invalidate();
        base.OnMouseLeave(eventArgs);
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButtons.Left)
        {
            Focus();
            ShowMenu();
        }
        base.OnMouseDown(eventArgs);
    }

    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        base.OnPaintBackground(eventArgs);
        var graphics = eventArgs.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = Theme.RoundedRectangle(bounds, Theme.RadiusSm);
        using var fill = new SolidBrush(_hovered ? Theme.SurfaceHover : Color.White);
        graphics.FillPath(fill, path);
        using var border = new Pen(_focused || _hovered ? Theme.Accent : Theme.BorderStrong,
            _focused ? 1.35f : 1f);
        graphics.DrawPath(border, path);
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);
        var graphics = eventArgs.Graphics;
        using var pen = new Pen(Theme.TextSecondary, 1.6f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        var x = Width - 19;
        var y = Height / 2 - 3;
        graphics.DrawLine(pen, x - 4, y, x, y + 4);
        graphics.DrawLine(pen, x, y + 4, x + 4, y);
    }

    private void ShowMenu()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _menu.Show(this, new Point(0, Height + 4));
    }

    private void RebuildMenu()
    {
        _menu.Items.Clear();
        var menuWidth = Math.Clamp(
            _items.Select(item => TextRenderer.MeasureText(item, _menu.Font,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine).Width)
                .DefaultIfEmpty(0)
                .Max() + 48,
            168,
            320);
        _menu.Width = menuWidth;

        for (var index = 0; index < _items.Count; index++)
        {
            var itemIndex = index;
            var item = new ToolStripMenuItem(_items[index])
            {
                Checked = index == _selectedIndex,
                CheckOnClick = false,
                AutoSize = false,
                Width = menuWidth - _menu.Padding.Horizontal,
                Height = 38,
                Padding = new Padding(14, 0, 18, 0),
                Margin = Padding.Empty,
                BackColor = Theme.SurfaceRaised,
                ForeColor = Theme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            item.Click += (_, _) => SelectedIndex = itemIndex;
            _menu.Items.Add(item);
        }

        _menu.Height = _menu.Padding.Vertical + (_items.Count * 38);
    }

    private sealed class GlassMenuRenderer : ToolStripProfessionalRenderer
    {
        public GlassMenuRenderer() : base(new GlassColorTable())
        {
            RoundedEdges = true;
        }
    }

    private sealed class GlassColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Theme.SurfaceRaised;
        public override Color MenuBorder => Theme.BorderStrong;
        public override Color MenuItemBorder => Theme.Accent;
        public override Color MenuItemSelected => Theme.SurfaceHover;
        public override Color MenuItemSelectedGradientBegin => Theme.SurfaceHover;
        public override Color MenuItemSelectedGradientEnd => Theme.SurfaceHover;
        public override Color ImageMarginGradientBegin => Theme.SurfaceRaised;
        public override Color ImageMarginGradientMiddle => Theme.SurfaceRaised;
        public override Color ImageMarginGradientEnd => Theme.SurfaceRaised;
    }
}
