namespace MultipleScreenPowersave;

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
    /// Main entry method.
    /// </summary>
    public static void Main()
    {
        SetupSerilog();

        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder
            .Services.AddMultipleScreenPowerSaveBackgroundService()
            .AddMultipleScreenPowerSaveWindowsPlatformServices()
            .AddSerilog(
                (services, configuration) =>
                {
                    configuration.ReadFrom.Configuration(hostBuilder.Configuration);
                    configuration.ReadFrom.Services(services);
                }
            );

        var host = hostBuilder.Build();

        // catch SIGTERM
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
        {
            var hostedBackgroundService = (HostedBackgroundService)
                host.Services.GetService<IHostedService>()!;

            hostedBackgroundService.StopAsync(cancellationToken: default);
        };

        host.Run();
    }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration().CreateLogger();
    }
}
