using System.Globalization;
using BzCore.Enums;

namespace BzCore.Helpers;

public static class CurrencyHelper
{
    public static string Symbol(BzCurrency currency) => currency switch
    {
        BzCurrency.Rial => "ریال",
        BzCurrency.Toman => "تومان",
        BzCurrency.Dollar => "$",
        BzCurrency.Pound => "£",
        BzCurrency.Euro => "€",
        _ => ""
    };

    public static bool SymbolAfter(BzCurrency currency)
        => currency is BzCurrency.Rial or BzCurrency.Toman;

    public static string GroupThousands(string? digits)
    {
        digits = PersianHelper.ToLatinDigits(digits);
        if (string.IsNullOrEmpty(digits)) return string.Empty;

        var negative = digits.StartsWith('-');
        if (negative) digits = digits[1..];

        var dot = digits.IndexOf('.');
        var intPart = dot >= 0 ? digits[..dot] : digits;
        var fracPart = dot >= 0 ? digits[dot..] : string.Empty;

        intPart = new string(intPart.Where(char.IsDigit).ToArray());
        if (intPart.Length == 0) intPart = "0";

        if (long.TryParse(intPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            intPart = value.ToString("#,0", CultureInfo.InvariantCulture);

        return (negative ? "-" : "") + intPart + fracPart;
    }

    public static string Format(string? digits, BzCurrency currency)
    {
        var grouped = GroupThousands(digits);
        if (string.IsNullOrEmpty(grouped)) return string.Empty;
        var sym = Symbol(currency);
        return SymbolAfter(currency) ? $"{grouped} {sym}" : $"{sym}{grouped}";
    }
}
