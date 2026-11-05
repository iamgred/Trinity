using MudBlazor;

namespace Client.Components.UI;

// one dark theme for all the MudBlazor layouts (same orange as MainLayout)
public static class TrinityTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#E87722",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#A855F7",
            Info = "#3B82F6",
            Success = "#35C759",
            Warning = "#F5C518",
            Error = "#E5484D",
            Background = "#0E0E10",
            BackgroundGray = "#161618",
            Surface = "#1C1C1F",
            DrawerBackground = "#111111",
            DrawerText = "rgba(255,255,255,0.87)",
            AppbarBackground = "#F57A1E",
            AppbarText = "#141416",
            TextPrimary = "rgba(255,255,255,0.90)",
            TextSecondary = "rgba(255,255,255,0.55)",
            Divider = "rgba(255,255,255,0.07)",
            LinesDefault = "rgba(255,255,255,0.07)",
            LinesInputs = "rgba(255,255,255,0.10)",
            TableLines = "rgba(255,255,255,0.06)",
            TableHover = "rgba(255,255,255,0.03)",
            OverlayDark = "rgba(5,5,6,0.66)",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "system-ui", "-apple-system", "Segoe UI", "sans-serif"],
                FontSize = ".8125rem",
                LineHeight = "1.45",
            },
            Button = new ButtonTypography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = ".8125rem",
                FontWeight = "500",
                TextTransform = "none",
                LetterSpacing = "0",
            },
            H5 = new H5Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = "1.25rem", FontWeight = "600", LineHeight = "1.3" },
            H6 = new H6Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".9375rem", FontWeight = "600", LineHeight = "1.3" },
            Body1 = new Body1Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".875rem" },
            Body2 = new Body2Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".8125rem" },
            Caption = new CaptionTypography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".75rem" },
            Overline = new OverlineTypography { FontFamily = ["JetBrains Mono", "monospace"], FontSize = ".6875rem", FontWeight = "500", LetterSpacing = ".16em" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
            AppbarHeight = "40px",
        },
    };

    // tone names -> hex, for charts (they take raw colours, not Color)
    public static string Hex(string? tone) => tone?.ToLowerInvariant() switch
    {
        "green" => "#35C759",
        "red" => "#E5484D",
        "yellow" => "#F5C518",
        "blue" => "#3B82F6",
        "orange" => "#E87722",
        "purple" => "#A855F7",
        _ => "#6C6C74",
    };

    // tone names the pages use -> MudBlazor colours
    public static Color ToColor(string? tone) => tone?.ToLowerInvariant() switch
    {
        "green" => Color.Success,
        "red" => Color.Error,
        "yellow" => Color.Warning,
        "blue" => Color.Info,
        "orange" => Color.Primary,
        "purple" => Color.Secondary,
        _ => Color.Default,
    };
}
