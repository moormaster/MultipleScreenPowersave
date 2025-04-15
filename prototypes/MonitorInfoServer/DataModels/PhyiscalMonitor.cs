namespace DataModels;

public record PhysicalMonitor
{
    public string Name { get; set; }
    public string I2CBus { get; set; }
    public string BacklightDevice { get; set; }
    public string EDID { get; set; }
    public List<VirtualMonitor> VirtualMonitors { get; set; } = new();
}