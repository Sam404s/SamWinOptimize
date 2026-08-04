using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI;

/// <summary>
/// Design tokens for the SamWinOptimize visual system.
/// Direction: atmospheric dark glass, cyan energy accents, quiet depth.
/// </summary>
internal static class Theme
{
    // ── Atmospheric canvas ───────────────────────────────────────
    public static readonly Color Canvas = Color.FromArgb(7, 16, 29);          // #07101D
    public static readonly Color CanvasSoft = Color.FromArgb(10, 27, 44);     // #0A1B2C
    public static readonly Color CanvasRaised = Color.FromArgb(13, 35, 55);   // #0D2337
    public static readonly Color Sidebar = Color.FromArgb(8, 22, 38);         // #081626
    public static readonly Color SidebarRaised = Color.FromArgb(14, 35, 57);  // #0E2339

    // ── Glass surfaces ───────────────────────────────────────────
    public static readonly Color Surface = Color.FromArgb(16, 39, 59);        // #10273B
    public static readonly Color SurfaceRaised = Color.FromArgb(20, 51, 74);   // #14334A
    public static readonly Color SurfaceHover = Color.FromArgb(26, 67, 91);    // #1A435B
    public static readonly Color SurfaceStrong = Color.FromArgb(32, 78, 104);  // #204E68
    public static readonly Color GlassTop = Color.FromArgb(25, 61, 84);
    public static readonly Color GlassBottom = Color.FromArgb(11, 29, 48);
    public static readonly Color GlassHighlight = Color.FromArgb(80, 176, 213);
    public static readonly Color Shadow = Color.FromArgb(90, 0, 0, 0);

    // ── Text ─────────────────────────────────────────────────────
    public static readonly Color TextPrimary = Color.FromArgb(238, 248, 255);  // #EEF8FF
    public static readonly Color TextSecondary = Color.FromArgb(164, 190, 210);// #A4BED2
    public static readonly Color TextMuted = Color.FromArgb(103, 137, 163);    // #6789A3

    // ── Borders ──────────────────────────────────────────────────
    public static readonly Color Border = Color.FromArgb(34, 76, 101);        // #224C65
    public static readonly Color BorderStrong = Color.FromArgb(61, 111, 139);  // #3D6F8B
    public static readonly Color BorderGlow = Color.FromArgb(93, 211, 242);

    // ── Accent: electric cyan / deep blue ────────────────────────
    public static readonly Color Accent = Color.FromArgb(67, 222, 255);       // #43DEFF
    public static readonly Color AccentStrong = Color.FromArgb(15, 146, 211);  // #0F92D3
    public static readonly Color AccentWash = Color.FromArgb(18, 62, 82);      // #123E52
    public static readonly Color AccentSoft = Color.FromArgb(142, 239, 255);   // #8EEFFF
    public static readonly Color AccentDeep = Color.FromArgb(8, 75, 117);      // #084B75

    // ── Semantic ─────────────────────────────────────────────────
    public static readonly Color Success = Color.FromArgb(87, 226, 178);      // #57E2B2
    public static readonly Color Warning = Color.FromArgb(255, 204, 102);     // #FFCC66
    public static readonly Color Danger = Color.FromArgb(255, 112, 149);      // #FF7095
    public static readonly Color DangerSurface = Color.FromArgb(68, 35, 56);   // #442338
    public static readonly Color Info = Color.FromArgb(161, 143, 255);         // #A18FFF

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
    public const int RadiusLg = 18;
    public const int RadiusXl = 24;

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
            SelectionForeColor = TextSecondary,
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
            BackColor = SurfaceRaised,
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
        input.BackColor = SurfaceRaised;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.Margin = new Padding(0);
    }

    public static void StyleDropDown(ComboBox input)
    {
        input.Font = Font(9.5f);
        input.ForeColor = TextPrimary;
        input.BackColor = SurfaceRaised;
        input.FlatStyle = FlatStyle.Flat;
        input.Margin = new Padding(0);
    }

    public static string RiskLabel(Models.RiskLevel risk) => risk switch
    {
        Models.RiskLevel.Low => "低风险",
        Models.RiskLevel.Medium => "需确认",
        Models.RiskLevel.High => "高风险",
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
