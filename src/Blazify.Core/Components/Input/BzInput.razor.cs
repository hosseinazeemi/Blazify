using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Helpers;

namespace BzCore.Components.Input;
public partial class BzInput<TValue> : BzFieldBase<TValue>
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private string _display = string.Empty;
    private string _domValue = string.Empty;
    private bool _focused;
    private ElementReference _input;

    /// <summary>Normal,Money,Mask</summary>
    [Parameter] public BzInputMode Mode { get; set; } = BzInputMode.Normal;
    /// <summary>Digits, Decimal, Persian/English letters…</summary>
    [Parameter] public BzCharFilter CharFilter { get; set; } = BzCharFilter.Any;
    /// <summary>OnChange,OnInput</summary>
    [Parameter] public BzUpdateTrigger UpdateTrigger { get; set; } = BzUpdateTrigger.OnChange;
    /// <summary>In dige kamelan moshakhase :|</summary>
    [Parameter] public BzCurrency Currency { get; set; } = BzCurrency.Toman;
    /// <summary>Input mask (Mask mode). Tokens: '#' = digit, 'A' = letter, '*' = any; other chars are literals.</summary>
    [Parameter] public string? Mask { get; set; }
    /// <summary>Display value using Persian digits</summary>
    [Parameter] public bool ShowPersianDigits { get; set; }
    /// <summary>Maximum number of characters</summary>
    [Parameter] public int? MaxLength { get; set; }

    /// <summary>Show persian words of amount</summary>
    [Parameter] public bool ShowAmountInWords { get; set; }
    /// <summary>Append currency name end of words(تومان، دلار، پوند و ..)</summary>
    [Parameter] public string? AmountInWordsSuffix { get; set; }

    private string? AmountInWords
    {
        get
        {
            if (!ShowAmountInWords || Mode != BzInputMode.Money) return null;
            var words = NumberToWordsHelper.FromDigits(_display);
            if (string.IsNullOrEmpty(words)) return null;
            var suffix = AmountInWordsSuffix ?? Currency switch
            {
                BzCurrency.Toman => "تومان",
                BzCurrency.Rial => "ریال",
                BzCurrency.Dollar => "دلار",
                BzCurrency.Pound => "پوند",
                BzCurrency.Euro => "یورو",
                _ => ""
            };
            return string.IsNullOrEmpty(suffix) ? words : $"{words} {suffix}";
        }
    }

    private string? CurrencySymbol => Mode == BzInputMode.Money ? CurrencyHelper.Symbol(Currency) : null;
    private bool CurrencyAfter => CurrencyHelper.SymbolAfter(Currency);

    private string WrapperClass => RootCss("bz-input")
        .Add($"bz-input--{Size.ToString().ToLowerInvariant()}")
        .AddIf(IsDisabled, "is-disabled")
        .AddIf(IsReadOnly, "is-readonly")
        .AddIf(Mode == BzInputMode.Money, "bz-input--money")
        .AddIf(HasError, "is-invalid")
        .Build();

    protected override void OnFieldParametersSet()
    {
        if (!_focused)
            _display = FormatForDisplay(Value);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_focused && _display != _domValue)
        {
            _domValue = _display;
            await SyncDomAsync();
        }
    }

    private async Task OnInputAsync(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        var cleaned = Clean(raw);
        _display = FormatRaw(cleaned);

        if (_display != raw)
        {
            int? caret = (Mode is BzInputMode.Mask or BzInputMode.Money) ? _display.Length : (int?)null;
            await SyncDomAsync(caret);
        }
        _domValue = _display;

        if (UpdateTrigger == BzUpdateTrigger.OnInput)
        {
            var parsed = ConvertBack(cleaned);
            if (!EqualityComparer<TValue?>.Default.Equals(parsed, Value))
                await SetValueAsync(parsed);
        }
    }

    private Task OnChangeAsync(ChangeEventArgs e) => ApplyAsync(e.Value?.ToString());

    private async Task ApplyAsync(string? raw)
    {
        var cleaned = Clean(raw);
        _display = FormatRaw(cleaned);
        _domValue = _display;
        await SyncDomAsync();

        var parsed = ConvertBack(cleaned);
        if (!EqualityComparer<TValue?>.Default.Equals(parsed, Value))
            await SetValueAsync(parsed);

        StateHasChanged();
    }

    private async Task SyncDomAsync(int? caret = null)
    {
        try { await JS.InvokeVoidAsync("bzInput.setValue", _input, _display, caret); }
        catch { /* prerender */ }
    }

    private async Task ClearAsync()
    {
        _display = string.Empty;
        _domValue = string.Empty;
        await SyncDomAsync();
        await SetValueAsync(default);
        StateHasChanged();
    }

    private string Clean(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        raw = PersianHelper.ToLatinDigits(PersianHelper.FixArabicChars(raw));

        if (Mode == BzInputMode.Money)
            return new string(raw.Where(c => char.IsDigit(c) || c == '-').ToArray());

        if (Mode == BzInputMode.Mask && !string.IsNullOrEmpty(Mask))
            return ApplyMask(raw, Mask!);

        return CharFilter switch
        {
            BzCharFilter.NumberOnly => new string(raw.Where(c => char.IsDigit(c) || c == '-').ToArray()),
            BzCharFilter.Decimal => FilterDecimal(raw),
            BzCharFilter.PersianOnly => new string(raw.Where(c => PersianHelper.IsPersianLetter(c) || char.IsWhiteSpace(c)).ToArray()),
            BzCharFilter.EnglishOnly => new string(raw.Where(c => PersianHelper.IsEnglishLetter(c) || char.IsWhiteSpace(c)).ToArray()),
            BzCharFilter.PersianAndEnglish => new string(raw.Where(c => PersianHelper.IsPersianLetter(c) || PersianHelper.IsEnglishLetter(c) || char.IsWhiteSpace(c)).ToArray()),
            _ => raw
        };
    }

    private static string FilterDecimal(string raw)
    {
        var sb = new System.Text.StringBuilder();
        var dotSeen = false;
        foreach (var c in raw)
        {
            if (char.IsDigit(c) || (c == '-' && sb.Length == 0)) sb.Append(c);
            else if (c == '.' && !dotSeen) { sb.Append('.'); dotSeen = true; }
        }
        return sb.ToString();
    }

    private static string ApplyMask(string raw, string mask)
    {
        var result = new System.Text.StringBuilder();
        var i = 0;
        foreach (var m in mask)
        {
            if (i >= raw.Length) break;
            switch (m)
            {
                case '#':
                    while (i < raw.Length && !char.IsDigit(raw[i])) i++;
                    if (i < raw.Length) { result.Append(raw[i]); i++; }
                    break;
                case 'A':
                    while (i < raw.Length && !char.IsLetter(raw[i])) i++;
                    if (i < raw.Length) { result.Append(raw[i]); i++; }
                    break;
                case '*':
                    result.Append(raw[i]); i++;
                    break;
                default:                 // literal separator
                    result.Append(m);
                    if (raw[i] == m) i++;
                    break;
            }
        }
        return result.ToString();
    }

    private string FormatForDisplay(TValue? value)
    {
        if (value is null) return string.Empty;
        var s = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        return FormatRaw(s);
    }

    private string FormatRaw(string cleaned)
    {
        var shown = Mode == BzInputMode.Money ? CurrencyHelper.GroupThousands(cleaned) : cleaned;
        return ShowPersianDigits ? PersianHelper.ToPersianDigits(shown) : shown;
    }

    private TValue? ConvertBack(string cleaned)
    {
        var t = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (t == typeof(string))
            return (TValue?)(object?)cleaned;

        if (string.IsNullOrWhiteSpace(cleaned) || cleaned == "-")
            return default;

        try
        {
            object? val;
            if (t == typeof(int)) val = int.Parse(cleaned, CultureInfo.InvariantCulture);
            else if (t == typeof(long)) val = long.Parse(cleaned, CultureInfo.InvariantCulture);
            else if (t == typeof(decimal)) val = decimal.Parse(cleaned, CultureInfo.InvariantCulture);
            else if (t == typeof(double)) val = double.Parse(cleaned, CultureInfo.InvariantCulture);
            else if (t == typeof(float)) val = float.Parse(cleaned, CultureInfo.InvariantCulture);
            else val = Convert.ChangeType(cleaned, t, CultureInfo.InvariantCulture);
            return (TValue?)val;
        }
        catch
        {
            return Value; // keep last good value on parse failure
        }
    }

    public ValueTask FocusAsync() => _input.FocusAsync();
    public Task ClearAsyncPublic() => ClearAsync();
}
