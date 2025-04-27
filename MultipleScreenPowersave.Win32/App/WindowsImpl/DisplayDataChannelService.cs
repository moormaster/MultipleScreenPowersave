namespace MultipleScreenPowersave.App.WindowsImpl;

using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.VCP;
using Windows.Win32;
using Windows.Win32.Foundation;

/// <summary>
/// Win32 implementation of <see cref="IDisplayDataChannelService"/>.
/// </summary>
public class DisplayDataChannelService : IDisplayDataChannelService
{
    /// <inheritdoc/>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        Guard.IsFalse(physicalMonitor.Handle == PhysicalMonitorHandle.Empty);

        var currentPowerModeValue = GetCurrentPowerMode(physicalMonitor);
        if (currentPowerModeValue == PowerModeValueConstants.DpmsOff)
        {
            // monitor is already turned off
            return;
        }

        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)physicalMonitor.Handle.Value,
            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,
            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmsOff
        );

        if (hresult != 1)
        {
            throw new InvalidOperationException(
                $"Failed to turn off monitor #{physicalMonitor.Handle.Value}: HRESULT={hresult.ToHexString()}"
            );
        }
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        Guard.IsFalse(monitor.Handle == PhysicalMonitorHandle.Empty);

        var currentPowerModeValue = GetCurrentPowerMode(monitor);
        if (currentPowerModeValue == PowerModeValueConstants.DpmOn)
        {
            // monitor is already turned on
            return;
        }

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

    private static uint GetCurrentPowerMode(PhysicalMonitorInformation monitor)
    {
        int hresult;
        uint currentValue;
        uint maximumValue;

        unsafe
        {
            hresult = PInvoke.GetVCPFeatureAndVCPFeatureReply(
                (HANDLE)monitor.Handle.Value,
                FeatureConstants.PowerMode,
                pdwCurrentValue: &currentValue,
                pdwMaximumValue: &maximumValue
            );
        }

        if (hresult != 1 || currentValue == 0)
        {
            throw new InvalidOperationException(
                $"Monitor #{monitor.Handle.Value} does not support PowerMode DDC feature: HRESULT={hresult.ToHexString()}"
            );
        }

        return currentValue;
    }
}
