using System.Numerics;
using BzCore.Enums;

namespace BzCore.Helpers;

public readonly record struct IdentifierValidationResult(
    bool IsValid,
    string? BankKey = null,
    string? BankName = null,
    string? Message = null)
{
    public static IdentifierValidationResult Ok(string? bankKey = null, string? bankName = null)
        => new(true, bankKey, bankName);

    public static IdentifierValidationResult Fail(string message)
        => new(false, Message: message);
}

public static class IdentifierValidator
{
    public static IdentifierValidationResult Validate(
        BzIdentifierType type, string? raw, IReadOnlyCollection<string>? allowedBanks = null, string? country = null)
    {
        var digits = OnlyDigits(raw);

        return type switch
        {
            BzIdentifierType.CardNumber => ValidateCard(digits, allowedBanks),
            BzIdentifierType.Iban => ValidateIban(raw, allowedBanks),
            BzIdentifierType.NationalCode => ValidateNationalCode(digits),
            BzIdentifierType.LegalId => ValidateLegalId(digits),
            BzIdentifierType.ForeignerId => ValidateLength(digits, 12, "شناسه فراگیر باید 12 رقم باشد."),
            BzIdentifierType.AccountNumber => digits.Length >= 4
                ? IdentifierValidationResult.Ok()
                : IdentifierValidationResult.Fail("شماره حساب نامعتبر است."),
            BzIdentifierType.Passport => ValidatePassport(raw, country),
            _ => string.IsNullOrEmpty(raw)
                ? IdentifierValidationResult.Fail("مقدار خالی است.")
                : IdentifierValidationResult.Ok()
        };
    }

    public static IdentifierValidationResult ValidateCard(string digits, IReadOnlyCollection<string>? allowedBanks)
    {
        if (digits.Length != 16)
            return IdentifierValidationResult.Fail("شماره کارت باید 16 رقم باشد.");
        if (!Luhn(digits))
            return IdentifierValidationResult.Fail("شماره کارت معتبر نیست.");

        var bin = digits[..6];
        IranBankData.CardBins.TryGetValue(bin, out var bankKey);
        var bank = IranBankData.BankByKey(bankKey);

        if (allowedBanks is { Count: > 0 })
        {
            if (bankKey is null || !allowedBanks.Contains(bankKey))
                return IdentifierValidationResult.Fail("کارت متعلق به بانک های مجاز نیست.");
        }

        return IdentifierValidationResult.Ok(bankKey, bank?.Name);
    }

    public static IdentifierValidationResult ValidateIban(string? raw, IReadOnlyCollection<string>? allowedBanks)
    {
        var s = (raw ?? "").ToUpperInvariant().Replace(" ", "").Replace("-", "");
        if (s.StartsWith("IR")) s = s; else s = "IR" + OnlyDigits(raw);
        if (s.Length != 26 || !s.StartsWith("IR"))
            return IdentifierValidationResult.Fail("شبا باید با IR و 24 رقم باشد.");
        if (!s[2..].All(char.IsDigit))
            return IdentifierValidationResult.Fail("شبا فقط باید شامل رقم باشد.");

        if (!Mod97(s))
            return IdentifierValidationResult.Fail("شماره شبا معتبر نیست.");

        var bankCode = s.Substring(4, 3);
        IranBankData.IbanCodes.TryGetValue(bankCode, out var bankKey);
        var bank = IranBankData.BankByKey(bankKey);

        if (allowedBanks is { Count: > 0 })
        {
            if (bankKey is null || !allowedBanks.Contains(bankKey))
                return IdentifierValidationResult.Fail("شبا متعلق به بانک های مجاز نیست.");
        }

        return IdentifierValidationResult.Ok(bankKey, bank?.Name);
    }

    public static IdentifierValidationResult ValidateNationalCode(string digits)
    {
        if (digits.Length != 10)
            return IdentifierValidationResult.Fail("کد ملی باید 10 رقم باشد.");
        if (digits.Distinct().Count() == 1)
            return IdentifierValidationResult.Fail("کد ملی معتبر نیست.");

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (digits[i] - '0') * (10 - i);
        var rem = sum % 11;
        var check = digits[9] - '0';
        var valid = (rem < 2 && check == rem) || (rem >= 2 && check == 11 - rem);
        return valid
            ? IdentifierValidationResult.Ok()
            : IdentifierValidationResult.Fail("کد ملی معتبر نیست.");
    }

    public static IdentifierValidationResult ValidateLegalId(string digits)
    {
        if (digits.Length != 11)
            return IdentifierValidationResult.Fail("شناسه ملی باید 11 رقم باشد.");

        var d = digits.Select(c => c - '0').ToArray();
        var dec = d[9] + 2;
        int[] coef = { 29, 27, 23, 19, 17, 29, 27, 23, 19, 17 };
        var sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (d[i] + dec) * coef[i];
        var rem = sum % 11;
        if (rem == 10) rem = 0;
        return rem == d[10]
            ? IdentifierValidationResult.Ok()
            : IdentifierValidationResult.Fail("شناسه ملی معتبر نیست.");
    }

    public static IdentifierValidationResult ValidatePassport(string? raw, string? country)
    {
        var s = (raw ?? "").Trim().ToUpperInvariant();
        if (s.Length == 0) return IdentifierValidationResult.Fail("شماره پاسپورت خالی است.");

        var c = (country ?? "IR").ToUpperInvariant();
        var pattern = c switch
        {
            "IR" => @"^[A-Z]?\d{8,9}$",        // ایران
            "US" => @"^\d{9}$",
            "GB" => @"^\d{9}$",
            "DE" => @"^[CFGHJKLMNPRTVWXYZ0-9]{9}$",
            _ => @"^[A-Z0-9]{6,12}$"            // عمومی
        };
        return System.Text.RegularExpressions.Regex.IsMatch(s, pattern)
            ? IdentifierValidationResult.Ok()
            : IdentifierValidationResult.Fail("شماره پاسپورت معتبر نیست.");
    }

    public static string OnlyDigits(string? raw)
        => new((PersianHelper.ToLatinDigits(raw) ?? "").Where(char.IsDigit).ToArray());

    private static IdentifierValidationResult ValidateLength(string digits, int len, string msg)
        => digits.Length == len ? IdentifierValidationResult.Ok() : IdentifierValidationResult.Fail(msg);

    // algoritm lon bara shomare kart
    private static bool Luhn(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var d = digits[i] - '0';
            if (i % 2 == 0)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
        }
        return sum % 10 == 0;
    }

    // mod97 vase validate o mohasebe sheba
    private static bool Mod97(string iban)
    {
        var rearranged = iban[4..] + iban[..4];
        var sb = new System.Text.StringBuilder();
        foreach (var ch in rearranged)
        {
            if (char.IsDigit(ch)) sb.Append(ch);
            else sb.Append((ch - 'A' + 10).ToString());
        }
        return BigInteger.TryParse(sb.ToString(), out var big) && big % 97 == 1;
    }
}
