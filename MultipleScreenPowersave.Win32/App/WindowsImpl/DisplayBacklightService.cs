namespace MultipleScreenPowersave.App.WindowsImpl;

using System.Management;
using MultipleScreenPowersave.Model;

/// <summary>
/// Implementation of <see cref="IDisplayBacklightService"/>.
/// </summary>
public class DisplayBacklightService : IDisplayBacklightService
{
    private const uint TimeoutInSeconds = 1;

    private readonly Dictionary<string, byte> previousBrightnessInPercentByWmiInstance = [];

    /// <inheritdoc/>
    public void TurnOffMonitor(PhysicalMonitorInformation monitor)
    {
        var previousBrightnessInPercent = WmiGetCurrentBrightness(monitor.WmiInstanceName!);

        if (previousBrightnessInPercent > 0)
        {
            this.previousBrightnessInPercentByWmiInstance[monitor.WmiInstanceName!] =
                previousBrightnessInPercent;
            WmiSetBrightness(monitor.WmiInstanceName!, TimeoutInSeconds, brightnessInPercent: 0);
        }
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        if (
            !this.previousBrightnessInPercentByWmiInstance.TryGetValue(
                monitor.WmiInstanceName!,
                out var previousBrightnessInPercent
            )
        )
            previousBrightnessInPercent = 100;

        var currentBrightness = WmiGetCurrentBrightness(monitor.WmiInstanceName!);
        if (currentBrightness == 0)
        {
            WmiSetBrightness(
                monitor.WmiInstanceName!,
                TimeoutInSeconds,
                previousBrightnessInPercent
            );
        }
    }

    private static byte WmiGetCurrentBrightness(string wmiInstanceName)
    {
        // see https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmimonitorbrightness
        ManagementScope scope = new(@"root\WMI");
        ObjectQuery query = new("SELECT * FROM WmiMonitorBrightness");
        ManagementObjectSearcher searcher = new(scope, query);

        var wmiObject = searcher
            .Get()
            .OfType<ManagementObject>()
            .First(wmiObject =>
                wmiObject.Properties["InstanceName"].Value.ToString() == wmiInstanceName
            );

        return (byte)wmiObject.Properties["CurrentBrightness"].Value;
    }

    private static void WmiSetBrightness(
        string wmiInstanceName,
        uint timeoutInSeconds,
        byte brightnessInPercent
    )
    {
        // see https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmimonitorbrightnessmethods
        ManagementScope scope = new(@"root\WMI");
        ObjectQuery query = new("SELECT * FROM WmiMonitorBrightnessMethods");
        ManagementObjectSearcher searcher = new(scope, query);

        var wmiObject = searcher
            .Get()
            .OfType<ManagementObject>()
            .First(wmiObject =>
                wmiObject.Properties["InstanceName"].Value.ToString() == wmiInstanceName
            );

        // see https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmisetbrightness-method-in-class-wmimonitorbrightnessmethods
        var inParameters = wmiObject.GetMethodParameters("WmiSetBrightness");
        inParameters["Brightness"] = brightnessInPercent;
        inParameters["Timeout"] = timeoutInSeconds;

        wmiObject.InvokeMethod("WmiSetBrightness", inParameters, new());
    }
}
