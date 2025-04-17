namespace MultipleScreenPowersave.Query.LinuxImpl;

using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.App;
using static MultipleScreenPowersave.ProcessHelper;

/// <summary>
/// Linux implementation of <see cref="IMouseQuery"/>.
/// </summary>
public class MouseQuery : IMouseQuery
{
    /// <inheritdoc/>
    public Point GetCurrentMouseCursorPosition()
    {
        var (command, arguments) = ("xdotool", "getmouselocation");
        var exitCode = RunProcess(
            command,
            arguments,
            out var standardOutput,
            out var standardError
        );
        ThrowIfExitCodeIsNotZero(exitCode, command, arguments, standardError);

        var match = Regex.Match(standardOutput, @"x:(\d+) y:(\d+)");
        var (x, y) = (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));

        return new Point(x, y);
    }
}
