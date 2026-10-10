using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BzCore.Routing;

/// <summary>Wires up the Blazify routing services.</summary>
public static class BzRoutingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="BzRouteTable"/> and every <see cref="IBzRouteGuard"/> /
    /// <see cref="IBzRouteLoader"/> found in <paramref name="assemblies"/>.
    /// </summary>
    /// <remarks>
    /// Guards and loaders are registered as scoped. Auto-registration is a convenience:
    /// register a type yourself first and that registration is kept.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddBlazify();
    /// builder.Services.AddBlazifyRouting(typeof(Program).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddBlazifyRouting(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        // Singleton: the route table is derived from assembly metadata, which cannot
        // change while the process runs.
        services.TryAddSingleton<BzRouteTable>();

        foreach (var asm in assemblies)
        {
            if (asm is null) continue;

            IEnumerable<Type> types;
            try { types = asm.GetTypes(); }
            catch (System.Reflection.ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null)!; }
            catch { continue; }

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;

                if (typeof(IBzRouteGuard).IsAssignableFrom(type))
                    services.TryAddScoped(type);

                if (typeof(IBzRouteLoader).IsAssignableFrom(type))
                    services.TryAddScoped(type);
            }
        }

        return services;
    }
}
