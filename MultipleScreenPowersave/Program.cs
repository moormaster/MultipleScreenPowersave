namespace MultipleScreenPowersave;

using System.Threading;
using MultipleScreenPowersave.App;

public static partial class Program
{
    public static void Main()
    {
        var applicationService = new ApplicationService();

        for (var i = 0; i < 30; i++)
        {
            applicationService.TurnOnOnlyUsedMonitors();

            Thread.Sleep(1000);
        }

        applicationService.TurnOnAllMonitors();
    }
}