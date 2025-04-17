namespace MultipleScreenPowersave.VCP;

/// <summary>
/// Class containing constants about possible PowerMode values.
/// <see href="https://github.com/rockowitz/ddcutil/blob/ca610f91d5483e19bfdae88bb0094973cc81fc95/src/vcp/vcp_feature_codes.c#L2581"/>.
/// </summary>
public static class PowerModeValueConstants
{
    /// <summary>
    /// DPM: On,  DPMS: Off.
    /// </summary>
    public const uint DpmOn = 0x01;

    /// <summary>
    /// DPM: Off, DPMS: Standby.
    /// </summary>
    public const uint DpmsStandby = 0x02;

    /// <summary>
    /// DPM: Off, DPMS: Suspend.
    /// </summary>
    public const uint DpmsSuspend = 0x03;

    /// <summary>
    /// DPM: Off, DPMS: Off.
    /// </summary>
    public const uint DpmsOff = 0x04;

    /// <summary>
    /// Write only value to turn off display.
    /// </summary>
    public const uint Off = 0x05;
}
