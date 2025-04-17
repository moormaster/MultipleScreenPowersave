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
    /// <param name="services">Servicecollection.</param>
    public static void AddMultipleScreenPowerSaveBackgroundService(this IServiceCollection services)
    {
        services
            .AddOptions<HostedBackgroundServiceOptions>()
            .BindConfiguration("hostedBackgroundService");
        services.AddOptions<BlacklistOptions>().BindConfiguration("blacklist");

        services.AddTransient(typeof(IApplicationService), typeof(ApplicationService));

        services.AddHostedService<HostedBackgroundService>();
    }
}
