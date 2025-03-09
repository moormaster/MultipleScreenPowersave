namespace MultipleScreenPowersave.Model;

using System.Text;
using CommunityToolkit.Diagnostics;

/// <summary>
/// Dto containing process information belonging to a certain window.
/// </summary>
public class WindowProcessInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProcessInformation"/> class.
    /// </summary>
    /// <param name="processName">Name of the process belonging to the window.</param>
    /// <param name="windowTitle">Title of the window.</param>
    public WindowProcessInformation(string processName, string windowTitle)
    {
        Guard.IsNotNullOrWhiteSpace(processName);
        Guard.IsNotNull(windowTitle);

        this.ProcessName = processName;
        this.WindowTitle = windowTitle;
    }

    /// <summary>
    /// Gets the name of the process belonging to the window.
    /// </summary>
    public string ProcessName { get; }

    /// <summary>
    /// Gets the title of the window.
    /// </summary>
    public string WindowTitle { get; }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("WindowProcessInformation {");
        sb.AppendLine($"\tProcessName: \"{this.ProcessName}\",");
        sb.AppendLine($"\tWindowTitle: \"{this.WindowTitle}\"");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
