using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Helpers;

namespace BzCore.Components.Loading;

// BzProgress — a linear bar or a circular ring showing 0–100% progress.
// Value is always clamped to [0,100]. Colour/size/thickness are handed to CSS
// through custom properties (--bz-pg-*).
public partial class BzProgress : BzComponentBase
{
    /// <summary>Linear bar (default) or Circular ring.</summary>
    [Parameter] public BzProgressType Type { get; set; } = BzProgressType.Linear;

    /// <summary>Current progress, 0–100 (clamped).</summary>
    [Parameter] public double Value { get; set; }

    /// <summary>Show an endless animation instead of a fixed value.</summary>
    [Parameter] public bool Indeterminate { get; set; }

    /// <summary>Semantic colour (overridden by <see cref="CustomColor"/>).</summary>
    [Parameter] public BzColor Color { get; set; } = BzColor.Primary;
    /// <summary>Explicit CSS colour that wins over <see cref="Color"/>.</summary>
    [Parameter] public string? CustomColor { get; set; }

    /// <summary>Draw diagonal stripes (linear only).</summary>
    [Parameter] public bool Striped { get; set; }
    /// <summary>Animate the stripes (linear only).</summary>
    [Parameter] public bool Animated { get; set; }
    /// <summary>Show the numeric percentage label.</summary>
    [Parameter] public bool ShowLabel { get; set; }

    /// <summary>Bar height (linear).</summary>
    [Parameter] public string Thickness { get; set; } = "8px";
    /// <summary>Ring diameter (circular).</summary>
    [Parameter] public string Size { get; set; } = "72px";

    // ----- derived values used by the markup -----
    private double Clamped => Math.Clamp(Value, 0, 100);
    private string ColorValue => CustomColor ?? ColorHelper.Var(Color);
    private string Pct => $"{Clamped:0.#}%";

    private string LinearClass => RootCss("bz-pg bz-pg--linear")
        .AddIf(Indeterminate, "is-indeterminate")
        .AddIf(Striped, "is-striped")
        .AddIf(Animated, "is-animated")
        .Build();

    private string CircularClass => RootCss("bz-pg bz-pg--circular")
        .AddIf(Indeterminate, "is-indeterminate")
        .Build();

    private string LinearStyle => $"--bz-pg-color:{ColorValue};--bz-pg-h:{Thickness};" + (Style ?? "");
    private string CircularStyle =>
        $"--bz-pg-color:{ColorValue};--bz-pg-size:{Size};--bz-pg-val:{Clamped};" + (Style ?? "");
}
