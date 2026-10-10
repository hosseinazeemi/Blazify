using BzCore.Enums;

namespace BzCore.Components.Grid;

/// <summary>Controls how many selected rows are required before a header action can run.</summary>
public enum BzGridActionSelectionRequirement
{
    /// <summary>The action does not require a selection.</summary>
    None,

    /// <summary>The action requires exactly one selected row.</summary>
    ExactlyOne,

    /// <summary>The action requires one or more selected rows.</summary>
    OneOrMore
}

/// <summary>Describes a button rendered in the grid header toolbar.</summary>
public sealed class BzGridHeaderAction<TItem>
{
    /// <summary>Text displayed inside the button.</summary>
    public string? Title { get; set; }

    /// <summary>Optional browser tooltip and accessible label for icon-only buttons.</summary>
    public string? Tooltip { get; set; }

    /// <summary>Font icon class rendered before the title.</summary>
    public string? StartIcon { get; set; }

    /// <summary>Font icon class rendered after the title.</summary>
    public string? EndIcon { get; set; }

    public BzColor Color { get; set; } = BzColor.Primary;
    public BzButtonVariant Variant { get; set; } = BzButtonVariant.Filled;
    public BzSize Size { get; set; } = BzSize.Small;

    public bool Visible { get; set; } = true;
    public bool Disabled { get; set; }
    public bool ReadOnly { get; set; }
    public bool Loading { get; set; }
    public bool IconOnly { get; set; }
    public bool FullWidth { get; set; }
    public bool Rounded { get; set; }
    public bool Ripple { get; set; } = true;
    public bool AutoLoadingOnClick { get; set; } = true;
    public BzLoadingEffect LoadingEffect { get; set; } = BzLoadingEffect.Spinner;
    public int TabIndex { get; set; }
    public string ButtonType { get; set; } = "button";

    /// <summary>
    /// Selection required to enable and invoke this action. The default allows the action
    /// to run without selecting a row.
    /// </summary>
    public BzGridActionSelectionRequirement SelectionRequirement { get; set; }

    /// <summary>Extra CSS class applied to the button.</summary>
    public string? Class { get; set; }

    /// <summary>Inline CSS applied to the button.</summary>
    public string? Style { get; set; }

    /// <summary>Additional HTML attributes applied to the button.</summary>
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Invoked on click with snapshots of the current selection, filtered rows and page rows.
    /// </summary>
    public Func<BzGridHeaderActionContext<TItem>, Task>? OnClick { get; set; }
}

/// <summary>Grid state supplied to a header action click handler.</summary>
public sealed record BzGridHeaderActionContext<TItem>(
    BzGridHeaderAction<TItem> Action,
    IReadOnlyCollection<TItem> SelectedItems,
    IReadOnlyList<TItem> FilteredItems,
    IReadOnlyList<TItem> PageItems);
