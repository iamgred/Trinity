using MudBlazor;

namespace Client.Components.UI;

public static class TrinityTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#F57A1E",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#A855F7",
            Info = "#3B82F6",
            Success = "#35C759",
            Warning = "#F5C518",
            Error = "#E5484D",
            Background = "#0E0E10",
            BackgroundGray = "#161618",
            Surface = "#18181B",
            DrawerBackground = "#111113",
            DrawerText = "rgba(255,255,255,0.87)",
            AppbarBackground = "#F57A1E",
            AppbarText = "#141416",
            TextPrimary = "#F5F5F7",
            TextSecondary = "#9A9AA2",
            TextDisabled = "#6C6C74",
            ActionDefault = "#9A9AA2",
            Divider = "#2C2C30",
            LinesDefault = "#2C2C30",
            LinesInputs = "#2C2C30",
            TableLines = "rgba(44,44,48,0.6)",
            TableHover = "rgba(255,255,255,0.02)",
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
                FontSize = ".78125rem",
                FontWeight = "600",
                TextTransform = "none",
                LetterSpacing = "0",
            },
            H5 = new H5Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = "1.25rem", FontWeight = "700", LineHeight = "1.3" },
            H6 = new H6Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = "1.0625rem", FontWeight = "700", LineHeight = "1.3" },
            Body1 = new Body1Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".875rem" },
            Body2 = new Body2Typography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".8125rem" },
            Caption = new CaptionTypography { FontFamily = ["Inter", "system-ui", "sans-serif"], FontSize = ".75rem" },
            Overline = new OverlineTypography { FontFamily = ["JetBrains Mono", "monospace"], FontSize = ".6875rem", FontWeight = "600", LetterSpacing = ".16em" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            AppbarHeight = "40px",
        },
    };

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
