namespace MultipleScreenPowersave.App.WindowsImpl;

using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.VCP;
using Windows.Win32;
using Windows.Win32.Foundation;

/// <summary>
/// Win32 implementation of <see cref="IDisplayDataChannelService"/>.
/// </summary>
public class DisplayDataChannelService : IDisplayDataChannelService
{
    /// <summary>
    /// Turn off given physical monitor.
    /// </summary>
    /// <param name="monitor">Handle to the physical monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn off monitor.</exception>
    public void TurnOffMonitor(PhysicalMonitorInformation monitor)
    {
        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)monitor.Handle.Value,
            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,
            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmsOff
        );

        if (hresult != 1)
        {
            throw new InvalidOperationException(
                $"Failed to turn off monitor #{monitor.Handle.Value}: HRESULT={hresult.ToHexString()}"
            );
        }
    }

    /// <summary>
    /// Turn on given physical monitor.
    /// </summary>
    /// <param name="monitor">Handle to the physical monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)monitor.Handle.Value,
            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,
            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmOn
        );

        if (hresult != 1)
        {
            throw new InvalidOperationException(
                $"Failed to turn on monitor #{monitor.Handle.Value}: HRESULT={hresult.ToHexString()}"
            );
        }
    }
}
