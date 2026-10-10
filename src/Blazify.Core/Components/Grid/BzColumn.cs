using System.Globalization;
using BzCore.Enums;
using Microsoft.AspNetCore.Components;

namespace BzCore.Components.Grid;
public class BzColumn<TItem> : ComponentBase
{
    [CascadingParameter] internal BzGrid<TItem>? Grid { get; set; }

    /// <summary>Header text.</summary>
    [Parameter] public string? Title { get; set; }
    /// <summary>Reads the cell value from a row (e.g. <c>x => x.Name</c>).</summary>
    [Parameter] public Func<TItem, object?>? Field { get; set; }
    /// <summary>Custom cell rendering.</summary>
    [Parameter] public RenderFragment<TItem>? Template { get; set; }
    /// <summary>Custom header rendering.</summary>
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>Allow sorting by this column.</summary>
    [Parameter] public bool Sortable { get; set; } = true;
    /// <summary>Show a per-column filter box.</summary>
    [Parameter] public bool Filterable { get; set; }
    /// <summary>Fixed column width (any CSS length).</summary>
    [Parameter] public string? Width { get; set; }
    /// <summary>Format string applied to <see cref="IFormattable"/> values (e.g. "N0", "d").</summary>
    [Parameter] public string? Format { get; set; }
    /// <summary>Text alignment: "start", "center" or "end".</summary>
    [Parameter] public string Align { get; set; } = "start";
    /// <summary>Pin the column while scrolling horizontally.</summary>
    [Parameter] public bool Sticky { get; set; }
    /// <summary>Allow inline editing of this column.</summary>
    [Parameter] public bool Editable { get; set; }
    /// <summary>Writes an edited value back to the row.</summary>
    [Parameter] public Action<TItem, string>? EditSetter { get; set; }
    /// <summary>Footer aggregate to compute (Sum, Avg, Count…).</summary>
    [Parameter] public BzAggregate Aggregate { get; set; } = BzAggregate.None;

    protected override void OnInitialized() => Grid?.AddColumn(this);

    /// <summary>The raw cell value for a row.</summary>
    public object? GetValue(TItem item) => Field?.Invoke(item);

    /// <summary>The cell value as a number, or null if it isn't numeric.</summary>
    public double? GetNumber(TItem item)
    {
        var value = Field?.Invoke(item);
        if (value is null) return null;
        if (value is IConvertible &&
            double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }

    public string GetText(TItem item)
    {
        var value = Field?.Invoke(item);
        if (value is null) return string.Empty;
        if (!string.IsNullOrEmpty(Format) && value is IFormattable f)
            return f.ToString(Format, CultureInfo.CurrentCulture);
        return value.ToString() ?? string.Empty;
    }
}
