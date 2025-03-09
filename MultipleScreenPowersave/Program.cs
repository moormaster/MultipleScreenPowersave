namespace MultipleScreenPowersave;

using System.Threading;
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

        var sleepTimeMs = 1000;
        var applicationService = new ApplicationService();
        var stop = false;

        // catch SIGTERM
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
        {
            stop = true;
            applicationService.TurnOnAllMonitors();
        };

        // catch SIGINT
        Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
        {
            stop = true;
            e.Cancel = true;
        };

        while (!stop)
        {
            applicationService.TurnOnOnlyUsedMonitors();
            Thread.Sleep(sleepTimeMs);
        }

        applicationService.TurnOnAllMonitors();
    }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings(filePath: ConfigFilePath.AppConfig)
            .ReadFrom.AppSettings(settingPrefix: "serilogDebug", filePath: ConfigFilePath.AppConfig)
            .CreateLogger();
    }
}
