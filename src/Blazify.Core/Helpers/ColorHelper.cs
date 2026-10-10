using BzCore.Enums;

namespace BzCore.Helpers;

public static class ColorHelper
{
    public static string Var(BzColor color) => color switch
    {
        BzColor.Primary => "var(--bz-primary)",
        BzColor.Secondary => "var(--bz-secondary)",
        BzColor.Success => "var(--bz-success)",
        BzColor.Info => "var(--bz-info)",
        BzColor.Warning => "var(--bz-warning)",
        BzColor.Danger => "var(--bz-danger)",
        BzColor.Surface => "var(--bz-surface)",
        _ => "var(--bz-text)"
    };

    public static string OnVar(BzColor color) => color switch
    {
        BzColor.Primary => "var(--bz-on-primary)",
        BzColor.Secondary => "var(--bz-on-secondary)",
        BzColor.Success or BzColor.Info or BzColor.Warning or BzColor.Danger => "var(--bz-on-status)",
        _ => "var(--bz-text)"
    };
}
