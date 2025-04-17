namespace MultipleScreenPowersave;

using System.Diagnostics;

/// <summary>
/// Helper methods for executing command line utils.
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// Execute a command line process.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="arguments">Arguments to execute command with.</param>
    /// <param name="standardOutput">Contents written to the standard output stream.</param>
    /// <param name="standardError">Contents written to the standard error stream.</param>
    /// <returns>The exitCode of the process.</returns>
    public static int RunProcess(
        string command,
        string arguments,
        out string standardOutput,
        out string standardError
    )
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(processStartInfo);

        standardOutput = process?.StandardOutput.ReadToEnd() ?? string.Empty;
        standardError = process?.StandardError.ReadToEnd() ?? string.Empty;

        process?.WaitForExit();

        return process?.ExitCode ?? -1;
    }

    /// <summary>
    /// Throws an exception when process of the given command exited with non-zero.
    /// </summary>
    /// <param name="exitCode">ExitCode of the process.</param>
    /// <param name="command">Command the process was initiated with.</param>
    /// <param name="arguments">Arguments passed to the command.</param>
    /// <param name="standardError">Contents written to the standard error stream.</param>
    /// <exception cref="InvalidOperationException">exitCode is != 0.</exception>
    public static void ThrowIfExitCodeIsNotZero(
        int exitCode,
        string command,
        string arguments,
        string standardError
    )
    {
        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $@"Command exited with exit code {exitCode}:
                {command} {arguments}

                {standardError}"
            );
        }
    }
}
