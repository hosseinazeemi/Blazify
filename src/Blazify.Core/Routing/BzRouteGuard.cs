namespace BzCore.Routing;

/// <summary>Everything a guard needs to decide whether a navigation may proceed.</summary>
public sealed class BzGuardContext
{
    /// <summary>Absolute URI being navigated to.</summary>
    public required string TargetUri { get; init; }

    /// <summary>Path + query, relative to the app base (e.g. <c>/orders/12?tab=lines</c>).</summary>
    public required string RelativeUri { get; init; }

    /// <summary>The page component that is about to render.</summary>
    public required Type PageType { get; init; }

    /// <summary>Route parameters matched from the template.</summary>
    public required IReadOnlyDictionary<string, object?> RouteValues { get; init; }

    /// <summary>Metadata declared with <see cref="BzRouteAttribute"/>, if any.</summary>
    public BzRouteAttribute? Meta { get; init; }

    /// <summary>Scoped service provider — resolve your own services from here.</summary>
    public required IServiceProvider Services { get; init; }
}

/// <summary>The decision a guard returns. Create one with the static factories.</summary>
public abstract class BzGuardResult
{
    private BzGuardResult() { }

    /// <summary>Let the navigation through.</summary>
    public static BzGuardResult Allow { get; } = new AllowResult();

    /// <summary>Send the user somewhere else instead.</summary>
    public static BzGuardResult Redirect(string uri, bool replace = false) => new RedirectResult(uri, replace);

    /// <summary>Block the page and render the router's <c>Denied</c> template.</summary>
    public static BzGuardResult Deny(string? reason = null) => new DenyResult(reason);

    internal sealed class AllowResult : BzGuardResult { }

    internal sealed class RedirectResult : BzGuardResult
    {
        public RedirectResult(string uri, bool replace) { Uri = uri; Replace = replace; }
        public string Uri { get; }
        public bool Replace { get; }
    }

    internal sealed class DenyResult : BzGuardResult
    {
        public DenyResult(string? reason) { Reason = reason; }
        public string? Reason { get; }
    }
}

/// <summary>
/// Runs before a page renders and can allow, redirect or block it. Guards are the
/// Blazor answer to Next.js middleware and React Router loaders' auth checks.
/// Register the implementation in DI (any lifetime) and attach it with
/// <see cref="BzGuardAttribute"/>.
/// </summary>
/// <example>
/// <code>
/// public sealed class AdminGuard : IBzRouteGuard
/// {
///     private readonly AuthenticationStateProvider _auth;
///     public AdminGuard(AuthenticationStateProvider auth) => _auth = auth;
///
///     public async ValueTask&lt;BzGuardResult&gt; CanActivateAsync(BzGuardContext ctx, CancellationToken ct)
///     {
///         var state = await _auth.GetAuthenticationStateAsync();
///         return state.User.IsInRole("Admin")
///             ? BzGuardResult.Allow
///             : BzGuardResult.Redirect($"/login?returnUrl={Uri.EscapeDataString(ctx.RelativeUri)}");
///     }
/// }
/// </code>
/// </example>
public interface IBzRouteGuard
{
    ValueTask<BzGuardResult> CanActivateAsync(BzGuardContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Attaches a guard to a page. Repeat it to chain several; they run in
/// <see cref="Order"/> then declaration order, and the first non-Allow result wins.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class BzGuardAttribute : Attribute
{
    public BzGuardAttribute(Type guardType)
    {
        if (!typeof(IBzRouteGuard).IsAssignableFrom(guardType))
            throw new ArgumentException(
                $"{guardType.FullName} must implement {nameof(IBzRouteGuard)}.", nameof(guardType));
        GuardType = guardType;
    }

    public Type GuardType { get; }

    /// <summary>Lower runs first. Default 0.</summary>
    public int Order { get; set; }
}
