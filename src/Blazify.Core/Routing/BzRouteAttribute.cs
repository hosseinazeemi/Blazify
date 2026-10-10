namespace BzCore.Routing;

/// <summary>
/// Describes a routable page: its title, where it sits in the navigation tree and
/// how it should be indexed. Put it next to <c>@page</c> and everything else —
/// document title, breadcrumbs, generated menus, sitemaps — follows from it.
/// </summary>
/// <example>
/// <code>
/// @page "/orders/{Id:int}"
/// @attribute [BzRoute(Title = "Order details", TitleFa = "جزئیات سفارش",
///                     Parent = "/orders", Icon = "fa-solid fa-receipt")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class BzRouteAttribute : Attribute
{
    /// <summary>Document title (used for &lt;title&gt; and as the breadcrumb label fallback).</summary>
    public string? Title { get; set; }

    /// <summary>Persian title. When set, it is used while the current UI culture is fa.</summary>
    public string? TitleFa { get; set; }

    /// <summary>Meta description for this page.</summary>
    public string? Description { get; set; }

    /// <summary>Persian meta description.</summary>
    public string? DescriptionFa { get; set; }

    /// <summary>Short label for breadcrumbs and menus. Falls back to <see cref="Title"/>.</summary>
    public string? Breadcrumb { get; set; }

    /// <summary>Persian breadcrumb label. Falls back to <see cref="TitleFa"/>.</summary>
    public string? BreadcrumbFa { get; set; }

    /// <summary>Icon class (FontAwesome) shown in generated menus and breadcrumbs.</summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Route template of the parent page, e.g. <c>"/orders"</c>. Used to build the
    /// breadcrumb trail and nested menus. Leave null for a top-level page.
    /// </summary>
    public string? Parent { get; set; }

    /// <summary>Group name used when generating menus (e.g. "Sales").</summary>
    public string? Group { get; set; }

    /// <summary>Sort order inside its group. Lower comes first.</summary>
    public int Order { get; set; }

    /// <summary>When true the page is skipped by generated menus and sitemaps.</summary>
    public bool Hidden { get; set; }

    /// <summary>When true a <c>noindex</c> hint is emitted for search engines.</summary>
    public bool NoIndex { get; set; }

    /// <summary>Resolves the title for the current culture.</summary>
    public string? ResolveTitle(bool fa) => fa ? (TitleFa ?? Title) : (Title ?? TitleFa);

    /// <summary>Resolves the breadcrumb label for the current culture.</summary>
    public string? ResolveBreadcrumb(bool fa) =>
        fa ? (BreadcrumbFa ?? TitleFa ?? Breadcrumb ?? Title)
           : (Breadcrumb ?? Title ?? BreadcrumbFa ?? TitleFa);

    /// <summary>Resolves the description for the current culture.</summary>
    public string? ResolveDescription(bool fa) => fa ? (DescriptionFa ?? Description) : (Description ?? DescriptionFa);
}
