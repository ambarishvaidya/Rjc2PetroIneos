using PowerPeriodInterface;

namespace PowerPeriodService;

public class Worker(ILogger<Worker> logger, 
    ITradePositionDataProvider<IAggregatedTradePosition> tradePositionDataProvider,
    ITradePositionDataPersistence tradePositionDataPersistence) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var resp = tradePositionDataProvider.GetTradePositions(DateTime.Now);
            logger.LogInformation("Worker running at: {time} with response: {response}", DateTimeOffset.Now, resp);
            await tradePositionDataPersistence.SaveAggregatedPositions(resp);
            await Task.Delay(5 * 60 * 1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
