namespace MultipleScreenPowersave.App;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Long running hosted service periodically calling the <see cref="ApplicationService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HostedBackgroundService"/> class.
/// </remarks>
/// <param name="applicationService">ApplicationService instance.</param>
/// <param name="logger">Logger instance.</param>
/// <param name="options">HostedBackgroundService options.</param>
public class HostedBackgroundService(
    IApplicationService applicationService,
    ILogger<HostedBackgroundService> logger,
    IOptions<HostedBackgroundServiceOptions> options
) : BackgroundService, IHostedBackgroundService
{
    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        base.StopAsync(cancellationToken);

        try
        {
            applicationService.TurnOnAllMonitors();
        }
        catch (Exception e)
        {
            logger.LogError("Failed to turn back on all monitors: {e}", e);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(
            () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        applicationService.TurnOnOnlyUsedMonitors();
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Failed to turn on only used monitors: {e}", e);
                    }

                    Thread.Sleep(options.Value.SleepTimeMs);
                }

                applicationService.TurnOnAllMonitors();
            },
            cancellationToken: stoppingToken
        );
    }
}
