namespace MultipleScreenPowersave;

using System.Threading;
using Microsoft.Extensions.Options;
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

        var options = new HostedBackgroundServiceOptions() { SleepTimeMs = 1000 };
        var applicationService = new ApplicationService(
            new App.WindowsImpl.DisplayDataChannelService(),
            new Query.WindowsImpl.MouseQuery(),
            new Query.WindowsImpl.ScreenQuery(),
            new Query.WindowsImpl.WindowQuery()
        );
        var hostedBackgroundService = new HostedBackgroundService(
            applicationService,
            new OptionsWrapper<HostedBackgroundServiceOptions>(options)
        );

        CancellationTokenSource tokenSource = new();
        var task = hostedBackgroundService.StartAsync(tokenSource.Token);

        // catch SIGTERM
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
        {
            tokenSource.Cancel();
            task.Wait();

            applicationService.TurnOnAllMonitors();
        };

        // catch SIGINT
        Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
        {
            e.Cancel = true;
        };
    }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings(filePath: ConfigFilePath.AppConfig)
            .ReadFrom.AppSettings(settingPrefix: "serilogDebug", filePath: ConfigFilePath.AppConfig)
            .CreateLogger();
    }
}
