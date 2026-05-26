using PowerPeriodInterface;

namespace PowerPeriodService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly IConfiguration configuration;
    private readonly ITradePositionDataProvider<IAggregatedTradePosition> tradePositionDataProvider;
    private readonly ITradePositionDataPersistence tradePositionDataPersistence;
    private readonly int intervalInMinutes;
    public Worker(ILogger<Worker> logger, IConfiguration configuration,
                ITradePositionDataProvider<IAggregatedTradePosition> tradePositionDataProvider,
                ITradePositionDataPersistence tradePositionDataPersistence)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.tradePositionDataProvider = tradePositionDataProvider;
        this.tradePositionDataPersistence = tradePositionDataPersistence;

        var interval = configuration["IntervalInMinutes"];
        if (string.IsNullOrEmpty(interval))
        {
            logger.LogCritical("IntervalInMinutes is not configured. Service cannot start.");
            throw new InvalidOperationException("IntervalInMinutes is not configured.");
        }
        if (!int.TryParse(interval, out int intervalInMinutes) || intervalInMinutes <= 0)
        {
            logger.LogCritical($"IntervalInMinutes '{interval}' is not a valid positive number greater than 0. Service cannot start.");
            throw new InvalidOperationException($"IntervalInMinutes '{interval}' is not a valid positive number greater than 0.");
        }        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Run(DateTime.Now);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation($"Worker running. Next run at { DateTime.Now.AddMinutes(intervalInMinutes)}.");
                await Task.Delay(intervalInMinutes * 60 * 1000, stoppingToken);
                Run(DateTime.Now);
            }
            catch(TaskCanceledException)
            {
                logger.LogInformation("Service stopping due to Requesting cancellation.");
                break;
            }
            catch (Exception oex)
            {
                logger.LogError($"An error occurred while executing the worker. Err {oex}");
            }            
        }
    }

    private void Run(DateTime now)
    {
        _ = Task.Run(() => ExtractPositions(now));
    }

    private async Task ExtractPositions(DateTime now)
    {
        try
        {
            var positions = await tradePositionDataProvider.GetTradePositionsAsync(now);
            await tradePositionDataPersistence.SaveAggregatedPositions(positions);
        }
        catch (Exception oex)
        {
            logger.LogError(oex, "An error occurred while extracting positions.");
        }
    }
    
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
