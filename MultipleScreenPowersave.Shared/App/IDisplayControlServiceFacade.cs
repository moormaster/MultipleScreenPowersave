namespace MultipleScreenPowersave.App;

/// <summary>
/// Facade to turn off monitors by trying:
/// <list type="bullet">
/// <item>to use DDC commands</item>
/// <item>set backlight</item>
/// </list>
/// </summary>
public interface IDisplayControlServiceFacade : IDisplayControlService { }
