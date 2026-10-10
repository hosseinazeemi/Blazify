namespace BzCore.Helpers;

// tabdile mablagh be horoof
public static class NumberToWordsHelper
{
    private static readonly string[] Yekan =
        { "", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };

    private static readonly string[] Dahgan =
        { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };

    private static readonly string[] DahYazdah =
        { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };

    private static readonly string[] Sadgan =
        { "", "صد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };

    private static readonly string[] Scales =
        { "", "هزار", "میلیون", "میلیارد", "بیلیون", "بیلیارد", "تریلیون" };

    private const string Sep = " و ";

    public static string ToPersianWords(long number)
    {
        if (number == 0) return "صفر";

        var negative = number < 0;
        var value = negative ? (ulong)(-(number + 1)) + 1UL : (ulong)number;

        var groups = new List<int>();
        while (value > 0)
        {
            groups.Add((int)(value % 1000));
            value /= 1000;
        }

        var parts = new List<string>();
        for (var i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i] == 0) continue;
            var words = ThreeDigitsToWords(groups[i]);
            var scale = Scales[i];
            parts.Add(string.IsNullOrEmpty(scale) ? words : $"{words} {scale}");
        }

        var result = string.Join(Sep, parts);
        return negative ? "منفی " + result : result;
    }

    public static string FromDigits(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var digits = new string(PersianHelper.ToLatinDigits(raw).Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return string.Empty;
        return long.TryParse(digits, out var n) ? ToPersianWords(n) : string.Empty;
    }

    private static string ThreeDigitsToWords(int n)
    {
        var parts = new List<string>();

        var hundreds = n / 100;
        var rest = n % 100;

        if (hundreds > 0) parts.Add(Sadgan[hundreds]);

        if (rest >= 10 && rest <= 19)
        {
            parts.Add(DahYazdah[rest - 10]);
        }
        else
        {
            var tens = rest / 10;
            var units = rest % 10;
            if (tens > 0) parts.Add(Dahgan[tens]);
            if (units > 0) parts.Add(Yekan[units]);
        }

        return string.Join(Sep, parts);
    }
}
