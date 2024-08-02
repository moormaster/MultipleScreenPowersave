namespace MultipleScreenPowersave.Model;

using CommunityToolkit.Diagnostics;
using System.Text;

public class WindowProcessInformation
{
    public WindowProcessInformation(string processName, string windowTitle)
    {
        Guard.IsNotNullOrWhiteSpace(processName);
        Guard.IsNotNull(windowTitle);

        ProcessName = processName;
        WindowTitle = windowTitle;
    }

    public string ProcessName { get; }

    public string WindowTitle { get; }

    public override string? ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("WindowProcessInformation {");
        sb.AppendLine($"\tProcessName: \"{ProcessName}\",");
        sb.AppendLine($"\tWindowTitle: \"{WindowTitle}\"");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
