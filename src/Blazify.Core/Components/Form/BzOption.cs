namespace BzCore.Components.Form;

/// <summary>You can use in BzSelect, BzRadioGroup, BzMultiSelect</summary>
public sealed class BzOption<TValue>
{
    public TValue Value { get; set; } = default!;
    public string Label { get; set; } = "";
    public string? Icon { get; set; }
    public bool Disabled { get; set; }

    public BzOption() { }

    public BzOption(TValue value, string label, string? icon = null)
    {
        Value = value; Label = label; Icon = icon;
    }
}
