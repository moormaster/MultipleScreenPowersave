namespace MultipleScreenPowersave.Configuration;

using System;
using MultipleScreenPowersave.Model;

/// <summary>
/// Entry describing a PhysicalMonitor to be blacklisted.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PhysicalMonitorBlacklistEntry"/> class.
/// </remarks>
/// <param name="description">Regular expression that should match to the physical monitors description to be blacklisted.</param>
/// <param name="deviceId">Regular expression that should match to the physical monitors deviceId to be blacklisted.</param>
/// <param name="wmiInstanceName">Regular expression that should match to the physical monitors wmiInstanceName to be blacklisted.</param>
/// <param name="index">0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.</param>
/// <exception cref="ArgumentNullException">All arguments were null.</exception>
public class PhysicalMonitorBlacklistEntry(
    string? description = null,
    string? deviceId = null,
    string? wmiInstanceName = null,
    uint? index = null
) : PhysicalMonitorEntryBase(description, deviceId, wmiInstanceName, index) { }
