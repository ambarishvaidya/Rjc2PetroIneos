using PowerPeriodInterface;
using System.IO.Abstractions;

namespace PowerPeriodService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITradePositionDataProvider<IAggregatedPositionResult> _tradePositionDataProvider;
    private readonly ITradePositionDataPersistence _tradePositionDataPersistence;
    private readonly IFileSystem _fileSystem;
    private readonly int _intervalInMinutes;
    public Worker(ILogger<Worker> logger, IConfiguration configuration,
                ITradePositionDataProvider<IAggregatedPositionResult> tradePositionDataProvider,
                ITradePositionDataPersistence tradePositionDataPersistence,
                IFileSystem fileSystem)
    {
        _logger = logger;
        _configuration = configuration;
        _tradePositionDataProvider = tradePositionDataProvider;
        _tradePositionDataPersistence = tradePositionDataPersistence;
        _fileSystem = fileSystem;

        var interval = _configuration["IntervalInMinutes"];
        if (string.IsNullOrEmpty(interval))
        {
            _logger.LogCritical("IntervalInMinutes is not configured. Service cannot start.");
            throw new InvalidOperationException("IntervalInMinutes is not configured.");
        }
        if (!int.TryParse(interval, out _intervalInMinutes) || _intervalInMinutes <= 0)
        {
            _logger.LogCritical($"IntervalInMinutes '{interval}' is not a valid positive number greater than 0. Service cannot start.");
            throw new InvalidOperationException($"IntervalInMinutes '{interval}' is not a valid positive number greater than 0.");
        }        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service starting. Extracting service start positions.");
        Run(DateTime.Now, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation($"Worker running. Next run at { DateTime.Now.AddMinutes(_intervalInMinutes)}.");
                await Task.Delay(_intervalInMinutes * 60 * 1000, stoppingToken);
                Run(DateTime.Now, stoppingToken);
            }
            catch(TaskCanceledException)
            {
                _logger.LogInformation("Service stopping due to Requesting cancellation.");
                break;
            }
            catch (Exception oex)
            {
                _logger.LogError($"An error occurred while executing the worker. Err {oex}");
            }            
        }
    }

    private void Run(DateTime now, CancellationToken stoppingToken)
    {
        _ = Task.Run(() => ExtractPositions(now, stoppingToken));
    }

    private async Task ExtractPositions(DateTime now, CancellationToken stoppingToken)
    {
        try
        {
            var positions = await _tradePositionDataProvider.GetTradePositionsAsync(now);
            await _tradePositionDataPersistence.SaveAggregatedPositions(positions, stoppingToken);
        }
        catch (Exception oex)
        {
            _logger.LogError(oex, "An error occurred while extracting positions.");
        }
    }
    
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
