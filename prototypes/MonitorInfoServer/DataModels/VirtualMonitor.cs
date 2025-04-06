namespace DataModels;

public record VirtualMonitor
{
    public string OutputName { get; set; }
    public string Resolution { get; set; }
    public string Position { get; set; }
    public string? EDID { get; set; }
}