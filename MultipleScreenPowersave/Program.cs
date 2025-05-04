namespace MultipleScreenPowersave;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultipleScreenPowersave.App;
using Serilog;

/// <summary>
/// Main class.
/// </summary>
public static class Program
{
    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    /// <returns>The AppBuilder.</returns>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<AvaloniaApp>().UsePlatformDetect().WithInterFont().LogToTrace();
    }

    /// <summary>
    /// Main entry method.
    /// </summary>
    public static void Main()
    {
        SetupSerilog();
        AppBuilder avaloniaAppBuilder = BuildAvaloniaApp();

        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder
            .Services.AddMultipleScreenPowerSaveBackgroundService()
#if WINDOWS
            .AddMultipleScreenPowerSaveWindowsPlatformServices()
#else
            .AddMultipleScreenPowerSaveLinuxPlatformServices()
#endif
            .AddSerilog(
                (services, configuration) =>
                {
                    configuration.ReadFrom.Configuration(hostBuilder.Configuration);
                    configuration.ReadFrom.Services(services);
                }
            );

        var host = hostBuilder.Build();

        // catch SIGTERM
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs ev) =>
        {
            try
            {
                var hostedBackgroundService = (HostedBackgroundService)
                    host.Services.GetService<IHostedService>()!;

                hostedBackgroundService.StopAsync(cancellationToken: default);
            }
            catch (ObjectDisposedException)
            {
                // ignore exception when host has already been stopped
            }
        };

        Task? hostTask = null;
        IControlledApplicationLifetime? avaloniaApplicationLifetime = null;

        avaloniaAppBuilder
            .AfterPlatformServicesSetup(appBuilder =>
            {
                hostTask = host.RunAsync();

                Task.Run(async () =>
                {
                    await hostTask;
                    avaloniaApplicationLifetime?.Shutdown();
                });
            })
            .StartWithClassicDesktopLifetime(
                [],
                lifeTime =>
                {
                    lifeTime.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

                    avaloniaApplicationLifetime = lifeTime;
                }
            );
    }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration().CreateLogger();
    }
}
