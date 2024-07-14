namespace MultipleScreenPowersave.Configuration;

using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

public class PhysicalMonitorBlacklistEntry
{
    public PhysicalMonitorBlacklistEntry(Regex? description)
    {
        if (description == null)
            throw new ArgumentNullException(nameof(description), "At least one property must be != null");

        this.Description = description;
    }

    public Regex? Description { get; }

    public bool IsMatch(DisplayMonitorInformation displayMonitor)
    {
        if (this.Description != null)
        {
            if (!displayMonitor.PhysicalMonitors.Any(
                physicalMonitor =>
                {
                    if (!this.Description.IsMatch(physicalMonitor.Description))
                        return false;

                    return true;
                }))
                return false;
        }

        return true;
    }
}
