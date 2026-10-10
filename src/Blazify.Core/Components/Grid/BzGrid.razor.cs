using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Blazify.Core.Base;
using BzCore.Helpers;
using BzCore.Enums;

namespace BzCore.Components.Grid;

// BzGrid<TItem> — a data grid: sorting, per-column filters, global search, paging,
// selection, expandable detail rows, grouping, footer aggregates and CSV import/export.
//
// The data pipeline is the heart of it and reads top-to-bottom in the `Filtered` getter:
//     Items  ->  global search  ->  per-column filters  ->  sort   =>  Filtered
//     Filtered  ->  skip/take the current page                     =>  PageItems
// Columns register themselves (AddColumn) from the <Columns> markup. Everything else
// (sort/filter/search/paging/selection) just mutates a little state and re-renders.
public partial class BzGrid<TItem> : BzComponentBase
{
    private readonly List<BzColumn<TItem>> _columns = new();
    private bool _columnsDirty;
    private IReadOnlyList<TItem> _providerItems = Array.Empty<TItem>();
    private int _providerTotal;
    private CancellationTokenSource? _loadCts;

    private BzColumn<TItem>? _sortColumn;
    private bool _sortAsc = true;
    private int _page;                                   // 0-based
    private string _search = string.Empty;
    private readonly Dictionary<BzColumn<TItem>, string> _filters = new();
    private readonly HashSet<TItem> _expanded = new();
    private readonly HashSet<TItem> _selected = new();

    /// <summary>The rows to display.</summary>
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    /// <summary>Total server rows when Items contains only the current page.</summary>
    [Parameter] public int? TotalItemCount { get; set; }
    /// <summary>Fetches the requested page on demand.</summary>
    [Parameter] public Func<BzGridDataRequest, Task<BzGridDataResult<TItem>>>? ItemsProvider { get; set; }

    /// <summary>The <see cref="BzColumn{TItem}"/> definitions.</summary>
    [Parameter] public RenderFragment? Columns { get; set; }

    /// <summary>Expandable detail row content.</summary>
    [Parameter] public RenderFragment<TItem>? DetailTemplate { get; set; }

    /// <summary>Show selection checkboxes.</summary>
    [Parameter] public bool Selectable { get; set; }
    /// <summary>Allow selecting several rows.</summary>
    [Parameter] public bool MultiSelect { get; set; } = true;
    /// <summary>Selected rows (two-way bindable).</summary>
    [Parameter] public IReadOnlyCollection<TItem>? SelectedItems { get; set; }
    [Parameter] public EventCallback<IReadOnlyCollection<TItem>> SelectedItemsChanged { get; set; }
    /// <summary>Fires when a row is clicked.</summary>
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }

    /// <summary>Fires when a row is double-clicked.</summary>
    [Parameter] public EventCallback<TItem> OnRowDoubleClick { get; set; }
    [Parameter] public EventCallback<BzGridSortEvent> OnSortChanged { get; set; }
    [Parameter] public EventCallback<BzGridPageEvent> OnPageChanged { get; set; }
    [Parameter] public EventCallback<int> OnPageSizeChanged { get; set; }
    [Parameter] public EventCallback<string?> OnSearchChanged { get; set; }
    [Parameter] public EventCallback<BzGridFilterEvent> OnFilterChanged { get; set; }
    [Parameter] public EventCallback<TItem> OnRowExpanded { get; set; }
    [Parameter] public EventCallback<TItem> OnRowCollapsed { get; set; }
    [Parameter] public EventCallback<int> OnExported { get; set; }

    /// <summary>Enable paging.</summary>
    [Parameter] public bool Pageable { get; set; } = true;
    /// <summary>Rows per page.</summary>
    [Parameter] public int PageSize { get; set; } = 10;
    /// <summary>Choices in the page-size dropdown.</summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = { 5, 10, 25, 50, 100 };

    /// <summary>Show the global search box.</summary>
    [Parameter] public bool ShowSearch { get; set; } = true;
    /// <summary>Show the toolbar.</summary>
    [Parameter] public bool ShowToolbar { get; set; } = true;
    /// <summary>Alternate row shading.</summary>
    [Parameter] public bool Striped { get; set; } = true;
    /// <summary>Highlight the row under the cursor.</summary>
    [Parameter] public bool Hoverable { get; set; } = true;
    /// <summary>Draw cell borders.</summary>
    [Parameter] public bool Bordered { get; set; }
    /// <summary>Show the loading state.</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Message shown when there are no rows.</summary>
    [Parameter] public string EmptyText { get; set; } = "داده ای برای نمایش وجود نداره";
    /// <summary>Return an extra CSS class per row.</summary>
    [Parameter] public Func<TItem, string?>? RowClass { get; set; }
    /// <summary>
    /// Returns true when a row must be completely non-interactive. Disabled rows cannot be
    /// selected, clicked, expanded or edited, and controls rendered inside their templates
    /// cannot receive pointer or keyboard interaction.
    /// </summary>
    [Parameter] public Func<TItem, bool>? RowDisabled { get; set; }
    /// <summary>Extra toolbar content.</summary>
    [Parameter] public RenderFragment? ToolbarContent { get; set; }

    /// <summary>Buttons rendered in the header toolbar.</summary>
    [Parameter] public IReadOnlyList<BzGridHeaderAction<TItem>>? HeaderActions { get; set; }

    /// <summary>Allow dragging column borders to resize.</summary>
    [Parameter] public bool Resizable { get; set; }
    /// <summary>Show the aggregate footer row.</summary>
    [Parameter] public bool ShowFooter { get; set; }
    /// <summary>Group rows by this key.</summary>
    [Parameter] public Func<TItem, object?>? GroupBy { get; set; }
    /// <summary>Virtualize rows instead of paging (for large data).</summary>
    [Parameter] public bool Virtualize { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private TItem? _editRow;
    private BzColumn<TItem>? _editCol;
    private string _editValue = "";

    private bool IsEditing(TItem row, BzColumn<TItem> col)
        => _editCol == col && EqualityComparer<TItem>.Default.Equals(_editRow, row);

    private void BeginEdit(TItem row, BzColumn<TItem> col)
    {
        if (IsRowDisabled(row) || !col.Editable || col.EditSetter is null) return;
        _editRow = row; _editCol = col; _editValue = col.GetText(row);
    }

    private void CommitEdit()
    {
        if (_editRow is not null && _editCol?.EditSetter is not null)
            _editCol.EditSetter(_editRow, _editValue);
        _editRow = default; _editCol = null;
    }

    private void CancelEdit() { _editRow = default; _editCol = null; }

    private string AggregateText(BzColumn<TItem> col)
    {
        if (col.Aggregate == BzAggregate.None) return "";
        var nums = Filtered.Select(col.GetNumber).Where(n => n.HasValue).Select(n => n!.Value).ToList();
        double v = col.Aggregate switch
        {
            BzAggregate.Sum => nums.Sum(),
            BzAggregate.Avg => nums.Count > 0 ? nums.Average() : 0,
            BzAggregate.Count => Filtered.Count,
            BzAggregate.Min => nums.Count > 0 ? nums.Min() : 0,
            BzAggregate.Max => nums.Count > 0 ? nums.Max() : 0,
            _ => 0
        };
        var label = col.Aggregate switch
        {
            BzAggregate.Sum => "جمع", BzAggregate.Avg => "میانگین", BzAggregate.Count => "تعداد",
            BzAggregate.Min => "کمینه", BzAggregate.Max => "بیشینه", _ => ""
        };
        return $"{label}: {v.ToString("N0", System.Globalization.CultureInfo.CurrentCulture)}";
    }

    private bool HasFooter => ShowFooter && _columns.Any(c => c.Aggregate != BzAggregate.None);

    private IEnumerable<IGrouping<object?, TItem>> GroupedPage()
        => GroupBy is null ? Enumerable.Empty<IGrouping<object?, TItem>>() : PageItems.GroupBy(GroupBy);

    private async Task InitResizeAsync()
    {
        if (Resizable) { try { await JS.InvokeVoidAsync("bzGridResize.init", Id ?? ""); } catch { } }
    }


    /// <summary>Show the CSV export button.</summary>
    [Parameter] public bool ShowExport { get; set; }
    /// <summary>Show the CSV import button.</summary>
    [Parameter] public bool ShowImport { get; set; }
    /// <summary>File name for CSV export.</summary>
    [Parameter] public string ExportFileName { get; set; } = "export.csv";

    /// <summary>Fires with the parsed rows after a CSV import.</summary>
    [Parameter] public EventCallback<List<Dictionary<string, string>>> OnImport { get; set; }

    private bool HasHeaderActions => HeaderActions?.Any(action => action.Visible) == true;

    private bool ShowTools => ShowToolbar &&
        (ShowSearch || ShowExport || ShowImport || ToolbarContent is not null || HasHeaderActions);

    private static IReadOnlyDictionary<string, object>? ActionAttributes(BzGridHeaderAction<TItem> action)
    {
        var accessibleTitle = action.Tooltip ?? (action.IconOnly ? action.Title : null);
        if (accessibleTitle is null)
            return action.AdditionalAttributes;

        var attributes = action.AdditionalAttributes is null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(action.AdditionalAttributes);
        attributes["title"] = accessibleTitle;
        attributes["aria-label"] = accessibleTitle;
        return attributes;
    }

    private async Task InvokeHeaderActionAsync(BzGridHeaderAction<TItem> action)
    {
        if (!action.Visible || IsHeaderActionDisabled(action) || action.OnClick is null)
            return;

        var context = new BzGridHeaderActionContext<TItem>(
            action,
            _selected.ToList(),
            Filtered,
            PageItems);

        await action.OnClick(context);
    }

    private bool IsHeaderActionDisabled(BzGridHeaderAction<TItem> action)
        => action.Disabled || action.SelectionRequirement switch
        {
            BzGridActionSelectionRequirement.ExactlyOne => _selected.Count != 1,
            BzGridActionSelectionRequirement.OneOrMore => _selected.Count == 0,
            _ => false
        };

    private async Task ExportCsvAsync()
    {
        var cols = _columns.Where(c => c.Field is not null).ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.Title ?? ""))));
        foreach (var item in Filtered)
            sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.GetText(item)))));

        await JS.InvokeVoidAsync("bzDownload", ExportFileName, sb.ToString(), "text/csv");
        if (OnExported.HasDelegate) await OnExported.InvokeAsync(Filtered.Count);
    }

    private static string Csv(string v)
    {
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }

    private async Task OnImportFile(InputFileChangeEventArgs e)
    {
        using var reader = new StreamReader(e.File.OpenReadStream(20 * 1024 * 1024), Encoding.UTF8);
        var text = await reader.ReadToEndAsync();
        var rows = ParseCsv(text);
        if (OnImport.HasDelegate) await OnImport.InvokeAsync(rows);
    }

    // A small CSV parser (handles quoted fields and "" escapes). Returns one dictionary
    // per data row, keyed by the header row.
    private static List<Dictionary<string, string>> ParseCsv(string text)
    {
        var records = new List<List<string>>();
        var field = new StringBuilder();
        var row = new List<string>();
        bool inQuotes = false;
        for (int i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(ch);
            }
            else
            {
                switch (ch)
                {
                    case '"': inQuotes = true; break;
                    case ',': row.Add(field.ToString()); field.Clear(); break;
                    case '\r': break;
                    case '\n': row.Add(field.ToString()); field.Clear(); records.Add(row); row = new(); break;
                    default: field.Append(ch); break;
                }
            }
        }
        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); records.Add(row); }

        var result = new List<Dictionary<string, string>>();
        if (records.Count == 0) return result;
        var headers = records[0];
        for (var r = 1; r < records.Count; r++)
        {
            var dict = new Dictionary<string, string>();
            for (var c = 0; c < headers.Count && c < records[r].Count; c++)
                dict[headers[c].Trim()] = records[r][c];
            if (dict.Count > 0) result.Add(dict);
        }
        return result;
    }

    internal void AddColumn(BzColumn<TItem> col)
    {
        if (!_columns.Contains(col))
        {
            _columns.Add(col);
            _columnsDirty = true;
        }
    }

    private readonly string _uid = "grid" + Guid.NewGuid().ToString("N")[..8];
    internal string GridId => string.IsNullOrEmpty(Id) ? _uid : Id;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_columnsDirty)
        {
            _columnsDirty = false;
            StateHasChanged();
        }
        if (firstRender)
        {
            await InitResizeAsync();
            if (ItemsProvider is not null) await LoadPageAsync();
        }
    }

    protected override void OnParametersSet()
    {
        _expanded.RemoveWhere(IsRowDisabled);
        if (_editRow is not null && IsRowDisabled(_editRow)) CancelEdit();

        if (SelectedItems is not null)
        {
            _selected.Clear();
            foreach (var i in SelectedItems.Where(i => !IsRowDisabled(i))) _selected.Add(i);
        }
        else
        {
            _selected.RemoveWhere(IsRowDisabled);
        }
    }

    // The full result set after search + column filters + sort (before paging).
    private IReadOnlyList<TItem> Filtered
    {
        get
        {
            IEnumerable<TItem> q = ItemsProvider is not null ? _providerItems : Items ?? Enumerable.Empty<TItem>();

            if (IsServerPaging) return q.ToList();

            if (!string.IsNullOrWhiteSpace(_search))
            {
                var s = _search.Trim();
                q = q.Where(i => _columns.Any(c => c.GetText(i).Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            foreach (var (col, val) in _filters)
            {
                if (string.IsNullOrWhiteSpace(val)) continue;
                var f = val.Trim();
                q = q.Where(i => col.GetText(i).Contains(f, StringComparison.OrdinalIgnoreCase));
            }

            if (_sortColumn is not null)
            {
                q = _sortAsc
                    ? q.OrderBy(i => _sortColumn!.GetValue(i), Comparer<object?>.Default)
                    : q.OrderByDescending(i => _sortColumn!.GetValue(i), Comparer<object?>.Default);
            }

            return q.ToList();
        }
    }

    private bool IsServerPaging => ItemsProvider is not null || TotalItemCount is not null;
    private int TotalCount => ItemsProvider is not null ? _providerTotal : TotalItemCount ?? Filtered.Count;
    private int PageCount => Pageable ? Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize)) : 1;

    private IReadOnlyList<TItem> PageItems
    {
        get
        {
            var data = Filtered;
            if (!Pageable || Virtualize || IsServerPaging) return data;
            if (_page >= PageCount) _page = PageCount - 1;
            if (_page < 0) _page = 0;
            return data.Skip(_page * PageSize).Take(PageSize).ToList();
        }
    }

    private List<TItem> PageItemsList => PageItems as List<TItem> ?? PageItems.ToList();

    private int TotalColumns => _columns.Count + (Selectable ? 1 : 0) + (DetailTemplate is not null ? 1 : 0);

    private async Task ToggleSort(BzColumn<TItem> col)
    {
        if (!col.Sortable) return;
        if (_sortColumn == col) _sortAsc = !_sortAsc;
        else { _sortColumn = col; _sortAsc = true; }
        if (OnSortChanged.HasDelegate)
            await OnSortChanged.InvokeAsync(new BzGridSortEvent(col.Title, _sortAsc));
        if (ItemsProvider is not null) await LoadPageAsync();
        StateHasChanged();
    }

    private string SortIcon(BzColumn<TItem> col)
        => _sortColumn != col ? "fa-solid fa-sort" : _sortAsc ? "fa-solid fa-sort-up" : "fa-solid fa-sort-down";

    private bool AnyFilterable => _columns.Any(c => c.Filterable);

    private async Task SetFilter(BzColumn<TItem> col, string? value)
    {
        _filters[col] = value ?? string.Empty;
        _page = 0;
        if (OnFilterChanged.HasDelegate)
            await OnFilterChanged.InvokeAsync(new BzGridFilterEvent(col.Title, value));
        if (ItemsProvider is not null) await LoadPageAsync();
        StateHasChanged();
    }

    private async Task OnSearch(string? value)
    {
        _search = value ?? string.Empty; _page = 0;
        if (OnSearchChanged.HasDelegate) await OnSearchChanged.InvokeAsync(_search);
        if (ItemsProvider is not null) await LoadPageAsync();
        StateHasChanged();
    }

    private async Task GoTo(int page)
    {
        _page = Math.Clamp(page, 0, PageCount - 1);
        if (OnPageChanged.HasDelegate)
            await OnPageChanged.InvokeAsync(new BzGridPageEvent(_page + 1, PageSize, TotalCount));
        if (ItemsProvider is not null) await LoadPageAsync();
        StateHasChanged();
    }
    private async Task ChangePageSize(int size)
    {
        PageSize = size; _page = 0;
        if (OnPageSizeChanged.HasDelegate) await OnPageSizeChanged.InvokeAsync(size);
        if (OnPageChanged.HasDelegate)
            await OnPageChanged.InvokeAsync(new BzGridPageEvent(1, PageSize, TotalCount));
        if (ItemsProvider is not null) await LoadPageAsync();
        StateHasChanged();
    }
    private int FirstRowIndex => TotalCount == 0 ? 0 : _page * PageSize + 1;
    private int LastRowIndex => Math.Min((_page + 1) * PageSize, TotalCount);

    /// <summary>Reloads the current server page.</summary>
    public async Task ReloadAsync() => await LoadPageAsync();

    private async Task LoadPageAsync()
    {
        if (ItemsProvider is null) return;
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        var cts = _loadCts = new CancellationTokenSource();
        Loading = true;
        StateHasChanged();
        try
        {
            var filters = _filters.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Key.Title ?? string.Empty, x => x.Value);
            var request = new BzGridDataRequest(_page + 1, PageSize, _search,
                _sortColumn?.Title, _sortAsc, filters, cts.Token);
            var result = await ItemsProvider(request);
            if (cts.IsCancellationRequested) return;
            _providerItems = result.Items ?? Array.Empty<TItem>();
            _providerTotal = Math.Max(0, result.TotalCount);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested) { }
        finally
        {
            if (ReferenceEquals(_loadCts, cts)) Loading = false;
            StateHasChanged();
        }
    }

    private bool IsExpanded(TItem item) => _expanded.Contains(item);
    private async Task ToggleExpand(TItem item)
    {
        if (IsRowDisabled(item)) return;

        if (_expanded.Add(item))
        {
            if (OnRowExpanded.HasDelegate) await OnRowExpanded.InvokeAsync(item);
        }
        else
        {
            _expanded.Remove(item);
            if (OnRowCollapsed.HasDelegate) await OnRowCollapsed.InvokeAsync(item);
        }
        StateHasChanged();
    }

    private bool IsSelected(TItem item) => _selected.Contains(item);

    private bool IsRowDisabled(TItem item) => RowDisabled?.Invoke(item) == true;

    private async Task ToggleSelect(TItem item)
    {
        if (IsRowDisabled(item)) return;

        if (!MultiSelect) _selected.Clear();
        if (!_selected.Add(item)) _selected.Remove(item);
        await NotifySelection();
    }

    private IReadOnlyList<TItem> SelectablePageItems => PageItems.Where(i => !IsRowDisabled(i)).ToList();

    private bool AllPageSelected => SelectablePageItems.Count > 0 && SelectablePageItems.All(_selected.Contains);

    private async Task ToggleSelectAll()
    {
        var selectableItems = SelectablePageItems;
        if (AllPageSelected)
            foreach (var i in selectableItems) _selected.Remove(i);
        else
            foreach (var i in selectableItems) _selected.Add(i);
        await NotifySelection();
    }

    private async Task NotifySelection()
    {
        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(_selected.ToList());
        StateHasChanged();
    }

    private async Task RowClicked(TItem item)
    {
        if (IsRowDisabled(item)) return;
        if (OnRowClick.HasDelegate) await OnRowClick.InvokeAsync(item);
    }

    private async Task RowDoubleClicked(TItem item)
    {
        if (IsRowDisabled(item)) return;
        if (OnRowDoubleClick.HasDelegate) await OnRowDoubleClick.InvokeAsync(item);
    }

    private string TableClass => CssBuilder.Default("bz-grid")
        .AddIf(Striped, "bz-grid--striped")
        .AddIf(Hoverable, "bz-grid--hover")
        .AddIf(Bordered, "bz-grid--bordered")
        .AddIf(IsDense, "bz-grid--dense")
        .Build();

    public void Refresh() => StateHasChanged();
    public IReadOnlyCollection<TItem> GetSelected() => _selected.ToList();
    public void ClearSelection() { _selected.Clear(); _ = NotifySelection(); }
    public void ExpandAll() { foreach (var i in Items ?? Enumerable.Empty<TItem>()) _expanded.Add(i); StateHasChanged(); }
    public void CollapseAll() { _expanded.Clear(); StateHasChanged(); }

    public override void Dispose()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        base.Dispose();
    }
}
