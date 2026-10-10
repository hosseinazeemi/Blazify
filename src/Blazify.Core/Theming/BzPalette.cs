namespace BzCore.Theming;

public sealed class BzPalette
{
    public string Primary { get; set; } = "#2563eb";
    public string OnPrimary { get; set; } = "#ffffff";
    public string Secondary { get; set; } = "#6b7280";
    public string OnSecondary { get; set; } = "#ffffff";

    public string Success { get; set; } = "#16a34a";
    public string Info { get; set; } = "#0ea5e9";
    public string Warning { get; set; } = "#f59e0b";
    public string Danger { get; set; } = "#dc2626";
    public string OnStatus { get; set; } = "#ffffff";

    public string Background { get; set; } = "#f8fafc";
    public string Surface { get; set; } = "#ffffff";
    public string SurfaceMuted { get; set; } = "#f1f5f9";
    public string Border { get; set; } = "#e2e8f0";

    public string Text { get; set; } = "#1e293b";
    public string TextMuted { get; set; } = "#6b7280";
    public string TextInverse { get; set; } = "#ffffff";

    public string Overlay { get; set; } = "rgba(15,23,42,.45)";
    public string FocusRing { get; set; } = "rgba(47,111,237,.35)";

    public string ToCssVariables() =>
        $"--bz-primary:{Primary};--bz-on-primary:{OnPrimary};" +
        $"--bz-secondary:{Secondary};--bz-on-secondary:{OnSecondary};" +
        $"--bz-success:{Success};--bz-info:{Info};--bz-warning:{Warning};--bz-danger:{Danger};--bz-on-status:{OnStatus};" +
        $"--bz-background:{Background};--bz-surface:{Surface};--bz-surface-muted:{SurfaceMuted};--bz-border:{Border};" +
        $"--bz-text:{Text};--bz-text-muted:{TextMuted};--bz-text-inverse:{TextInverse};" +
        $"--bz-overlay:{Overlay};--bz-focus-ring:{FocusRing};";
}
