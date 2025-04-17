namespace MultipleScreenPowersave.Query.LinuxImpl;

using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using static MultipleScreenPowersave.ProcessHelper;

/// <summary>
/// Linux implementation of <see cref="IWindowQuery"/>.
/// </summary>
public class WindowQuery : IWindowQuery
{
    private readonly HashSet<string> windowTypes = ["normal", "utility"];

    /// <inheritdoc/>
    public IEnumerable<WindowProcessInformation> GetWindows()
    {
        var (command, arguments) = ("xwininfo", "-root -children -int");
        var exitCode = RunProcess(
            command,
            arguments,
            out var standardOutput,
            out var standardError
        );
        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        var windowProcessInformations = ParseXWinInfoChildrenOutput(standardOutput);

        foreach (var window in windowProcessInformations)
        {
            (command, arguments) = ("xwininfo", $"-id {window.Handle} -stats -wm");
            exitCode = RunProcess(command, arguments, out standardOutput, out standardError);
            ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

            var (mapState, windowStates, windowType) = ParseXWinInfoOutput(standardOutput);

            window.X11MapState = mapState;
            window.X11WindowStates.AddRange(windowStates);
            window.X11WindowType = windowType;
        }

        return windowProcessInformations
            .Where(window => !IsWindowAreaZero(window))
            .Where(window => !IsWindowMinimized(window))
            .Where(window => this.IsWindowVisible(window));
    }

    private static bool IsWindowAreaZero(WindowProcessInformation windowProcessInformation) =>
        windowProcessInformation.Rectangle.GetArea() <= double.Epsilon;

    private static bool IsWindowMinimized(WindowProcessInformation windowProcessInformation) =>
        windowProcessInformation.X11WindowStates.Any(state =>
            state.Equals("hidden", StringComparison.CurrentCultureIgnoreCase)
        );

    private static List<WindowProcessInformation> ParseXWinInfoChildrenOutput(string output)
    {
        var windows = new List<WindowProcessInformation>();
        var lines = output.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            var pattern =
                @"(?<handle>\d+) (?<title>\(has no name\)|"".*""): \((?<res_name>\(none\)|"".*"") (?<res_class>\(none\)|"".*"")\)\s+(?<width>\d+)x(?<height>\d+)\+(?<x>[-]?\d+)\+(?<y>[-]?\d+)";
            if (!Regex.IsMatch(trimmed, pattern))
                continue;

            var match = Regex.Match(trimmed, pattern);
            if (!match.Success)
                continue;

            var windowTitle = match.Groups["title"].Value;
            if (windowTitle == "(has no name)")
            {
                windowTitle = string.Empty;
            }
            else
            {
                // remove enclosing double-quotes
                windowTitle = windowTitle.Substring(1, windowTitle.Length - 2);
            }

            windows.Add(
                new WindowProcessInformation(
                    handle: new WindowHandle(int.Parse(match.Groups["handle"].Value)),
                    processName: match.Groups["res_name"].Value,
                    windowTitle: windowTitle,
                    rectangle: new Rect(
                        x: int.Parse(match.Groups["x"].Value),
                        y: int.Parse(match.Groups["y"].Value),
                        width: int.Parse(match.Groups["width"].Value),
                        height: int.Parse(match.Groups["height"].Value)
                    )
                )
            );
        }

        return windows;
    }

    private static (
        string MapState,
        List<string> WindowStates,
        string WindowType
    ) ParseXWinInfoOutput(string output)
    {
        string? mapState = null;
        List<string> windowStates = [];
        string? windowType = null;

        var lines = output.Split('\n');

        bool isParseWindowStates = false;
        bool isParseWindowType = false;
        foreach (var line in lines)
        {
            if (isParseWindowStates && !Regex.IsMatch(line, "^          "))
                isParseWindowStates = false;

            switch (isParseWindowStates, isParseWindowType)
            {
                case (false, false):
                    if (Regex.IsMatch(line, "Map State: "))
                        mapState = line.Split(": ")[1].Trim();
                    if (Regex.IsMatch(line, "Window type:"))
                        isParseWindowType = true;
                    if (Regex.IsMatch(line, "Window state:"))
                        isParseWindowStates = true;
                    break;

                case (_, true):
                    windowType = line.Trim();
                    isParseWindowType = false;
                    break;

                case (true, _):
                    windowStates.Add(line.Trim());
                    break;
            }
        }

        return (mapState ?? string.Empty, windowStates, windowType ?? string.Empty);
    }

    private bool IsWindowVisible(WindowProcessInformation windowProcessInformation) =>
        this.windowTypes.Contains(windowProcessInformation.X11WindowType?.ToLower()!)
        && windowProcessInformation.X11MapState?.ToLower() == "isviewable";
}
