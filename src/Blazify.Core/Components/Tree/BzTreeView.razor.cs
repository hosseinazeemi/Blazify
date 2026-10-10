using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;

namespace BzCore.Components.Tree;

// BzTreeView — a generic, hierarchical tree.
//
// The tree doesn't own your data shape: you tell it how to read children/text/icon
// from your TItem via the *Selector callbacks. State is four simple sets:
//   _selected       – chosen nodes
//   _expanded       – manually opened nodes
//   _visible        – nodes matching the current search (only used while searching)
//   _searchExpanded – parents auto-opened to reveal a search hit
// Child rows (BzTreeNode) subscribe to the Changed event to re-render.
public partial class BzTreeView<TItem> : BzComponentBase
{
    private readonly HashSet<TItem> _selected = new();
    private readonly HashSet<TItem> _expanded = new();
    private readonly HashSet<TItem> _visible = new();
    private readonly HashSet<TItem> _searchExpanded = new();
    private string _search = string.Empty;

    /// <summary>Root items.</summary>
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    /// <summary>Returns a node's children (null/empty for leaves).</summary>
    [Parameter] public Func<TItem, IEnumerable<TItem>?>? ChildrenSelector { get; set; }
    /// <summary>Returns a node's display text (defaults to ToString()).</summary>
    [Parameter] public Func<TItem, string>? TextSelector { get; set; }
    /// <summary>Returns a node's icon class (optional).</summary>
    [Parameter] public Func<TItem, string?>? IconSelector { get; set; }

    /// <summary>Allow selecting nodes.</summary>
    [Parameter] public bool Selectable { get; set; } = true;
    /// <summary>Allow multiple selection (shows checkboxes).</summary>
    [Parameter] public bool MultiSelect { get; set; }
    /// <summary>Show the search box.</summary>
    [Parameter] public bool Searchable { get; set; } = true;
    [Parameter] public string SearchPlaceholder { get; set; } = "جستجو…";

    /// <summary>Selected items (two-way bindable).</summary>
    [Parameter] public IReadOnlyCollection<TItem>? SelectedItems { get; set; }
    [Parameter] public EventCallback<IReadOnlyCollection<TItem>> SelectedItemsChanged { get; set; }
    /// <summary>Fires when any node is clicked (before selection changes).</summary>
    [Parameter] public EventCallback<TItem> OnNodeClick { get; set; }

    /// <summary>Optional per-node action buttons.</summary>
    [Parameter] public RenderFragment<TItem>? NodeActions { get; set; }

    /// <summary>Expand every node on first render.</summary>
    [Parameter] public bool ExpandedByDefault { get; set; }

    // Raised on any state change so BzTreeNode rows re-render.
    internal event Action? Changed;

    private bool Searching => !string.IsNullOrWhiteSpace(_search);

    protected override void OnParametersSet()
    {
        if (SelectedItems is not null)
        {
            _selected.Clear();
            foreach (var item in SelectedItems) _selected.Add(item);
        }
        if (ExpandedByDefault && _expanded.Count == 0 && Items is not null)
            foreach (var root in Items) ExpandRecursive(root);
    }

    private void ExpandRecursive(TItem node)
    {
        _expanded.Add(node);
        foreach (var child in Children(node)) ExpandRecursive(child);
    }

    // ----- read helpers (used by BzTreeNode) -----
    internal string TextOf(TItem node) => TextSelector?.Invoke(node) ?? node?.ToString() ?? "";
    internal string? IconOf(TItem node) => IconSelector?.Invoke(node);
    internal IEnumerable<TItem> Children(TItem node) => ChildrenSelector?.Invoke(node) ?? Enumerable.Empty<TItem>();
    internal bool HasChildren(TItem node) => Children(node).Any();

    internal bool IsSelected(TItem node) => _selected.Contains(node);
    internal bool IsExpanded(TItem node) => _expanded.Contains(node) || (Searching && _searchExpanded.Contains(node));
    internal bool IsVisible(TItem node) => !Searching || _visible.Contains(node);

    internal RenderFragment<TItem>? Actions => NodeActions;
    internal bool ShowCheckbox => Selectable && MultiSelect;

    // ----- interactions -----
    internal void ToggleExpand(TItem node)
    {
        if (!_expanded.Add(node)) _expanded.Remove(node);   // Add returns false if already present → collapse
        Changed?.Invoke();
    }

    internal async Task RowClickAsync(TItem node)
    {
        if (OnNodeClick.HasDelegate) await OnNodeClick.InvokeAsync(node);
        if (!Selectable) return;

        if (MultiSelect)
        {
            if (!_selected.Add(node)) _selected.Remove(node);   // toggle
        }
        else
        {
            _selected.Clear();
            _selected.Add(node);
        }
        await NotifySelection();
    }

    internal async Task ToggleCheckAsync(TItem node)
    {
        if (!_selected.Add(node)) _selected.Remove(node);
        await NotifySelection();
    }

    // Wrap the search term in <mark> for display (HTML-encoded to stay safe).
    internal MarkupString Highlight(string text)
    {
        if (!Searching) return (MarkupString)System.Net.WebUtility.HtmlEncode(text);
        var idx = text.IndexOf(_search, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return (MarkupString)System.Net.WebUtility.HtmlEncode(text);

        var before = System.Net.WebUtility.HtmlEncode(text[..idx]);
        var match  = System.Net.WebUtility.HtmlEncode(text.Substring(idx, _search.Length));
        var after  = System.Net.WebUtility.HtmlEncode(text[(idx + _search.Length)..]);
        return (MarkupString)$"{before}<mark class=\"bz-tree__mark\">{match}</mark>{after}";
    }

    private async Task NotifySelection()
    {
        if (SelectedItemsChanged.HasDelegate) await SelectedItemsChanged.InvokeAsync(_selected.ToList());
        Changed?.Invoke();
        StateHasChanged();
    }

    // ----- search -----
    private void OnSearch(string? value)
    {
        _search = value ?? "";
        _visible.Clear();
        _searchExpanded.Clear();
        if (Searching && Items is not null)
            foreach (var root in Items) MarkMatches(root);
        Changed?.Invoke();
        StateHasChanged();
    }

    // Depth-first: a node is visible if it or any descendant matches; parents of a
    // match are auto-expanded. Returns whether this subtree contains a match.
    private bool MarkMatches(TItem node)
    {
        var selfMatch = TextOf(node).Contains(_search, StringComparison.OrdinalIgnoreCase);
        var childMatch = false;
        foreach (var child in Children(node)) childMatch |= MarkMatches(child);

        var visible = selfMatch || childMatch;
        if (visible) _visible.Add(node);
        if (childMatch) _searchExpanded.Add(node);
        return visible;
    }

    // ----- public API -----
    /// <summary>The currently selected items.</summary>
    public IReadOnlyCollection<TItem> GetSelected() => _selected.ToList();
    /// <summary>Clear the selection.</summary>
    public void ClearSelection() { _selected.Clear(); _ = NotifySelection(); }
    /// <summary>Expand every node.</summary>
    public void ExpandAll()
    {
        if (Items is not null) foreach (var root in Items) ExpandRecursive(root);
        Changed?.Invoke();
        StateHasChanged();
    }
    /// <summary>Collapse every node.</summary>
    public void CollapseAll() { _expanded.Clear(); Changed?.Invoke(); StateHasChanged(); }
}
