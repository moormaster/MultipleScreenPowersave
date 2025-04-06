using DataModels;
using System.Diagnostics;
using System.Text.RegularExpressions;

public static class Helpers {
    public static List<PhysicalMonitor> GetPhysicalMonitors()
    {
        var output = RunProcess("ddcutil", "detect --edid-read-size=128 --verbose");
        var monitors = new List<PhysicalMonitor>();

        var lines = output.Split('\n');
        PhysicalMonitor? current = null;
        bool isCollectingEdid = false;
        string edid = "";
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
                    current.EDID = string.IsNullOrWhiteSpace(edid) ? null : NormalizeHex(edid);
                    monitors.Add(current);
                }

                current = new PhysicalMonitor();
                edid = "";
                isCollectingEdid = false;
            }

            // I2C bus line (parse bus number)
            if (trimmed.StartsWith("I2C bus:") && current != null)
            {
                current.I2CBus = trimmed.Split(':')[1].Trim();
            }

            // EDID block starts
            if (trimmed.StartsWith("EDID hex dump:"))
            {
                isCollectingEdid = true;
                continue;
            }

            // Collect EDID hex bytes
            if (isCollectingEdid && trimmed.StartsWith("+"))
            {
                var parts = trimmed.Split("   ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var hexPart = parts[1].Trim().Replace(" ", "");
                    if (Regex.IsMatch(hexPart, @"^[0-9a-fA-F]+$"))
                    {
                        edid += hexPart;
                    }
                }

                // Stop collecting after 128 bytes
                if (edid.Length >= edidMaxLength)
                {
                    isCollectingEdid = false;
                }
            }
            else if (isCollectingEdid && !trimmed.StartsWith("+"))
            {
                // Stop collection if no more EDID lines are found
                isCollectingEdid = false;
            }
        }

        // Add the last monitor
        if (current != null)
        {
            current.EDID = string.IsNullOrWhiteSpace(edid) ? null : NormalizeHex(edid);
            monitors.Add(current);
        }

        return monitors;
    }

    public static List<VirtualMonitor> GetVirtualMonitors()
    {
        var output = RunProcess("xrandr", "--verbose");
        var monitors = new List<VirtualMonitor>();

        var lines = output.Split('\n');
        VirtualMonitor? current = null;
        
        bool isCollectingEdid = false;
        string? edid = null;
        int edidMaxLength = 128; // Limit to 128 bytes for consistency with ddcutil

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // New monitor section
            if (trimmed.EndsWith("connected") || trimmed.Contains(" connected "))
            {
                if (current != null) monitors.Add(current);
                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                current = new VirtualMonitor
                {
                    OutputName = parts[0]
                };
                edid = "";
                isCollectingEdid = false;

                // Resolution and position lines
                if (current != null && Regex.IsMatch(trimmed, @"^\d{3,4}x\d{3,4}[\s+@]"))
                {
                    var match = Regex.Match(trimmed, @"^(\d{3,4}x\d{3,4})");
                    if (match.Success)
                    {
                        current.Resolution = match.Groups[1].Value;
                    }
                }
                if (current != null && Regex.IsMatch(trimmed, @"\d+x\d+\+\d+\+\d+"))
                {
                    var match = Regex.Match(trimmed, @"(\d+x\d+)\+(\d+)\+(\d+)");
                    if (match.Success)
                    {
                        current.Resolution = match.Groups[1].Value;
                        current.Position = $"+{match.Groups[2].Value}+{match.Groups[3].Value}";
                    }
                }
            }

            // Start EDID block
            if (trimmed == "EDID:")
            {
                isCollectingEdid = true;
                edid = "";
                continue;
            }

            // Collect EDID lines
            if (isCollectingEdid)
            {
                if (Regex.IsMatch(trimmed, @"^([0-9a-fA-F]{2}\s*){8,16}$"))
                {
                    edid += trimmed.Replace(" ", "");

                    // Stop collecting after 128 bytes
                    if (edid.Length >= edidMaxLength)
                    {
                        isCollectingEdid = false;
                    }
                }
                else
                {
                    isCollectingEdid = false;
                    if (current != null) current.EDID = NormalizeHex(edid);
                }
            }
        }

        // Final monitor
        if (current != null)
        {
            if (!string.IsNullOrWhiteSpace(edid))
                current.EDID = NormalizeHex(edid);
            monitors.Add(current);
        }

        return monitors;
    }

    public static string NormalizeHex(string? hex)
    {
        return hex?.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToLower() ?? "";
    }

    public static string RunProcess(string command, string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        return proc?.StandardOutput.ReadToEnd() ?? "";
    }
}