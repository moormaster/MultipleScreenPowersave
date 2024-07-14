namespace MultipleScreenPowersave.Configuration;

using MultipleScreenPowersave.Model;

public record class DisplayMonitorBlacklistEntry
{
    public DisplayMonitorBlacklistEntry(bool? isPrimary, IList<PhysicalMonitorBlacklistEntry>? physicalMonitors)
    {
        if (isPrimary == null
            && physicalMonitors == null)
            throw new ArgumentNullException(nameof(isPrimary), "At least one property must be != null");

        this.IsPrimary = isPrimary;

        foreach (var monitor in physicalMonitors ?? [])
            this.PhysicalMonitors.Add(monitor);
    }

    public bool? IsPrimary { get; }

    public IList<PhysicalMonitorBlacklistEntry> PhysicalMonitors { get; } = [];

    public bool IsMatch(DisplayMonitorInformation displayMonitor)
    {
        if (this.IsPrimary != null)
        {
            if (this.IsPrimary != displayMonitor.IsPrimary)
                return false;
        }

        if (this.PhysicalMonitors.Any() && !this.PhysicalMonitors.Any(
            physicalMonitorEntry => physicalMonitorEntry.IsMatch(displayMonitor)))
            return false;

        return true;
    }
}
