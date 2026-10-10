using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;

namespace BzCore.Components.Tabs;

// BzTabs — a tab strip plus its panels. <BzTab> children register themselves via a
// cascading value; this component renders the buttons and the active panel.
public partial class BzTabs : BzComponentBase
{
    private readonly List<BzTab> _tabs = new();
    private bool _tabsChanged;   // set when a tab registers/unregisters, so we re-render once

    /// <summary>The BzTab children.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }
    /// <summary>Horizontal or Vertical tab strip.</summary>
    [Parameter] public BzOrientation Orientation { get; set; } = BzOrientation.Horizontal;
    /// <summary>Visual style (Line, Pills…).</summary>
    [Parameter] public BzTabsVariant Variant { get; set; } = BzTabsVariant.Line;

    /// <summary>Index of the selected tab (two-way bindable).</summary>
    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    /// <summary>Fires after the active tab changes.</summary>
    [Parameter] public EventCallback<int> OnTabChanged { get; set; }

    /// <summary>Keep inactive panels in the DOM (hidden) instead of tearing them down.</summary>
    [Parameter] public bool KeepAlive { get; set; }

    internal IReadOnlyList<BzTab> Tabs => _tabs;

    // ----- called by child tabs -----

    internal void AddTab(BzTab tab)
    {
        if (!_tabs.Contains(tab)) { _tabs.Add(tab); _tabsChanged = true; }
    }

    internal void RemoveTab(BzTab tab)
    {
        if (_tabs.Remove(tab)) { _tabsChanged = true; InvokeAsync(StateHasChanged); }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        // Tabs register during the first render pass; render once more so the strip shows them.
        if (_tabsChanged) { _tabsChanged = false; StateHasChanged(); }
    }

    private string RootClass => RootCss("bz-tabs")
        .Add($"bz-tabs--{Orientation.ToString().ToLowerInvariant()}")
        .Add($"bz-tabs--{Variant.ToString().ToLowerInvariant()}")
        .Build();

    private async Task SelectAsync(int index)
    {
        // Ignore out-of-range, disabled, or already-active selections.
        if (index < 0 || index >= _tabs.Count || _tabs[index].Disabled || index == ActiveIndex) return;

        ActiveIndex = index;
        if (ActiveIndexChanged.HasDelegate) await ActiveIndexChanged.InvokeAsync(index);
        if (OnTabChanged.HasDelegate) await OnTabChanged.InvokeAsync(index);
        StateHasChanged();
    }

    // ----- public API -----

    /// <summary>Select a tab by index.</summary>
    public Task Select(int index) => SelectAsync(index);

    /// <summary>Move to the next enabled tab.</summary>
    public Task Next() => SelectAsync(NextEnabledIndex(ActiveIndex, +1));

    /// <summary>Move to the previous enabled tab.</summary>
    public Task Previous() => SelectAsync(NextEnabledIndex(ActiveIndex, -1));

    // Walk from 'from' in the given direction (+1/-1) to the first non-disabled tab.
    // Returns 'from' unchanged if none is found.
    private int NextEnabledIndex(int from, int step)
    {
        for (var i = from + step; i >= 0 && i < _tabs.Count; i += step)
            if (!_tabs[i].Disabled) return i;
        return from;
    }
}
