namespace BzCore.Validation;

/// <param name="FieldName">Model property name; empty means a form-level error.</param>
/// <param name="Message">The human-readable error text.</param>
public readonly record struct BzValidationError(string FieldName, string Message);
