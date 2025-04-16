namespace MultipleScreenPowersave.Configuration;

using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

/// <summary>
/// Entry describing a Process to be blacklisted.
/// </summary>
public record class ProcessBlacklistEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessBlacklistEntry"/> class.
    /// </summary>
    /// <param name="processName">Regular expression that matches the process to be blacklisted.</param>
    /// <param name="windowTitle">Regular expression that matches the window title to be blacklisted.</param>
    /// <exception cref="ArgumentNullException">All arguments are null.</exception>
    public ProcessBlacklistEntry(Regex? processName, Regex? windowTitle)
    {
        if (processName == null && windowTitle == null)
        {
            throw new ArgumentNullException(
                nameof(processName),
                "At least one property must be != null"
            );
        }

        this.ProcessName = processName;
        this.WindowTitle = windowTitle;
    }

    /// <summary>
    /// Gets the regular expression that matches the process to be blacklisted.
    /// </summary>
    [JsonConverter(typeof(RegexJsonConverter))]
    public Regex? ProcessName { get; }

    /// <summary>
    /// Gets the regular expression that matches the window title to be blacklisted.
    /// </summary>
    [JsonConverter(typeof(RegexJsonConverter))]
    public Regex? WindowTitle { get; }

    /// <summary>
    /// Determines wether this entry matches to the given WindowProcessInformation.
    /// </summary>
    /// <param name="windowProcessInformation">WindowProcessInformation to check against.</param>
    /// <returns>True iff processName and windowTitle regular expressions match to the given WindowProcessInformation.</returns>
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
