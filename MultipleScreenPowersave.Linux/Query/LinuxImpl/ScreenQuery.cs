namespace MultipleScreenPowersave.Query.LinuxImpl;

using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using static MultipleScreenPowersave.ProcessHelper;

/// <summary>
/// Linux implementation of <see cref="IScreenQuery"/>.
/// </summary>
public class ScreenQuery : IScreenQuery
{
    private int nextDisplayMonitorHandle = 1;
    private int nextPhysicalMonitorHandle = 1;

    /// <summary>
    /// Determines screens available.
    /// </summary>
    /// <returns>ScreenInformation.</returns>
    public ScreenInformation GetScreenInformation()
    {
        var physicalMonitors = this.GetPhysicalMonitors();
        var displayMonitors = this.GetVirtualMonitors(physicalMonitors);

        return new ScreenInformation(displayMonitors);
    }

    private static string NormalizeHex(string? hex)
    {
        return hex?.Replace(" ", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .ToLower() ?? string.Empty;
    }

    private static Dictionary<string, string> ParseBacklightDeviceOutput(string output)
    {
        var lines = output.Split('\n');
        var backlightDeviceByI2cDevice = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            var match = Regex.Match(
                trimmed,
                @"^\/sys\/class\/backlight\/(?<backlightDevice>[\w-]+)\/device\/ddc\/i2c-dev\/(?<i2cDevice>[\w-]+)"
            );
            if (match.Success)
            {
                backlightDeviceByI2cDevice.Add(
                    match.Groups["i2cDevice"].Value,
                    match.Groups["backlightDevice"].Value
                );
            }
        }

        return backlightDeviceByI2cDevice;
    }

    private List<PhysicalMonitorInformation> GetPhysicalMonitors()
    {
        var (command, arguments) = ("ddcutil", "detect --edid-read-size=128 --verbose");
        var exitCode = RunProcess(
            command,
            arguments,
            out var standardOutput,
            out var standardError
        );
        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        var monitors = this.ParseDdcUtilOutput(standardOutput);

        (command, arguments) = (
            "sh",
            "-c \"for dev in \"/sys/class/backlight/*/device/ddc/i2c-dev/*\"; do echo $dev; done\""
        );
        exitCode = RunProcess(command, arguments, out standardOutput, out standardError);
        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        var backlightDeviceByI2cDevice = ParseBacklightDeviceOutput(standardOutput);

        foreach (
            var (i2cDevice, monitor) in monitors.ToDictionary(keySelector: it => it.LinuxI2cDevice!)
        )
        {
            if (!backlightDeviceByI2cDevice.TryGetValue(i2cDevice, out var backlightDevice))
                continue;

            monitor.LinuxBacklightDevice = backlightDevice;
        }

        return monitors;
    }

    private List<DisplayMonitorInformation> GetVirtualMonitors(
        IEnumerable<PhysicalMonitorInformation> physicalMonitors
    )
    {
        var (command, arguments) = ("xrandr", "--verbose");
        var exitCode = RunProcess(
            command,
            arguments,
            out var standardOutput,
            out var standardError
        );
        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        var monitors = this.ParseXrandrOutput(standardOutput, physicalMonitors);

        return monitors;
    }

    private List<PhysicalMonitorInformation> ParseDdcUtilOutput(string output)
    {
        uint physicalMonitorIndex = 0;
        var monitors = new List<PhysicalMonitorInformation>();

        var lines = output.Split('\n');
        PhysicalMonitorInformation? current = null;
        bool isCollectingEdid = false;
        string edid = string.Empty;
        int edidMaxLength = 128; // Limit to 128 bytes for consistency with ddcutil

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // New display section or invalid display (turned off monitors)
            if (trimmed.StartsWith("Display ") || trimmed.StartsWith("Invalid display"))
            {
                // Save previous monitor before starting new one
                if (current != null)
                {
                    current.EdidHex = string.IsNullOrWhiteSpace(edid) ? null : NormalizeHex(edid);
                    monitors.Add(current);
                }

                current = new PhysicalMonitorInformation(
                    new PhysicalMonitorHandle(this.nextPhysicalMonitorHandle++),
                    physicalMonitorIndex++
                );

                isCollectingEdid = false;
            }

            // I2C bus line (parse bus number)
            if (trimmed.StartsWith("I2C bus:") && current != null)
                current.LinuxI2cDevice = trimmed.Split(':')[1].Split("/dev/")[1].Trim();

            // Mfg id line
            if (trimmed.StartsWith("Mfg id:") && current != null)
                current.Description = trimmed.Split(':')[1].Trim();

            // Model line
            if (trimmed.StartsWith("Model:") && current != null)
            {
                var model = trimmed.Split(':')[1].Trim();

                if (string.IsNullOrWhiteSpace(current.Description))
                    current.Description = model;
                else
                    current.Description += " " + model;
            }

            // EDID block starts
            if (trimmed.StartsWith("EDID hex dump:"))
            {
                isCollectingEdid = true;
                edid = string.Empty;
                continue;
            }

            // Collect EDID hex bytes
            if (isCollectingEdid && trimmed.StartsWith('+'))
            {
                var parts = trimmed.Split("   ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var hexPart = parts[1].Trim().Replace(" ", string.Empty);
                    if (Regex.IsMatch(hexPart, @"^[0-9a-fA-F]+$"))
                        edid += hexPart;
                }

                // Stop collecting after 128 bytes
                if (edid.Length >= edidMaxLength)
                    isCollectingEdid = false;
            }
            else if (isCollectingEdid && !trimmed.StartsWith('+'))
            {
                // Stop collection if no more EDID lines are found
                isCollectingEdid = false;
            }
        }

        // Add the last monitor
        if (current != null)
        {
            current.EdidHex = string.IsNullOrWhiteSpace(edid) ? null : NormalizeHex(edid);
            monitors.Add(current);
        }

        return monitors;
    }

    private List<DisplayMonitorInformation> ParseXrandrOutput(
        string output,
        IEnumerable<PhysicalMonitorInformation> physicalMonitors
    )
    {
        var monitors = new List<DisplayMonitorInformation>();

        var lines = output.Split('\n');
        DisplayMonitorInformation? current = null;

        bool isCollectingEdid = false;
        string edid = string.Empty;
        int edidMaxLength = 128; // Limit to 128 bytes for consistency with ddcutil

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // New monitor section
            if (trimmed.EndsWith("connected") || trimmed.Contains(" connected "))
            {
                isCollectingEdid = false;

                if (current != null)
                    monitors.Add(current);

                var isPrimary = trimmed.Contains(" primary ");

                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var x11OutputName = parts[0];
                Rect monitorRectangle = new(x: 0, y: 0, width: 0, height: 0);

                // Resolution and position lines
                if (Regex.IsMatch(trimmed, @"(\d+)x(\d+)\+(\d+)\+(\d+)"))
                {
                    var match = Regex.Match(trimmed, @"(\d+)x(\d+)\+(\d+)\+(\d+)");
                    if (match.Success)
                    {
                        monitorRectangle.X = int.Parse(match.Groups[3].Value);
                        monitorRectangle.Y = int.Parse(match.Groups[4].Value);
                        monitorRectangle.Width = int.Parse(match.Groups[1].Value);
                        monitorRectangle.Height = int.Parse(match.Groups[2].Value);
                    }
                }

                current = new DisplayMonitorInformation(
                    new DisplayMonitorHandle(this.nextDisplayMonitorHandle++),
                    isPrimary: isPrimary,
                    monitorRectangle
                );
            }

            // Start EDID block
            if (trimmed == "EDID:")
            {
                isCollectingEdid = true;
                edid = string.Empty;
                continue;
            }

            // Collect EDID lines
            if (isCollectingEdid)
            {
                if (Regex.IsMatch(trimmed, @"^([0-9a-fA-F]{2}\s*){8,16}$"))
                {
                    edid += trimmed.Replace(" ", string.Empty);

                    // Stop collecting after 128 bytes
                    if (edid.Length >= edidMaxLength)
                    {
                        isCollectingEdid = false;
                        if (current != null)
                            current.LinuxEdidHex = NormalizeHex(edid);
                    }
                }
                else
                {
                    isCollectingEdid = false;
                    if (current != null)
                        current.LinuxEdidHex = NormalizeHex(edid);
                }
            }
        }

        // Final monitor
        if (current != null)
        {
            if (!string.IsNullOrWhiteSpace(edid))
                current.LinuxEdidHex = NormalizeHex(edid);
            monitors.Add(current);
        }

        // Join physical monitors
        foreach (var monitor in monitors)
        {
            monitor.PhysicalMonitors.AddRange(
                physicalMonitors.Where(physicalMonitor =>
                    physicalMonitor.EdidHex == monitor.LinuxEdidHex
                )
            );
        }

        return monitors;
    }
}
