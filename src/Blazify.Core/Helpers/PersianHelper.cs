using System.Globalization;
using System.Text;

namespace BzCore.Helpers;

public static class PersianHelper
{
    private static readonly char[] PersianDigits = { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
    private static readonly char[] ArabicDigits = { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };

    public static string ToLatinDigits(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            var pi = Array.IndexOf(PersianDigits, ch);
            if (pi >= 0) { sb.Append((char)('0' + pi)); continue; }
            var ai = Array.IndexOf(ArabicDigits, ch);
            if (ai >= 0) { sb.Append((char)('0' + ai)); continue; }
            sb.Append(ch);
        }
        return sb.ToString();
    }

    public static string ToPersianDigits(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
            sb.Append(ch is >= '0' and <= '9' ? PersianDigits[ch - '0'] : ch);
        return sb.ToString();
    }

    public static bool IsPersianLetter(char c) =>
        (c >= '\u0600' && c <= '\u06FF') || (c >= '\uFB8A' && c <= '\uFBFC') || c == '\u200C';

    public static bool IsEnglishLetter(char c) =>
        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

    public static string FixArabicChars(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
        return input.Replace('ي', 'ی').Replace('ك', 'ک');
    }
}
