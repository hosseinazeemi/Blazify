using System.Text.RegularExpressions;

namespace BzCore.Helpers;

public static class IranValidation
{
    public static bool IsNationalCode(string? input)
    {
        var s = PersianHelper.ToLatinDigits(input);
        if (string.IsNullOrEmpty(s) || !Regex.IsMatch(s, @"^\d{10}$")) return false;
        if (s.Distinct().Count() == 1) return false;
        var check = s[9] - '0';
        var sum = 0;
        for (var i = 0; i < 9; i++) sum += (s[i] - '0') * (10 - i);
        var r = sum % 11;
        return (r < 2 && check == r) || (r >= 2 && check == 11 - r);
    }

    public static bool IsMobile(string? input)
    {
        var s = PersianHelper.ToLatinDigits(input);
        return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^09\d{9}$");
    }

    public static bool IsIban(string? input)
    {
        var s = (PersianHelper.ToLatinDigits(input) ?? "").Replace(" ", "").ToUpperInvariant();
        if (s.StartsWith("IR")) s = s;
        else if (Regex.IsMatch(s, @"^\d{24}$")) s = "IR" + s;
        if (!Regex.IsMatch(s, @"^IR\d{24}$")) return false;

        var rearranged = s[4..] + s[..4];
        var numeric = "";
        foreach (var c in rearranged)
            numeric += char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString();

        var rem = 0;
        foreach (var c in numeric) rem = (rem * 10 + (c - '0')) % 97;
        return rem == 1;
    }

    public static string? GetIbanBank(string? iban)
    {
        var s = (PersianHelper.ToLatinDigits(iban) ?? "").Replace(" ", "").ToUpperInvariant();
        if (s.StartsWith("IR")) s = s[2..];
        if (s.Length < 5) return null;
        var code = s.Substring(2, 3);
        return IbanBanks.TryGetValue(code, out var name) ? name : null;
    }

    public static bool IsBankCard(string? input)
    {
        var s = PersianHelper.ToLatinDigits(input)?.Replace("-", "").Replace(" ", "") ?? "";
        if (!Regex.IsMatch(s, @"^\d{16}$")) return false;
        var sum = 0;
        for (var i = 0; i < 16; i++)
        {
            var d = s[i] - '0';
            if (i % 2 == 0) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
        }
        return sum % 10 == 0;
    }

    public static string? GetCardBank(string? card)
    {
        var s = PersianHelper.ToLatinDigits(card)?.Replace("-", "").Replace(" ", "") ?? "";
        if (s.Length < 6) return null;
        return CardBins.TryGetValue(s[..6], out var name) ? name : null;
    }

    private static readonly Dictionary<string, string> CardBins = new()
    {
        ["603799"] = "ملی",
        ["589210"] = "سپه",
        ["627648"] = "توسعه صادرات",
        ["627961"] = "صنعت و معدن",
        ["603770"] = "کشاورزی",
        ["628023"] = "مسکن",
        ["627760"] = "پست بانک",
        ["502908"] = "توسعه تعاون",
        ["627412"] = "اقتصاد نوین",
        ["622106"] = "پارسیان",
        ["639194"] = "پارسیان",
        ["627884"] = "پارسیان",
        ["502229"] = "پاسارگاد",
        ["639347"] = "پاسارگاد",
        ["627488"] = "کارآفرین",
        ["502910"] = "کارآفرین",
        ["621986"] = "سامان",
        ["639346"] = "سینا",
        ["639607"] = "سرمایه",
        ["636214"] = "تات/آینده",
        ["502938"] = "دی",
        ["603769"] = "صادرات",
        ["610433"] = "ملت",
        ["991975"] = "ملت",
        ["589463"] = "رفاه",
        ["627381"] = "انصار",
        ["639370"] = "مهر اقتصاد",
        ["636949"] = "حکمت ایرانیان",
        ["505785"] = "ایران زمین",
        ["636795"] = "مرکزی",
        ["505416"] = "گردشگری",
        ["606373"] = "قرض الحسنه مهر",
        ["628157"] = "اعتباری توسعه",
        ["502806"] = "شهر",
        ["504172"] = "رسالت",
        ["606256"] = "ملل",
    };

    private static readonly Dictionary<string, string> IbanBanks = new()
    {
        ["010"] = "مرکزی",
        ["011"] = "صنعت و معدن",
        ["012"] = "ملت",
        ["013"] = "رفاه",
        ["014"] = "مسکن",
        ["015"] = "سپه",
        ["016"] = "کشاورزی",
        ["017"] = "ملی",
        ["018"] = "تجارت",
        ["019"] = "صادرات",
        ["020"] = "توسعه صادرات",
        ["021"] = "پست بانک",
        ["022"] = "توسعه تعاون",
        ["051"] = "موسسه توسعه",
        ["053"] = "کارآفرین",
        ["054"] = "پارسیان",
        ["055"] = "اقتصاد نوین",
        ["056"] = "سامان",
        ["057"] = "پاسارگاد",
        ["058"] = "سرمایه",
        ["059"] = "سینا",
        ["060"] = "مهر ایران",
        ["061"] = "شهر",
        ["062"] = "آینده",
        ["063"] = "انصار",
        ["064"] = "گردشگری",
        ["066"] = "دی",
        ["069"] = "ایران زمین",
        ["070"] = "رسالت",
        ["073"] = "کوثر",
        ["075"] = "ملل",
        ["078"] = "خاورمیانه",
    };
}
