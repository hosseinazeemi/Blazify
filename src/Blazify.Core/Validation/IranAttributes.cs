using System.ComponentModel.DataAnnotations;
using BzCore.Helpers;

namespace BzCore.Validation;

public sealed class NationalCodeAttribute : ValidationAttribute
{
    public NationalCodeAttribute() => ErrorMessage = "کد ملی نامعتبر است.";
    public override bool IsValid(object? value)
        => value is null || string.IsNullOrEmpty(value.ToString()) || IranValidation.IsNationalCode(value.ToString());
}

public sealed class IranMobileAttribute : ValidationAttribute
{
    public IranMobileAttribute() => ErrorMessage = "شماره موبایل نامعتبر است.";
    public override bool IsValid(object? value)
        => value is null || string.IsNullOrEmpty(value.ToString()) || IranValidation.IsMobile(value.ToString());
}

public sealed class ShebaAttribute : ValidationAttribute
{
    public ShebaAttribute() => ErrorMessage = "شماره شبا نامعتبر است.";
    public override bool IsValid(object? value)
        => value is null || string.IsNullOrEmpty(value.ToString()) || IranValidation.IsIban(value.ToString());
}

public sealed class BankCardAttribute : ValidationAttribute
{
    public BankCardAttribute() => ErrorMessage = "شماره کارت نامعتبر است.";
    public override bool IsValid(object? value)
        => value is null || string.IsNullOrEmpty(value.ToString()) || IranValidation.IsBankCard(value.ToString());
}
