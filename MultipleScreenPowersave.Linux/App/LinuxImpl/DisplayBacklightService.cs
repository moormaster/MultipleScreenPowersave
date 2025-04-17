namespace MultipleScreenPowersave.App.LinuxImpl;

using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model;
using static MultipleScreenPowersave.ProcessHelper;

/// <summary>
/// Linux implementation of <see cref="IDisplayBacklightService"/>.
/// </summary>
public class DisplayBacklightService : IDisplayBacklightService
{
    /// <inheritdoc/>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        Guard.IsNotNullOrWhiteSpace(physicalMonitor.LinuxBacklightDevice);

        TurnOffMonitorBacklight(physicalMonitor);
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        Guard.IsNotNullOrWhiteSpace(monitor.LinuxBacklightDevice);

        TurnOnMonitorBacklight(monitor);
    }

    private static string GetBacklightSysFsDevice(string backlightDevice)
    {
        return $"sysfs/backlight/{backlightDevice}";
    }

    private static double GetMonitorBacklightPercent(PhysicalMonitorInformation monitor)
    {
        var backlightSysFsDevice = GetBacklightSysFsDevice(monitor.LinuxBacklightDevice!);
        var (command, arguments) = ("light", $"-v 1 -s {backlightSysFsDevice} -G");
        var exitCode = RunProcess(
            command,
            arguments,
            out var standardOutput,
            out var standardError
        );

        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        return double.Parse(standardOutput);
    }

    private static void RestoreMonitorBacklight(PhysicalMonitorInformation monitor)
    {
        var backlightSysFsDevice = GetBacklightSysFsDevice(monitor.LinuxBacklightDevice!);
        var (command, arguments) = ("light", $"-v 1 -s {backlightSysFsDevice} -I");
        var exitCode = RunProcess(command, arguments, out var _, out var standardError);

        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);
    }

    private static void SaveMonitorBacklight(PhysicalMonitorInformation monitor)
    {
        var backlightSysFsDevice = GetBacklightSysFsDevice(monitor.LinuxBacklightDevice!);
        var (command, arguments) = ("light", $"-v 1 -s {backlightSysFsDevice} -O");
        var exitCode = RunProcess(command, arguments, out var _, out var standardError);

        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);
    }

    private static void SetMonitorBacklightToZero(PhysicalMonitorInformation monitor)
    {
        var backlightSysFsDevice = GetBacklightSysFsDevice(monitor.LinuxBacklightDevice!);
        var (command, arguments) = ("light", $"-v 1 -s {backlightSysFsDevice} -S 0");
        var exitCode = RunProcess(command, arguments, out var _, out var standardError);

        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);
    }

    private static void TurnOffMonitorBacklight(PhysicalMonitorInformation monitor)
    {
        if (GetMonitorBacklightPercent(monitor) > double.Epsilon)
        {
            SaveMonitorBacklight(monitor);
            SetMonitorBacklightToZero(monitor);
        }
    }

    private static void TurnOnMonitorBacklight(PhysicalMonitorInformation monitor)
    {
        if (GetMonitorBacklightPercent(monitor) < double.Epsilon)
            RestoreMonitorBacklight(monitor);
    }
}
