using System.ComponentModel.DataAnnotations;
using BzCore.Enums;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace BzCore.Validation;

public static class BzValidationEngine
{
    /// <summary>Validate <paramref name="model"/> using the selected <paramref name="mode"/>.</summary>
    public static List<BzValidationError> Validate(
        object model,
        BzValidationMode mode,
        object? fluentValidator,
        Func<object, IEnumerable<BzValidationError>>? customValidator)
    {
        var errors = new List<BzValidationError>();

        if (mode.HasFlag(BzValidationMode.DataAnnotations))
            RunDataAnnotations(model, errors);

        if (mode.HasFlag(BzValidationMode.Custom) && customValidator is not null)
            errors.AddRange(customValidator(model));

        return errors;
    }

    private static void RunDataAnnotations(object model, List<BzValidationError> errors)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);

        foreach (var r in results)
        {
            var message = r.ErrorMessage ?? "مقدار نامعتبر است.";
            var members = r.MemberNames?.ToList() ?? new List<string>();
            if (members.Count == 0)
                errors.Add(new BzValidationError(string.Empty, message)); // form-level
            else
                foreach (var m in members)
                    errors.Add(new BzValidationError(m, message));
        }
    }
}
