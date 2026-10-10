namespace BzCore.Helpers;

public static class IdentifierHelper
{
    public static string New(string prefix = "Bz")
        => $"{prefix}-{Guid.NewGuid():N}"[..(prefix.Length + 13)];
}
