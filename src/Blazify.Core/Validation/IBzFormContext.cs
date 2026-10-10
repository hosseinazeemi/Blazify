using Microsoft.AspNetCore.Components.Forms;

namespace BzCore.Validation;

public interface IBzFormContext
{
    EditContext EditContext { get; }

    void RegisterField(FieldIdentifier field, string? group);

    void UnregisterField(FieldIdentifier field);
}
