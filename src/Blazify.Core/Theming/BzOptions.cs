using BzCore.Enums;

namespace BzCore.Theming;

public sealed class BzOptions
{
    public BzThemeKind DefaultTheme { get; set; } = BzThemeKind.Light;

    public BzTheme? Theme { get; set; }

    public BzAppearance Appearance { get; set; } = BzAppearance.Modern;

    public BzDirection Direction { get; set; } = BzDirection.Rtl;

    public BzFont Font { get; set; } = BzFont.Vazirmatn;

    /// <summary>Render the whole Blazify UI in compact mode by default.</summary>
    public bool Dense { get; set; } = true;
}
