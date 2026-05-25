using PowerPeriodInterface;

namespace PowerPeriodService;

public class Worker(ILogger<Worker> logger, ITradePositionDataProvider<IAggregatedTradePosition> tradePositionDataProvider) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var resp = tradePositionDataProvider.GetTradePositions(DateTime.Now);
            logger.LogInformation("Worker running at: {time} with response: {response}", DateTimeOffset.Now, resp);
            if(resp.Status == AggregatedTradePositionStatus.Success)
            {
                foreach (var kvp in resp.TradePositions)
                {
                    logger.LogInformation("Trade Position: {key} with value: {value}", kvp.Key, kvp.Value);
                }
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
