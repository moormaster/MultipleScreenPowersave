namespace MultipleScreenPowersave.Configuration;

using System.Text.RegularExpressions;

public record class ProcessBlacklistEntry
{
    public ProcessBlacklistEntry(Regex? processName, Regex? windowTitle)
    {
        if (processName == null
            && windowTitle == null)
            throw new ArgumentNullException(nameof(processName), "At least one property must be != null");

        this.ProcessName = processName;
        this.WindowTitle = windowTitle;
    }

    public Regex? ProcessName { get; }

    public Regex? WindowTitle { get; }
}
