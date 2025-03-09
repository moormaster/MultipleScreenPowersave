namespace MultipleScreenPowersave.Configuration;

using MultipleScreenPowersave.Query;

/// <summary>
/// Factory for creating a <see cref="ConfigurationQuery"/> instance.
/// </summary>
public static class ConfigurationQueryFactory
{
    /// <summary>
    /// Default configuration file name when running under windows.
    /// </summary>
    public const string ConfigFileNameWindows = "config.windows.json";

    /// <summary>
    /// Default configuration file name for other platforms.
    /// </summary>
    public const string ConfigFilename = "config.json";

    /// <summary>
    /// Creates a new instance of <see cref="ConfigurationQuery"/>.
    /// </summary>
    /// <returns>The instance created.</returns>
    public static ConfigurationQuery GetConfigurationQuery()
    {
        return new ConfigurationQuery(GetConfigurationFileName());
    }

    /// <summary>
    /// Determines the path to the configuration file to be used.
    /// </summary>
    /// <returns>the path to the configuration file.</returns>
    /// <exception cref="InvalidOperationException">HOME environment variable is not set.</exception>
    public static string GetConfigurationFileName()
    {
        var fileName = Environment.GetEnvironmentVariable(
            EnvironmentVariables.MultipleScreenPowerSaveConfigurationFile
        );
        if (!string.IsNullOrEmpty(fileName))
            return fileName;

        var configDir = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        if (string.IsNullOrEmpty(configDir))
            configDir = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(configDir))
            throw new InvalidOperationException("HOME environment variable not set!");

        if (OperatingSystem.IsWindows())
            return $"{configDir}\\{typeof(Program).Assembly.GetName().Name}\\{ConfigFileNameWindows}";

        return $"{configDir}/{typeof(Program).Assembly.GetName().Name}/{ConfigFilename}";
    }
}
