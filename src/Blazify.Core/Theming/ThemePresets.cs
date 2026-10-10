using BzCore.Enums;

namespace BzCore.Theming;

public static class ThemePresets
{
    public static BzTheme Resolve(BzThemeKind kind) => kind switch
    {
        BzThemeKind.Dark => Dark(),
        BzThemeKind.LightGray => LightGray(),
        BzThemeKind.LightPurple => LightPurple(),
        _ => Light()
    };

    public static BzTheme Light() => new BzTheme
    {
        Name = "light",
        Palette = new BzPalette
        {
            Primary = "#2563eb",
            OnPrimary = "#ffffff",
            Secondary = "#64748b",
            OnSecondary = "#ffffff",
            Background = "#f8fafc",
            Surface = "#ffffff",
            SurfaceMuted = "#f1f5f9",
            Border = "#e2e8f0",
            Text = "#1e293b",
            TextMuted = "#6b7280",
            FocusRing = "rgba(37,99,235,.32)"
        }
    }.WithAppearance(BzAppearance.Modern);

    public static BzTheme Dark() => new BzTheme
    {
        Name = "dark",
        Palette = new BzPalette
        {
            Primary = "#60a5fa",
            OnPrimary = "#0f172a",
            Secondary = "#94a3b8",
            OnSecondary = "#0f172a",
            Success = "#22c55e",
            Info = "#38bdf8",
            Warning = "#fbbf24",
            Danger = "#f87171",
            Background = "#020617",
            Surface = "#0f172a",
            SurfaceMuted = "#1e293b",
            Border = "#334155",
            Text = "#f1f5f9",
            TextMuted = "#94a3b8",
            TextInverse = "#0f172a",
            Overlay = "rgba(2,6,23,.6)",
            FocusRing = "rgba(96,165,250,.4)"
        }
    }.WithAppearance(BzAppearance.Modern);

    public static BzTheme LightGray() => new BzTheme
    {
        Name = "light_gray",
        Palette = new BzPalette
        {
            Primary = "#475569",
            OnPrimary = "#ffffff",
            Secondary = "#94a3b8",
            OnSecondary = "#ffffff",
            Background = "#e2e8f0",
            Surface = "#f8fafc",
            SurfaceMuted = "#f1f5f9",
            Border = "#cbd5e1",
            Text = "#1e293b",
            TextMuted = "#64748b",
            FocusRing = "rgba(71,85,105,.3)"
        }
    }.WithAppearance(BzAppearance.Official);

    public static BzTheme LightPurple() => new BzTheme
    {
        Name = "light_purple",
        Palette = new BzPalette
        {
            Primary = "#4f46e5",
            OnPrimary = "#ffffff",
            Secondary = "#a5b4fc",
            OnSecondary = "#1e1b4b",
            Success = "#16a34a",
            Info = "#0ea5e9",
            Warning = "#f59e0b",
            Danger = "#e11d48",
            Background = "#eef2ff",
            Surface = "#ffffff",
            SurfaceMuted = "#e0e7ff",
            Border = "#c7d2fe",
            Text = "#1e1b4b",
            TextMuted = "#6366f1",
            FocusRing = "rgba(79,70,229,.32)"
        }
    }.WithAppearance(BzAppearance.Modern);
}
