namespace MultipleScreenPowersave.Configuration;

using MultipleScreenPowersave.Query;

public static class ConfigurationQueryFactory
{
    public const string ConfigFileNameWindows = "config.windows.json";

    public const string ConfigFilename = "config.json";

    public static ConfigurationQuery GetConfigurationQuery()
    {
        return new ConfigurationQuery(GetConfigurationFileName());
    }

    private static string GetConfigurationFileName()
    {
        var fileName = Environment.GetEnvironmentVariable(EnvironmentVariables.MultipleScreenPowerSaveConfigurationFile);
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
