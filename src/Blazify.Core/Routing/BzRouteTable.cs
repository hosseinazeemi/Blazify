using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace BzCore.Routing;

/// <summary>One discovered page: its template plus whatever it declared.</summary>
public sealed class BzRouteEntry
{
    public required string Template { get; init; }
    public required Type PageType { get; init; }
    public BzRouteAttribute? Meta { get; init; }

    /// <summary>True when the template has no <c>{parameters}</c> and can be linked to directly.</summary>
    public bool IsStatic => !Template.Contains('{');

    public string? Title(bool fa) => Meta?.ResolveTitle(fa) ?? PageType.Name;
    public string? Breadcrumb(bool fa) => Meta?.ResolveBreadcrumb(fa) ?? PageType.Name;
    public string? Icon => Meta?.Icon;
    public string? Group => Meta?.Group;
    public int Order => Meta?.Order ?? 0;
    public bool Hidden => Meta?.Hidden ?? false;
}

/// <summary>
/// Discovers every <c>@page</c> in the given assemblies once, together with its
/// <see cref="BzRouteAttribute"/>. Blazor keeps its route table private, so this
/// exists to make routes *data* — which is what lets menus, breadcrumbs and
/// sitemaps be generated instead of hand-maintained.
/// </summary>
/// <example>
/// Build a whole nav menu with no hardcoded list:
/// <code>
/// @inject BzRouteTable Routes
/// @foreach (var g in Routes.ByGroup(fa: true))
/// {
///     &lt;h4&gt;@g.Key&lt;/h4&gt;
///     @foreach (var r in g)
///     {
///         &lt;a href="@r.Template"&gt;&lt;i class="@r.Icon"&gt;&lt;/i&gt; @r.Title(true)&lt;/a&gt;
///     }
/// }
/// </code>
/// </example>
public sealed class BzRouteTable
{
    private readonly List<BzRouteEntry> _entries = new();
    private readonly Dictionary<string, BzRouteEntry> _byTemplate =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Every discovered route, in declaration order.</summary>
    public IReadOnlyList<BzRouteEntry> Entries => _entries;

    /// <summary>Scans the assemblies for routable components. Safe to call more than once.</summary>
    public void Load(params Assembly?[] assemblies)
    {
        foreach (var asm in assemblies)
        {
            if (asm is null) continue;

            IEnumerable<Type> types;
            try { types = asm.GetExportedTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null)!; }
            catch { continue; }

            foreach (var type in types)
            {
                if (!typeof(IComponent).IsAssignableFrom(type)) continue;

                var meta = type.GetCustomAttribute<BzRouteAttribute>();
                foreach (var route in type.GetCustomAttributes<RouteAttribute>())
                {
                    var entry = new BzRouteEntry
                    {
                        Template = route.Template,
                        PageType = type,
                        Meta = meta,
                    };
                    _entries.Add(entry);
                    _byTemplate[route.Template] = entry;
                }
            }
        }

        _entries.Sort((a, b) =>
        {
            var g = string.CompareOrdinal(a.Group ?? "", b.Group ?? "");
            if (g != 0) return g;
            var o = a.Order.CompareTo(b.Order);
            return o != 0 ? o : string.CompareOrdinal(a.Template, b.Template);
        });
    }

    /// <summary>Finds a route by its exact template.</summary>
    public BzRouteEntry? ByTemplate(string template)
        => _byTemplate.TryGetValue(template, out var e) ? e : null;

    /// <summary>Finds the entry for a page component.</summary>
    public BzRouteEntry? ByPage(Type pageType)
        => _entries.FirstOrDefault(e => e.PageType == pageType);

    /// <summary>Visible, linkable routes grouped by <see cref="BzRouteAttribute.Group"/> — ready to render as a menu.</summary>
    public IEnumerable<IGrouping<string, BzRouteEntry>> ByGroup(bool fa = true)
        => _entries
            .Where(e => !e.Hidden && e.IsStatic && e.Meta is not null)
            .GroupBy(e => e.Group ?? string.Empty);

    /// <summary>Every route that should appear in a sitemap.</summary>
    public IEnumerable<BzRouteEntry> Sitemap()
        => _entries.Where(e => e.IsStatic && !e.Hidden && !(e.Meta?.NoIndex ?? false));

    /// <summary>
    /// Walks the <see cref="BzRouteAttribute.Parent"/> chain to build a breadcrumb trail,
    /// substituting route values into any parameterised parent templates.
    /// </summary>
    public IReadOnlyList<BzBreadcrumbStep> BuildBreadcrumbs(
        Type pageType,
        IReadOnlyDictionary<string, object?> routeValues,
        string currentUri,
        bool fa)
    {
        var chain = new List<BzRouteEntry>();
        var entry = ByPage(pageType);
        var guard = 0;

        while (entry is not null && guard++ < 16)
        {
            chain.Add(entry);
            var parent = entry.Meta?.Parent;
            if (string.IsNullOrWhiteSpace(parent)) break;
            var next = ByTemplate(parent!);
            if (next is null || chain.Contains(next)) break;   // unknown or cyclic
            entry = next;
        }

        chain.Reverse();

        var steps = new List<BzBreadcrumbStep>(chain.Count);
        for (var i = 0; i < chain.Count; i++)
        {
            var e = chain[i];
            var last = i == chain.Count - 1;
            var href = last ? null : Substitute(e.Template, routeValues);
            steps.Add(new BzBreadcrumbStep(e.Breadcrumb(fa) ?? e.PageType.Name, href, e.Icon));
        }
        return steps;
    }

    /// <summary>Replaces <c>{Name}</c> / <c>{Name:int}</c> placeholders with actual route values.</summary>
    private static string Substitute(string template, IReadOnlyDictionary<string, object?> values)
    {
        if (!template.Contains('{')) return template;

        var result = template;
        foreach (var kv in values)
        {
            foreach (var form in new[] { $"{{{kv.Key}}}", $"{{*{kv.Key}}}" })
                result = result.Replace(form, kv.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // constrained forms such as {Id:int} or {Slug:minlength(3)}
            var open = result.IndexOf('{' + kv.Key + ':', StringComparison.OrdinalIgnoreCase);
            if (open >= 0)
            {
                var close = result.IndexOf('}', open);
                if (close > open)
                    result = result.Remove(open, close - open + 1)
                                   .Insert(open, kv.Value?.ToString() ?? "");
            }
        }
        return result;
    }
}
