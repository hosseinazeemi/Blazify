namespace BzCore.Enums;

public enum BzAppearance
{
    Modern,
    Official,
    Gaming,
    Corporate
}

public enum BzThemeKind
{
    Light,
    Dark,
    LightGray,
    LightPurple
}

public enum BzDirection
{
    Rtl,
    Ltr
}

public enum BzFont
{
    Vazirmatn,
    IranSans,
    Dana,
    Yekan
}

public enum BzColor
{
    Default,
    Primary,
    Secondary,
    Success,
    Info,
    Warning,
    Danger,
    Surface
}

public enum BzSize
{
    Small,
    Medium,
    Large
}

public enum BzButtonVariant
{
    Filled,
    Outlined,
    Text,
    Soft
}

public enum BzInputMode
{
    Normal,
    Money,
    Mask
}

public enum BzCharFilter
{
    Any,
    PersianOnly,
    EnglishOnly,
    PersianAndEnglish,
    NumberOnly,
    Decimal
}

public enum BzCurrency
{
    Rial,
    Toman,
    Dollar,
    Pound,
    Euro
}

public enum BzUpdateTrigger
{
    OnChange,
    OnInput
}

public enum BzLoadingEffect
{
    Spinner,
    Dots,
    Bar,
    Pulse
}
