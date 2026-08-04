using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI;

/// <summary>
/// Design tokens for the SamWinOptimize visual system.
/// Palette: fresh blue-slate, airy spacing, soft borders.
/// </summary>
internal static class Theme
{
    // ── Backgrounds ──────────────────────────────────────────────
    public static readonly Color Canvas = Color.FromArgb(248, 250, 252);       // #F8FAFC
    public static readonly Color CanvasSoft = Color.FromArgb(241, 245, 249);   // #F1F5F9
    public static readonly Color Sidebar = Color.FromArgb(255, 255, 255);      // #FFFFFF
    public static readonly Color SidebarRaised = Color.FromArgb(248, 250, 252);// #F8FAFC
    public static readonly Color Surface = Color.FromArgb(255, 255, 255);      // #FFFFFF
    public static readonly Color SurfaceRaised = Color.FromArgb(248, 250, 252);// #F8FAFC
    public static readonly Color SurfaceHover = Color.FromArgb(241, 245, 249); // #F1F5F9
    public static readonly Color SurfaceStrong = Color.FromArgb(226, 232, 240);// #E2E8F0

    // ── Text ─────────────────────────────────────────────────────
    public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);     // #0F172A
    public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);  // #475569
    public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);    // #94A3B8

    // ── Borders ──────────────────────────────────────────────────
    public static readonly Color Border = Color.FromArgb(226, 232, 240);       // #E2E8F0
    public static readonly Color BorderStrong = Color.FromArgb(203, 213, 225); // #CBD5E1

    // ── Accent (clean blue) ─────────────────────────────────────
    public static readonly Color Accent = Color.FromArgb(59, 130, 246);        // #3B82F6
    public static readonly Color AccentStrong = Color.FromArgb(37, 99, 235);   // #2563EB
    public static readonly Color AccentWash = Color.FromArgb(239, 246, 255);   // #EFF6FF
    public static readonly Color AccentSoft = Color.FromArgb(191, 219, 254);   // #BFDBFE

    // ── Semantic ─────────────────────────────────────────────────
    public static readonly Color Success = Color.FromArgb(16, 185, 129);       // #10B981
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);       // #F59E0B
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);         // #EF4444
    public static readonly Color DangerSurface = Color.FromArgb(254, 242, 242);// #FEF2F2
    public static readonly Color Info = Color.FromArgb(99, 102, 241);          // #6366F1

    // ── Spacing tokens ───────────────────────────────────────────
    public const int SpacingXs = 4;
    public const int SpacingSm = 8;
    public const int SpacingMd = 16;
    public const int SpacingLg = 24;
    public const int SpacingXl = 32;
    public const int Spacing2Xl = 48;

    // ── Radius tokens ────────────────────────────────────────────
    public const int RadiusSm = 8;
    public const int RadiusMd = 12;
    public const int RadiusLg = 16;
    public const int RadiusXl = 20;

    // ── Typography ───────────────────────────────────────────────
    public static Font Font(float size, FontStyle style = FontStyle.Regular) =>
        CreateFont("Segoe UI Variable Text", "Microsoft YaHei UI", size, style);

    public static Font DisplayFont(float size, FontStyle style = FontStyle.Regular) =>
        CreateFont("Segoe UI Variable Display", "Segoe UI", size, style);

    public static Font MonoFont(float size, FontStyle style = FontStyle.Regular) =>
        CreateFont("Cascadia Mono", "Consolas", size, style);

    public static Font IconFont(float size) =>
        new("Segoe Fluent Icons", size, FontStyle.Regular, GraphicsUnit.Point);

    // ── Geometry helpers ─────────────────────────────────────────
    public static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var diameter = Math.Max(2, Math.Min(Math.Min(bounds.Width, bounds.Height), radius * 2));
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static Color Blend(Color from, Color to, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return Color.FromArgb(
            (int)(from.A + ((to.A - from.A) * amount)),
            (int)(from.R + ((to.R - from.R) * amount)),
            (int)(from.G + ((to.G - from.G) * amount)),
            (int)(from.B + ((to.B - from.B) * amount)));
    }

    // ── Control styling ──────────────────────────────────────────
    public static void StyleDataGrid(DataGridView grid)
    {
        grid.BackgroundColor = Canvas;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Border;
        grid.ForeColor = TextPrimary;
        grid.Font = Font(9.5f);
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoGenerateColumns = false;
        grid.ColumnHeadersHeight = 52;
        grid.RowTemplate.Height = 58;
        grid.EnableHeadersVisualStyles = false;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = CanvasSoft,
            ForeColor = TextSecondary,
            Font = Font(8.8f, FontStyle.Bold),
            SelectionBackColor = CanvasSoft,
            Padding = new Padding(18, 0, 18, 0)
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = TextPrimary,
            SelectionBackColor = AccentWash,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(18, 0, 18, 0)
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = CanvasSoft,
            ForeColor = TextPrimary,
            SelectionBackColor = AccentWash,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(18, 0, 18, 0)
        };
    }

    public static void StyleTextInput(TextBoxBase input)
    {
        input.Font = Font(10);
        input.ForeColor = TextPrimary;
        input.BackColor = Surface;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.Margin = new Padding(0);
    }

    public static void StyleDropDown(ComboBox input)
    {
        input.Font = Font(9.5f);
        input.ForeColor = TextPrimary;
        input.BackColor = Surface;
        input.FlatStyle = FlatStyle.Flat;
        input.Margin = new Padding(0);
    }

    public static string RiskLabel(Models.RiskLevel risk) => risk switch
    {
        Models.RiskLevel.Low => "\u4f4e\u98ce\u9669",
        Models.RiskLevel.Medium => "\u9700\u786e\u8ba4",
        Models.RiskLevel.High => "\u9ad8\u98ce\u9669",
        _ => throw new ArgumentOutOfRangeException(nameof(risk))
    };

    public static Color RiskColor(Models.RiskLevel risk) => risk switch
    {
        Models.RiskLevel.Low => Success,
        Models.RiskLevel.Medium => Warning,
        Models.RiskLevel.High => Danger,
        _ => throw new ArgumentOutOfRangeException(nameof(risk))
    };

    private static Font CreateFont(string preferred, string fallback, float size, FontStyle style)
    {
        using var preferredFont = new Font(preferred, size, style, GraphicsUnit.Point);
        var family = preferredFont.Name.Equals(preferred, StringComparison.OrdinalIgnoreCase)
            ? preferred
            : fallback;
        return new Font(family, size, style, GraphicsUnit.Point);
    }
}