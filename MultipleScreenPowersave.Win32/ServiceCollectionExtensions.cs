namespace MultipleScreenPowersave;

using Microsoft.Extensions.DependencyInjection;
using MultipleScreenPowersave.App;
using MultipleScreenPowersave.Query;

/// <summary>
/// Provides extension methods for configuring services for HostApplicationBuilder.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure platform specific services and options for MultipleScreenpowersave application.
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <returns>The ServiceCollection.</returns>
    public static IServiceCollection AddMultipleScreenPowerSaveWindowsPlatformServices(
        this IServiceCollection services
    )
    {
        services
            .AddTransient(
                typeof(IDisplayBacklightService),
                typeof(App.WindowsImpl.DisplayBacklightService)
            )
            .AddTransient(
                typeof(IDisplayDataChannelService),
                typeof(App.WindowsImpl.DisplayDataChannelService)
            )
            .AddTransient(typeof(IMouseQuery), typeof(Query.WindowsImpl.MouseQuery))
            .AddTransient(typeof(IScreenQuery), typeof(Query.WindowsImpl.ScreenQuery))
            .AddTransient(typeof(IWindowQuery), typeof(Query.WindowsImpl.WindowQuery));

        return services;
    }
}
