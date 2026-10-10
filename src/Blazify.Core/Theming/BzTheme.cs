using BzCore.Enums;

namespace BzCore.Theming;
public sealed class BzTheme
{
    public string Name { get; set; } = "custom";

    public BzPalette Palette { get; set; } = new();

    public BzAppearance Appearance { get; set; } = BzAppearance.Modern;

    public string Radius { get; set; } = "10px";
    public string RadiusSmall { get; set; } = "6px";
    public string RadiusLarge { get; set; } = "16px";
    public string BorderWidth { get; set; } = "1px";
    public string Shadow { get; set; } = "0 1px 2px rgba(16,24,40,.06),0 4px 12px rgba(16,24,40,.08)";
    public string ShadowLarge { get; set; } = "0 8px 30px rgba(16,24,40,.16)";

    public string TransitionFast { get; set; } = "120ms cubic-bezier(.2,.8,.2,1)";
    public string Transition { get; set; } = "220ms cubic-bezier(.2,.8,.2,1)";

    public string ControlHeight { get; set; } = "42px";
    public string FontSize { get; set; } = "14px";

    public string ToCssVariables() =>
        Palette.ToCssVariables() +
        $"--bz-radius:{Radius};--bz-radius-sm:{RadiusSmall};--bz-radius-lg:{RadiusLarge};" +
        $"--bz-border-width:{BorderWidth};--bz-shadow:{Shadow};--bz-shadow-lg:{ShadowLarge};" +
        $"--bz-transition-fast:{TransitionFast};--bz-transition:{Transition};" +
        $"--bz-control-h:{ControlHeight};--bz-font-size:{FontSize};";

    public BzTheme WithAppearance(BzAppearance appearance)
    {
        Appearance = appearance;
        switch (appearance)
        {
            case BzAppearance.Official:
                Radius = "6px"; RadiusSmall = "4px"; RadiusLarge = "6px";
                Shadow = "0 1px 2px rgba(16,24,40,.08)";
                ShadowLarge = "0 4px 14px rgba(16,24,40,.12)";
                BorderWidth = "1px";
                break;

            case BzAppearance.Gaming:
                Radius = "4px"; RadiusSmall = "3px"; RadiusLarge = "4px";
                Shadow = "0 0 0 1px rgba(0,0,0,.5),0 6px 18px rgba(0,0,0,.45)";
                ShadowLarge = "0 0 24px rgba(0,0,0,.6)";
                BorderWidth = "2px";
                break;

            case BzAppearance.Modern:
            default:
                Radius = "10px"; RadiusSmall = "6px"; RadiusLarge = "16px";
                Shadow = "0 1px 2px rgba(16,24,40,.06),0 4px 12px rgba(16,24,40,.08)";
                ShadowLarge = "0 8px 30px rgba(16,24,40,.16)";
                BorderWidth = "1px";
                break;

            case BzAppearance.Corporate:
                Radius = "12px"; RadiusSmall = "8px"; RadiusLarge = "20px";
                Shadow = "0 2px 6px -1px rgba(34,41,47,.08),0 1px 3px rgba(34,41,47,.06)";
                ShadowLarge = "0 16px 40px -12px rgba(34,41,47,.28)";
                BorderWidth = "1px";
                break;
        }
        return this;
    }
}
