namespace MultipleScreenPowersave.Configuration;

using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

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

    public bool IsMatch(WindowProcessInformation windowProcessInformation)
    {
        if (this.ProcessName != null)
        {
            if (!this.ProcessName.IsMatch(windowProcessInformation.ProcessName))
                return false;
        }

        if (this.WindowTitle != null)
        {
            if (!this.WindowTitle.IsMatch(windowProcessInformation.WindowTitle))
                return false;
        }

        return true;
    }
}
