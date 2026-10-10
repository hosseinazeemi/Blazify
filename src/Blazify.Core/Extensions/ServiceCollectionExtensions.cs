using Microsoft.Extensions.DependencyInjection;
using BzCore.Dialog;
using BzCore.Notifications;
using BzCore.Theming;

namespace BzCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazify(this IServiceCollection services)
        => services.AddBlazify(_ => { });

    public static IServiceCollection AddBlazify(this IServiceCollection services, Action<BzOptions> configure)
    {
        var options = new BzOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddScoped<IBzThemeService, BzThemeService>();
        services.AddScoped<IBzDialogService, BzDialogService>();
        services.AddScoped<IBzNotificationService, BzNotificationService>();
        services.AddScoped<BzCore.Localization.IBzLocalizer, BzCore.Localization.BzLocalizer>();

        return services;
    }
}
