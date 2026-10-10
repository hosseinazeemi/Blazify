namespace BzCore.Components.Navigation;

public sealed class BzBreadcrumbItem
{
    public string Text { get; set; } = "";
    /// <summary>Optional icon font-awesome class</summary>
    public string? Icon { get; set; }
    public string? Href { get; set; }

    public BzBreadcrumbItem() { }

    public BzBreadcrumbItem(string text, string? href = null, string? icon = null)
    {
        Text = text;
        Href = href;
        Icon = icon;
    }
}

public sealed class BzMenuItem
{
    /// <summary>Menu lable</summary>
    public string Text { get; set; } = "";
    /// <summary>Optional icon</summary>
    public string? Icon { get; set; }
    public string? Href { get; set; }
    public bool Disabled { get; set; }
    /// <summary>Optional badge text</summary>
    public string? Badge { get; set; }
    /// <summary>OnClick: used when there is no Href</summary>
    public Action? OnClick { get; set; }
    /// <summary>SubItems: this node becomes an expandable parent</summary>
    public List<BzMenuItem>? Children { get; set; }
}
