namespace MultipleScreenPowersave;

using Microsoft.Extensions.Hosting;
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
