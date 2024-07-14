namespace MultipleScreenPowersave;

using System.Threading;
using MultipleScreenPowersave.App;

public static partial class Program
{
    public static void Main()
    {
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
}