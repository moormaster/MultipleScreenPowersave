namespace MultipleScreenPowersave.App.LinuxImpl;

using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.VCP;
using static MultipleScreenPowersave.ProcessHelper;

/// <summary>
/// Linux implementation of <see cref="IDisplayDataChannelService"/>.
/// </summary>
public class DisplayDataChannelService : IDisplayDataChannelService
{
    /// <inheritdoc/>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        Guard.IsNotNullOrWhiteSpace(physicalMonitor.LinuxI2cDevice);

        TurnOffMonitorDdc(physicalMonitor);
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        Guard.IsNotNullOrWhiteSpace(monitor.LinuxI2cDevice);

        TurnOnMonitorDdc(monitor);
    }

    private static int GetI2cBusNumber(string i2cBusDevice)
    {
        var match = Regex.Match(i2cBusDevice, @"i2c-(\d+)");
        return int.Parse(match.Groups[1].Value);
    }

    private static uint GetMonitorPowerMode(PhysicalMonitorInformation monitor, int i2cBusNumber)
    {
        var (command, arguments) = (
            "ddcutil",
            $"--bus {i2cBusNumber} getvcp {FeatureConstants.PowerMode:x}"
        );
        var exitCode = RunProcess(
            command,
            arguments,
            out var standardOutput,
            out var standardError
        );
        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        uint powerMode = ParseDdcUtilPowerModeOutput(standardOutput);

        if (powerMode == 0u)
        {
            throw new InvalidOperationException(
                $"Monitor #{monitor.Handle.Value} does not support PowerMode DDC feature"
            );
        }

        return powerMode;
    }

    private static uint ParseDdcUtilPowerModeOutput(string standardOutput)
    {
        var match = Regex.Match(
            standardOutput,
            @"VCP code 0xd6 \([^)]*\):[^)]*\(s1=0x(?<powerModeValue>[0-9a-fA-F]*)\)"
        );
        return uint.Parse(match.Groups["powerModeValue"].Value, NumberStyles.HexNumber);
    }

    private static void TurnOffMonitorDdc(PhysicalMonitorInformation monitor)
    {
        var i2cBusNumber = GetI2cBusNumber(monitor.LinuxI2cDevice!);
        var currentPowerModeValue = GetMonitorPowerMode(monitor, i2cBusNumber);
        if (currentPowerModeValue == PowerModeValueConstants.DpmsOff)
        {
            // monitor is already turned off
            return;
        }

        var (command, arguments) = (
            "ddcutil",
            $"--bus {i2cBusNumber} setvcp {FeatureConstants.PowerMode:x} {PowerModeValueConstants.DpmsOff:x}"
        );
        var exitCode = RunProcess(command, arguments, out var _, out var standardError);

        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);
    }

    private static void TurnOnMonitorDdc(PhysicalMonitorInformation monitor)
    {
        var i2cBusNumber = GetI2cBusNumber(monitor.LinuxI2cDevice!);
        var currentPowerModeValue = GetMonitorPowerMode(monitor, i2cBusNumber);
        if (currentPowerModeValue == PowerModeValueConstants.DpmOn)
        {
            // monitor is already turned on
            return;
        }

        var (command, arguments) = (
            "ddcutil",
            $"--bus {i2cBusNumber} setvcp {FeatureConstants.PowerMode:x} {PowerModeValueConstants.DpmOn:x}"
        );
        var exitCode = RunProcess(command, arguments, out var _, out var standardError);

        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);
    }
}
