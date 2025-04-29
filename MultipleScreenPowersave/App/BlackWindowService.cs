namespace MultipleScreenPowersave.App;

using Avalonia.Threading;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Options;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Ui;

/// <summary>
/// Implementation of <see cref="IBlackWindowService"/>.
/// </summary>
public class BlackWindowService : IBlackWindowService
{
    private readonly Dictionary<string, BlackWindow> windowByEdidHex = [];
    private readonly IOptions<BlackWindowServiceOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlackWindowService"/> class.
    /// </summary>
    /// <param name="options">BlackWindowServiceOptions instance.</param>
    public BlackWindowService(IOptions<BlackWindowServiceOptions> options)
    {
        this.options = options;
    }

    /// <inheritdoc/>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        Guard.IsNotNullOrWhiteSpace(physicalMonitor.EdidHex);

        Dispatcher.UIThread.Invoke(() =>
        {
            BlackWindow window;
            if (
                !this.windowByEdidHex.TryGetValue(physicalMonitor.EdidHex!, out window!) // window not existing?
                || window.PlatformImpl is null // existing window already closed?
            )
            {
                window = this.CreateWindow(virtualMonitor);
                this.windowByEdidHex[physicalMonitor.EdidHex] = window;
            }

            window.Show();
        });
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        Guard.IsNotNullOrWhiteSpace(monitor.EdidHex);

        Dispatcher.UIThread.Invoke(() =>
        {
            if (this.windowByEdidHex.Remove(monitor.EdidHex!, out var window))
                window.Close();
        });
    }

    private BlackWindow CreateWindow(DisplayMonitorInformation virtualMonitor)
    {
        BlackWindow window;

        var background = Avalonia.Media.Brushes.Black;
        var matchinOptionsEntry = this.options.Value.PhysicalMonitors.FirstOrDefault(
            physicalMonitorEntry => physicalMonitorEntry.IsMatch(virtualMonitor)
        );

        if (matchinOptionsEntry?.InvertBackgroundColor ?? false)
            background = Avalonia.Media.Brushes.White;

        window = new BlackWindow(closeOnMouseMove: true)
        {
            Background = background,
            Position = virtualMonitor.MonitorRectangle.Location.ToPixelPoint(),
            Title = BlackWindow.BlackWindowTitle,
        };

        return window;
    }
}
