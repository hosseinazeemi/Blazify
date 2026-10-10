using BzCore.Enums;

namespace BzCore.Theming;

public sealed class BzThemeService : IBzThemeService
{
    public BzTheme Theme { get; private set; }
    public BzThemeKind Kind { get; private set; }
    public BzAppearance Appearance { get; private set; }
    public BzDirection Direction { get; private set; }
    public BzFont Font { get; private set; }
    public bool Dense { get; private set; }

    public event Action? Changed;

    public BzThemeService(BzOptions options)
    {
        Theme = options.Theme ?? ThemePresets.Resolve(options.DefaultTheme);
        Kind = options.DefaultTheme;
        Appearance = options.Appearance;
        Theme.WithAppearance(Appearance);
        Direction = options.Direction; // rtl default
        Font = options.Font;
        Dense = options.Dense;
    }

    public void SetTheme(BzThemeKind kind)
    {
        Kind = kind;
        Theme = ThemePresets.Resolve(kind).WithAppearance(Appearance);
        Notify();
    }

    public void SetTheme(BzTheme theme)
    {
        Theme = theme;
        Appearance = theme.Appearance;
        Notify();
    }

    public void SetAppearance(BzAppearance appearance)
    {
        Appearance = appearance;
        Theme.WithAppearance(appearance);
        Notify();
    }

    public void SetDirection(BzDirection direction)
    {
        Direction = direction;
        Notify();
    }

    public void ToggleDirection()
        => SetDirection(Direction == BzDirection.Rtl ? BzDirection.Ltr : BzDirection.Rtl);

    public void SetFont(BzFont font)
    {
        Font = font;
        Notify();
    }

    public void SetDense(bool dense)
    {
        Dense = dense;
        Notify();
    }

    public string BuildCssVariables()
    {
        var fontStack = FontStack(Font);
        return Theme.ToCssVariables() + $"--bz-font-family:{fontStack};";
    }

    private static string FontStack(BzFont font) => font switch
    {
        BzFont.IranSans => "'IRANSans','Vazirmatn',system-ui,sans-serif",
        BzFont.Dana => "'Dana','Vazirmatn',system-ui,sans-serif",
        BzFont.Yekan => "'Yekan','Vazirmatn',system-ui,sans-serif",
        _ => "'Vazirmatn',system-ui,sans-serif"
    };

    private void Notify() => Changed?.Invoke();
}
