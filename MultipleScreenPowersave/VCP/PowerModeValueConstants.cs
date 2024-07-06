namespace MultipleScreenPowersave.VCP;

public static class PowerModeValueConstants
{
    // see https://github.com/rockowitz/ddcutil/blob/ca610f91d5483e19bfdae88bb0094973cc81fc95/src/vcp/vcp_feature_codes.c#L2581
    public const uint DpmOn = 0x01;
    public const uint DpmsStandby = 0x02;
    public const uint DpmsSuspend = 0x03;
    public const uint DpmsOff = 0x04;
    public const uint Off = 0x05;
}
