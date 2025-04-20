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
    /// Configure platform specific services and options for MultipleScreenpowersave application.
    /// </summary>
    /// <param name="services">Servicecollection.</param>
    public static void AddMultipleScreenPowerSaveWindowsPlatformServices(
        this IServiceCollection services
    )
    {
        services.AddTransient(
            typeof(IDisplayBacklightService),
            typeof(App.WindowsImpl.DisplayBacklightService)
        );
        services.AddTransient(
            typeof(IDisplayDataChannelService),
            typeof(App.WindowsImpl.DisplayDataChannelService)
        );
        services.AddTransient(typeof(IMouseQuery), typeof(Query.WindowsImpl.MouseQuery));
        services.AddTransient(typeof(IScreenQuery), typeof(Query.WindowsImpl.ScreenQuery));
        services.AddTransient(typeof(IWindowQuery), typeof(Query.WindowsImpl.WindowQuery));
    }
}
