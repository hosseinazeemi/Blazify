using Microsoft.AspNetCore.Components;

namespace BzCore.Routing;

/// <summary>A single step in a breadcrumb trail.</summary>
/// <param name="Text">Label to show.</param>
/// <param name="Href">Link target, or null for the current (last) step.</param>
/// <param name="Icon">Optional FontAwesome class.</param>
public sealed record BzBreadcrumbStep(string Text, string? Href, string? Icon = null);

/// <summary>
/// Cascaded by <see cref="BzRouter"/> to every page. Gives the page its metadata,
/// its loader result and the breadcrumb trail, with no plumbing of its own.
/// </summary>
/// <example>
/// <code>
/// [CascadingParameter] public BzRouteContext Route { get; set; } = default!;
/// private Order? Order =&gt; Route.DataAs&lt;Order&gt;();
/// </code>
/// </example>
public sealed class BzRouteContext
{
    /// <summary>Path + query relative to the app base, e.g. <c>/orders/12?tab=lines</c>.</summary>
    public string RelativeUri { get; init; } = "/";

    /// <summary>The page component being rendered.</summary>
    public Type? PageType { get; init; }

    /// <summary>Route template that matched, e.g. <c>/orders/{Id:int}</c>.</summary>
    public string? Template { get; init; }

    /// <summary>Metadata from <see cref="BzRouteAttribute"/>, if the page declared any.</summary>
    public BzRouteAttribute? Meta { get; init; }

    /// <summary>Route parameters matched from the template.</summary>
    public IReadOnlyDictionary<string, object?> RouteValues { get; init; }
        = new Dictionary<string, object?>();

    /// <summary>Whatever the page's loader returned, or null when it has none.</summary>
    public object? Data { get; init; }

    /// <summary>Breadcrumb trail built from the <see cref="BzRouteAttribute.Parent"/> chain.</summary>
    public IReadOnlyList<BzBreadcrumbStep> Breadcrumbs { get; init; } = Array.Empty<BzBreadcrumbStep>();

    /// <summary>True while a loader is running for a pending navigation.</summary>
    public bool IsLoading { get; init; }

    /// <summary>Casts <see cref="Data"/>, returning default when it is absent or another type.</summary>
    public T? DataAs<T>() => Data is T typed ? typed : default;

    /// <summary>Reads a route parameter and converts it.</summary>
    public T? Param<T>(string name)
    {
        if (!RouteValues.TryGetValue(name, out var raw) || raw is null) return default;
        if (raw is T typed) return typed;
        try { return (T)Convert.ChangeType(raw, typeof(T)); }
        catch { return default; }
    }
}
