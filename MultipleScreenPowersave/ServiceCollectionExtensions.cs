namespace MultipleScreenPowersave;

using Microsoft.Extensions.DependencyInjection;
using MultipleScreenPowersave.App;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Query;

/// <summary>
/// Provides extension methods for configuring services for HostApplicationBuilder.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure services and options for MultipleScreenpowersave application.
    /// This method does not register platform specific services like <see cref="IMouseQuery"/>, <see cref="IScreenQuery"/> or <see cref="IWindowQuery"/>.
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <returns>The ServiceCollection.</returns>
    public static IServiceCollection AddMultipleScreenPowerSaveBackgroundService(
        this IServiceCollection services
    )
    {
        services
            .AddOptions<HostedBackgroundServiceOptions>()
            .BindConfiguration("hostedBackgroundService");
        services.AddOptions<BlacklistOptions>().BindConfiguration("blacklist");
        services.AddOptions<BlackWindowServiceOptions>().BindConfiguration("blackWindowService");

        services
            .AddTransient(typeof(IApplicationService), typeof(ApplicationService))
            .AddTransient(typeof(IBlackWindowService), typeof(BlackWindowService))
            .AddTransient(typeof(IDisplayControlServiceFacade), typeof(DisplayControlServiceFacade))
            .AddHostedService<HostedBackgroundService>();

        return services;
    }
}
