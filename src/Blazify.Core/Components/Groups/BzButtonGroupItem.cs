namespace BzCore.Components.Groups;

public sealed class BzButtonGroupItem<TValue>
{
    public TValue Value { get; set; } = default!;

    public string? Label { get; set; }

    public string? Icon { get; set; }

    public bool Disabled { get; set; }

    public BzButtonGroupItem() { }

    public BzButtonGroupItem(TValue value, string? label = null, string? icon = null)
    {
        Value = value;
        Label = label;
        Icon = icon;
    }
}
