using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BzCore.Enums;
using BzCore.Helpers;
using Blazify.Core.Base;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace BzCore.Components.AutoComplete;

public partial class BzAutoComplete<TItem> : BzFieldBase<TItem>
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private ElementReference _input;
    private string _search = string.Empty;
    private bool _open;
    private bool _loading;
    private int _activeIndex = -1;
    private List<TItem> _results = new();
    private CancellationTokenSource? _debounceCts;

    /// <summary> Single Auto Complete</summary>
    [Parameter] public IEnumerable<TItem>? Items { get; set; }

    /// <summary> Link Methods </summary>
    [Parameter] public Func<string, Task<IEnumerable<TItem>>>? SearchFunc { get; set; }

    [Parameter] public Func<TItem, string>? ToStringFunc { get; set; }

    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

    [Parameter] public int MinSearchLength { get; set; } = 1;
    [Parameter] public int DebounceMs { get; set; } = 250;
    [Parameter] public int MaxItems { get; set; } = 50;
    [Parameter] public bool OpenOnFocus { get; set; } = true;
    [Parameter] public bool HighlightMatch { get; set; } = true;
    [Parameter] public string NoResultsText { get; set; } = "Not Found Any Item";
    [Parameter] public BzCharFilter CharFilter { get; set; } = BzCharFilter.Any;

    [Parameter] public EventCallback<TItem> OnItemSelected { get; set; }

    private string WrapperClass => RootCss("bz-ac")
        .Add($"bz-ac--{Size.ToString().ToLowerInvariant()}")
        .AddIf(IsDisabled, "is-disabled")
        .AddIf(IsReadOnly, "is-readonly")
        .AddIf(_open, "is-open")
        .AddIf(HasError, "is-invalid")
        .Build();

    private string ToText(TItem? item)
    {
        if (item is null)
            return string.Empty;

        if (ToStringFunc is not null)
            return ToStringFunc(item) ?? string.Empty;

        return item.ToString() ?? string.Empty;
    }

    protected override void OnFieldParametersSet()
    {
        if (!_open) 
            _search = ToText(Value);
    }

    // cancelation token : when user press keyboard key,old request will canceled and new request will start{LoOoOoOP For All, Set Delay Time With DebounceMs}
    private async Task OnInputAsync(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        _search = FilterText(raw);
        _activeIndex = -1;

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            if (DebounceMs > 0)
                await Task.Delay(DebounceMs, token);
            if (token.IsCancellationRequested) 
                return;
            await RunSearchAsync();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task RunSearchAsync()
    {
        if (_search.Length < MinSearchLength)
        {
            _results = new();
            _open = false;
            StateHasChanged();
            return;
        }

        if (SearchFunc is not null)
        {
            _loading = true;
            _open = true;
            StateHasChanged();
            try
            {
                var found = await SearchFunc(_search);
                _results = found.Take(MaxItems).ToList();
            }
            finally
            {
                _loading = false;
            }
        }
        else if (Items is not null)
        {
            var q = _search;
            _results = Items
                .Where(i => ToText(i).Contains(q, StringComparison.OrdinalIgnoreCase))
                .Take(MaxItems)
                .ToList();
        }
        else
        {
            _results = new();
        }

        _open = true;
        StateHasChanged();
    }

    private string FilterText(string raw)
    {
        raw = PersianHelper.ToLatinDigits(PersianHelper.FixArabicChars(raw));
        return CharFilter switch
        {
            BzCharFilter.PersianOnly => new string(raw.Where(c => PersianHelper.IsPersianLetter(c) || char.IsWhiteSpace(c)).ToArray()),
            BzCharFilter.EnglishOnly => new string(raw.Where(c => PersianHelper.IsEnglishLetter(c) || char.IsWhiteSpace(c)).ToArray()),
            BzCharFilter.NumberOnly => new string(raw.Where(char.IsDigit).ToArray()),
            _ => raw
        };
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                if (!_open) 
                { 
                    await RunSearchAsync();
                }
                if (_results.Count > 0) 
                    _activeIndex = (_activeIndex + 1) % _results.Count;
                break;
            case "ArrowUp":
                if (_results.Count > 0) 
                    _activeIndex = (_activeIndex - 1 + _results.Count) % _results.Count;
                break;
            case "Enter":
                if (_open && _activeIndex >= 0 && _activeIndex < _results.Count)
                    await SelectAsync(_results[_activeIndex]);
                break;
            case "Escape":
                Close();
                break;
        }
    }

    private async Task OnFocusAsync()
    {
        if (OpenOnFocus && !IsReadOnly && !IsDisabled && _search.Length >= MinSearchLength)
            await RunSearchAsync();
    }

    private void OnFocusOut() => Close();

    private async Task SelectAsync(TItem item)
    {
        _search = ToText(item);
        Close();
        await SetValueAsync(item);
        if (OnItemSelected.HasDelegate) 
            await OnItemSelected.InvokeAsync(item);
    }

    private async Task ClearAsync()
    {
        _search = string.Empty;
        _results = new();
        Close();
        await SetValueAsync(default);
        await _input.FocusAsync();
    }

    private void Close() { 
        _open = false;
        _activeIndex = -1;
        StateHasChanged(); 
    }

    private MarkupString Highlight(string text)
    {
        if (!HighlightMatch || string.IsNullOrEmpty(_search))
            return (MarkupString)System.Net.WebUtility.HtmlEncode(text);

        var idx = text.IndexOf(_search, StringComparison.OrdinalIgnoreCase);

        if (idx < 0)
            return (MarkupString)System.Net.WebUtility.HtmlEncode(text);

        var before = System.Net.WebUtility.HtmlEncode(text[..idx]);
        var match = System.Net.WebUtility.HtmlEncode(text.Substring(idx, _search.Length));
        var after = System.Net.WebUtility.HtmlEncode(text[(idx + _search.Length)..]);

        return (MarkupString)$"{before}<mark class=\"bz-ac__mark\">{match}</mark>{after}";
    }


    public ValueTask FocusAsync() => _input.FocusAsync();
    public Task ClearAsyncPublic() => ClearAsync();
    public void OpenDropdown() 
    { 
        _ = RunSearchAsync(); 
    }
    public void CloseDropdown() => Close();

    public override void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        base.Dispose();
    }
}
