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
    public static IServiceCollection AddMultipleScreenPowerSaveLinuxPlatformServices(
        this IServiceCollection services
    )
    {
        services
            .AddTransient(
                typeof(IDisplayBacklightService),
                typeof(App.LinuxImpl.DisplayBacklightService)
            )
            .AddTransient(
                typeof(IDisplayDataChannelService),
                typeof(App.LinuxImpl.DisplayDataChannelService)
            )
            .AddTransient(typeof(IMouseQuery), typeof(Query.LinuxImpl.MouseQuery))
            .AddTransient(typeof(IScreenQuery), typeof(Query.LinuxImpl.ScreenQuery))
            .AddTransient(typeof(IWindowQuery), typeof(Query.LinuxImpl.WindowQuery));

        return services;
    }
}
