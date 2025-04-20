namespace MultipleScreenPowersave.App;

using MultipleScreenPowersave.Model;

/// <summary>
/// Service to turn on/off a monitor via DDC/CI and MCCS.
/// See <see href="https://de.wikipedia.org/wiki/Display_Data_Channel"/>
/// and <see href="https://en.wikipedia.org/wiki/Monitor_Control_Command_Set"/>.
/// </summary>
public interface IDisplayDataChannelService : IDisplayControlService { }
