using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Helpers;

namespace BzCore.Components.Loading;
public partial class BzSpinner : BzComponentBase
{
    /// <summary>Animation type</summary>
    [Parameter] public BzSpinnerVariant Variant { get; set; } = BzSpinnerVariant.Ring;

    /// <summary>Size example 40px</summary>
    [Parameter] public string Size { get; set; } = "40px";

    /// <summary>Spinner color</summary>
    [Parameter] public BzColor Color { get; set; } = BzColor.Primary;

    /// <summary>When not null, will replace with <see cref="Color"/> </summary>
    [Parameter] public string? CustomColor { get; set; }

    /// <summary>Animation duration: "1s" / "100ms"</summary>
    [Parameter] public string Speed { get; set; } = "1s";

    /// <summary>Optional caption</summary>
    [Parameter] public string? Label { get; set; }
    /// <summary>Centers the spinner over the viewport until Visible becomes false.</summary>
    [Parameter] public bool FullScreen { get; set; }
    [Parameter] public bool Backdrop { get; set; } = true;
    [Parameter] public bool BlockInteraction { get; set; } = true;

    private string ColorValue => CustomColor ?? ColorHelper.Var(Color);

    private string BoxStyle =>
        $"--bz-sp-size:{Size};--bz-sp-color:{ColorValue};--bz-sp-speed:{Speed};" + (Style ?? "");

    private string RootClass =>
        RootCss("bz-sp").Add($"bz-sp--{Variant.ToString().ToLowerInvariant()}")
            .AddIf(FullScreen, "bz-sp--fullscreen")
            .AddIf(FullScreen && BlockInteraction, "bz-sp--blocking").Build();
}
