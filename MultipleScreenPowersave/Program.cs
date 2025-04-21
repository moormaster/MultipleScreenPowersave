namespace MultipleScreenPowersave;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultipleScreenPowersave.App;
using MultipleScreenPowersave.Configuration;
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
        hostBuilder.Services.AddMultipleScreenPowerSaveBackgroundService();
        hostBuilder.Services.AddMultipleScreenPowerSaveWindowsPlatformServices();

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
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings(filePath: ConfigFilePath.AppConfig)
            .ReadFrom.AppSettings(settingPrefix: "serilogDebug", filePath: ConfigFilePath.AppConfig)
            .CreateLogger();
    }
}
