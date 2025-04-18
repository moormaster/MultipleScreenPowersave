namespace MultipleScreenPowersave.Model;

using System.Text;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;

/// <summary>
/// Dto returned by <see cref="IScreenQuery.GetScreenInformation"/>.
/// </summary>
/// <param name="handle">PhysicalMonitorHandle.</param>
/// <param name="description">Text description of the physical monitor.</param>
public class PhysicalMonitorInformation(PhysicalMonitorHandle handle, string description)
{
    /// <summary>
    /// Gets the PhysicalMonitorHandle.
    /// </summary>
    public PhysicalMonitorHandle Handle { get; } = handle;

    /// <summary>
    /// Gets the text description of the physical monitor.
    /// </summary>
    public string Description { get; } = description;

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("PhysicalMonitorInformation {");

        sb.AppendLine($"\tHandle: {this.Handle},");
        sb.AppendLine($"\tDescription: {this.Description},");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
