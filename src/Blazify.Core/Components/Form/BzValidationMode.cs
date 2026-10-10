namespace BzCore.Enums;

[Flags]
public enum BzValidationMode
{
    None = 0,
    DataAnnotations = 1 << 0,
    FluentValidation = 1 << 1,
    Custom = 1 << 2
}
