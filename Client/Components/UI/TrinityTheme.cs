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
            AppbarBackground = "#161618",
            AppbarText = "rgba(255,255,255,0.87)",
            TextPrimary = "rgba(255,255,255,0.87)",
            TextSecondary = "rgba(255,255,255,0.60)",
            Divider = "rgba(255,255,255,0.12)",
            TableLines = "rgba(255,255,255,0.08)",
            TableHover = "rgba(255,255,255,0.04)",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
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
