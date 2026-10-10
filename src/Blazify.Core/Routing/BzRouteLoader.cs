namespace BzCore.Routing;

/// <summary>Everything a loader needs to fetch the data a page will render.</summary>
public sealed class BzLoaderContext
{
    public required string RelativeUri { get; init; }
    public required Type PageType { get; init; }
    public required IReadOnlyDictionary<string, object?> RouteValues { get; init; }
    public required IServiceProvider Services { get; init; }

    /// <summary>Reads a route parameter and converts it, e.g. <c>Param&lt;int&gt;("Id")</c>.</summary>
    public T? Param<T>(string name)
    {
        if (!RouteValues.TryGetValue(name, out var raw) || raw is null) return default;
        if (raw is T typed) return typed;
        try { return (T)Convert.ChangeType(raw, typeof(T)); }
        catch { return default; }
    }
}

/// <summary>
/// Fetches a page's data <em>before</em> the page renders, so the page never has to
/// juggle a null model or its own spinner. This is React Router's <c>loader</c>
/// idea brought to Blazor.
/// </summary>
/// <remarks>
/// Throwing from <see cref="LoadAsync"/> renders the router's <c>ErrorContent</c>
/// template instead of the page.
/// </remarks>
/// <example>
/// <code>
/// public sealed class OrderLoader : IBzRouteLoader
/// {
///     private readonly IOrderService _orders;
///     public OrderLoader(IOrderService orders) => _orders = orders;
///
///     public async ValueTask&lt;object?&gt; LoadAsync(BzLoaderContext ctx, CancellationToken ct)
///         =&gt; await _orders.GetAsync(ctx.Param&lt;int&gt;("Id"), ct);
/// }
/// </code>
/// Then on the page:
/// <code>
/// @attribute [BzLoader(typeof(OrderLoader))]
/// @code {
///     [CascadingParameter] public BzRouteContext Route { get; set; } = default!;
///     private Order? Order =&gt; Route.DataAs&lt;Order&gt;();
/// }
/// </code>
/// </example>
public interface IBzRouteLoader
{
    ValueTask<object?> LoadAsync(BzLoaderContext context, CancellationToken cancellationToken);
}

/// <summary>Strongly typed convenience base. Implement <see cref="LoadAsync"/> and return <typeparamref name="T"/>.</summary>
public abstract class BzRouteLoader<T> : IBzRouteLoader
{
    public abstract ValueTask<T?> LoadAsync(BzLoaderContext context, CancellationToken cancellationToken);

    async ValueTask<object?> IBzRouteLoader.LoadAsync(BzLoaderContext context, CancellationToken cancellationToken)
        => await LoadAsync(context, cancellationToken);
}

/// <summary>Attaches a data loader to a page. One per page.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class BzLoaderAttribute : Attribute
{
    public BzLoaderAttribute(Type loaderType)
    {
        if (!typeof(IBzRouteLoader).IsAssignableFrom(loaderType))
            throw new ArgumentException(
                $"{loaderType.FullName} must implement {nameof(IBzRouteLoader)}.", nameof(loaderType));
        LoaderType = loaderType;
    }

    public Type LoaderType { get; }

    /// <summary>
    /// When true the previous page stays on screen while the loader runs, instead of
    /// swapping to the loading template. Mirrors React Router's pending UI and keeps
    /// fast navigations from flashing a spinner.
    /// </summary>
    public bool KeepPreviousUi { get; set; }
}
