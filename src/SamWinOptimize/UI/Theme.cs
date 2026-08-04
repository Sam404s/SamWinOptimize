using System.Drawing.Drawing2D;

namespace SamWinOptimize.UI;

/// <summary>
/// Design tokens for the SamWinOptimize visual system.
/// Direction: light macOS utility, quiet surfaces, restrained blue accents.
/// </summary>
internal static class Theme
{
    // ── macOS-inspired canvas ────────────────────────────────────
    public static readonly Color Canvas = Color.FromArgb(246, 246, 248);       // #F6F6F8
    public static readonly Color CanvasSoft = Color.FromArgb(239, 239, 244);   // #EFEFF4
    public static readonly Color CanvasRaised = Color.White;                    // #FFFFFF
    public static readonly Color Sidebar = Color.FromArgb(242, 242, 247);      // #F2F2F7
    public static readonly Color SidebarRaised = Color.FromArgb(235, 235, 241); // #EBEBF1

    // ── Quiet utility surfaces ───────────────────────────────────
    public static readonly Color Surface = Color.White;                         // #FFFFFF
    public static readonly Color SurfaceRaised = Color.White;                    // #FFFFFF
    public static readonly Color SurfaceHover = Color.FromArgb(242, 246, 252); // #F2F6FC
    public static readonly Color SurfaceStrong = Color.FromArgb(231, 233, 238); // #E7E9EE
    public static readonly Color GlassTop = Color.White;
    public static readonly Color GlassBottom = Color.FromArgb(250, 250, 252);
    public static readonly Color GlassHighlight = Color.White;
    public static readonly Color Shadow = Color.FromArgb(24, 0, 0, 0);

    // ── Text ─────────────────────────────────────────────────────
    public static readonly Color TextPrimary = Color.FromArgb(29, 29, 31);      // #1D1D1F
    public static readonly Color TextSecondary = Color.FromArgb(91, 95, 105);  // #5B5F69
    public static readonly Color TextMuted = Color.FromArgb(142, 142, 147);    // #8E8E93

    // ── Borders ──────────────────────────────────────────────────
    public static readonly Color Border = Color.FromArgb(218, 218, 224);        // #DADAE0
    public static readonly Color BorderStrong = Color.FromArgb(194, 195, 202);  // #C2C3CA
    public static readonly Color BorderGlow = Color.FromArgb(94, 156, 219);     // #5E9CDB

    // ── Accent: calm system blue ─────────────────────────────────
    public static readonly Color Accent = Color.FromArgb(10, 132, 255);         // #0A84FF
    public static readonly Color AccentStrong = Color.FromArgb(0, 112, 232);    // #0070E8
    public static readonly Color AccentWash = Color.FromArgb(232, 242, 255);    // #E8F2FF
    public static readonly Color AccentSoft = Color.FromArgb(74, 145, 224);     // #4A91E0
    public static readonly Color AccentDeep = Color.FromArgb(0, 88, 190);       // #0058BE

    // ── Semantic ─────────────────────────────────────────────────
    public static readonly Color Success = Color.FromArgb(38, 142, 91);         // #268E5B
    public static readonly Color Warning = Color.FromArgb(177, 116, 20);       // #B17414
    public static readonly Color Danger = Color.FromArgb(207, 67, 67);          // #CF4343
    public static readonly Color DangerSurface = Color.FromArgb(253, 238, 238); // #FDEEEE
    public static readonly Color Info = Color.FromArgb(96, 86, 164);            // #6056A4

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
