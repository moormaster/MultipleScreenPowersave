namespace MultipleScreenPowersave.VCP;

/// <summary>
/// Class containing constants for VCP features reported by a physical monitor.
/// </summary>
public static class FeatureConstants
{
    /// <summary>
    /// PowerMode feature.
    /// <see href="https://github.com/rockowitz/ddcutil/blob/ca610f91d5483e19bfdae88bb0094973cc81fc95/src/vcp/vcp_feature_codes.c#L4049"/>.
    /// </summary>
    public const int PowerMode = 0xD6;
}
