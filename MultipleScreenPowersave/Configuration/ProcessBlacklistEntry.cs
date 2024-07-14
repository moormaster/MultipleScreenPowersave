namespace MultipleScreenPowersave.Configuration;

using System.Diagnostics;
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

    public bool IsMatch(Process process)
    {
        if (this.ProcessName != null)
        {
            if (!this.ProcessName.IsMatch(process.ProcessName))
                return false;
        }

        if (this.WindowTitle != null)
        {
            if (!this.WindowTitle.IsMatch(process.MainWindowTitle))
                return false;
        }

        return true;
    }
}
