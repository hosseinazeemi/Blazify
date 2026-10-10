using BzCore.Enums;

namespace BzCore.Theming;

public interface IBzThemeService
{
    BzTheme Theme { get; }
    BzThemeKind Kind { get; }
    BzAppearance Appearance { get; }
    BzDirection Direction { get; }
    BzFont Font { get; }
    bool Dense { get; }

    event Action? Changed;

    void SetTheme(BzThemeKind kind);
    void SetTheme(BzTheme theme);
    void SetAppearance(BzAppearance appearance);
    void SetDirection(BzDirection direction);
    void ToggleDirection();
    void SetFont(BzFont font);
    void SetDense(bool dense);

    string BuildCssVariables();
}
