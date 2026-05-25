using Microsoft.Extensions.Logging;
using Polly;
using PowerPeriodInterface;
using Services;
using System.Collections.Frozen;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<IAggregatedTradePosition>
{
    private int TOLERANCE_IN_MINS = 1;
    private readonly IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetry;
    private readonly ISyncPolicy<IEnumerable<PowerTrade>> syncRetry;
    private readonly IPowerService _powerService;    
    private readonly ILogger _logger;
    private static FrozenDictionary<int, string> PeriodTimeMap = new Dictionary<int, string>()
    {
        { 1 , "23:00" }, { 2 , "00:00" }, { 3 , "01:00" }, { 4 , "02:00" }, { 5 , "03:00" }, { 6 , "04:00" }, 
        { 7 , "05:00" }, { 8 , "06:00" }, { 9 , "07:00" }, { 10 , "08:00" }, { 11 , "09:00" }, { 12 , "10:00" },
        { 13 , "11:00" }, { 14 , "12:00" }, { 15 , "13:00" }, { 16 , "14:00" }, { 17 , "15:00" }, { 18 , "16:00" },
        { 19 , "17:00" }, { 20 , "18:00" }, { 21 , "19:00" }, { 22 , "20:00" }, { 23 , "21:00" }, { 24 , "22:00" }
    }.ToFrozenDictionary();

    

    public TradePositionAggregator(IPowerService powerService,
        ILogger<TradePositionAggregator> logger)
    {
        _powerService = powerService;        
        _logger = logger;
        asyncRetry = RetryPolicy.AsyncRetry;
        syncRetry = RetryPolicy.SyncRetry;
    }

    /// <summary>
    /// This is used only by Test environment.
    /// Service worker will not use this constructor as we are not 
    /// exposing PowerService to outside world.
    /// </summary>
    /// <param name="powerService"></param>
    /// <param name="asyncRetry"></param>
    /// <param name="syncRetry"></param>
    public TradePositionAggregator(IPowerService powerService, 
        ILogger<TradePositionAggregator> logger,
        IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetry,
        ISyncPolicy<IEnumerable<PowerTrade>> syncRetry)
    {
        _powerService = powerService;
        _logger = logger;
        this.asyncRetry = asyncRetry;
        this.syncRetry = syncRetry;
    }

    public IAggregatedTradePosition GetTradePositions(DateTime localDateTime)
    {
        if(!IsPassedLocalDateTimeValid(localDateTime))         
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");
        IAggregatedTradePosition aggregatedTradePosition = new AggregatedTradePosition(localDateTime);        

        try
        {
            var resp = syncRetry.Execute(() => _powerService.GetTrades(localDateTime));
            (bool flowControl, IAggregatedTradePosition value) = ValidateGetTradesResponse(aggregatedTradePosition, resp);
            if (!flowControl)
            {
                return value;
            }

            ProcessPowerTrades(resp, aggregatedTradePosition);
        }
        catch (Exception ex)
        {
            aggregatedTradePosition.Errors = new List<string> { ex.Message };
        }
        return aggregatedTradePosition;
    }


    public async Task<IAggregatedTradePosition> GetTradePositionsAsync(DateTime localDateTime)
    {
        if (!IsPassedLocalDateTimeValid(localDateTime))        
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        IAggregatedTradePosition aggregatedTradePosition = new AggregatedTradePosition(localDateTime);        

        try
        {
            var resp = await asyncRetry.ExecuteAsync(() => _powerService.GetTradesAsync(localDateTime));
            (bool flowControl, IAggregatedTradePosition value) = ValidateGetTradesResponse(aggregatedTradePosition, resp);
            if (!flowControl)
            {
                return value;
            }

            ProcessPowerTrades(resp, aggregatedTradePosition);

        }
        catch (Exception ex)
        {
            aggregatedTradePosition.Errors = new List<string> { ex.Message } ;
        }
        
        return aggregatedTradePosition;
    }

    private (bool flowControl, IAggregatedTradePosition value) ValidateGetTradesResponse(IAggregatedTradePosition aggregatedTradePosition, IEnumerable<PowerTrade> resp)
    {
        if (resp == null)
        {
            aggregatedTradePosition.Errors = new List<string> { "Received null response from PowerService." };
            return (flowControl: false, value: aggregatedTradePosition);
        }

        if (!resp.Any())
        {
            aggregatedTradePosition.Errors = new List<string> { "Received empty response from PowerService." };
            return (flowControl: false, value: aggregatedTradePosition);
        }

        return (flowControl: true, value: aggregatedTradePosition);
    }

    private void ProcessPowerTrades(IEnumerable<PowerTrade> powerTrades, IAggregatedTradePosition aggregatedTradePosition)
    {
        var tradePositions = new Dictionary<string, double>();
        var errors = new List<string>();
        var count = 0;

        foreach (var powerTrade in powerTrades)
        {            
            foreach(var period in powerTrade.Periods)
            {
                if (PeriodTimeMap.TryGetValue(period.Period, out var time))
                {
                    errors.Add($"Period {period.Period} is not supported. Igorning {powerTrade.Date} [{period.Period} : {period.Volume}].");
                }
                else
                {
                    if (!tradePositions.TryGetValue(time, out var volume))
                        tradePositions.Add(time, 0);
                    tradePositions[time] += period.Volume;
                }
                count++;
            }
        }
            
        aggregatedTradePosition.TradePositionCount = count;
        aggregatedTradePosition.Errors.AddRange(errors);
        aggregatedTradePosition.TradePositions = tradePositions;
        aggregatedTradePosition.Status = errors.Any() ? AggregatedTradePositionStatus.SuccessWithErrors : AggregatedTradePositionStatus.Success;
    }

    private bool IsPassedLocalDateTimeValid(DateTime localDateTime)
    {
        if (localDateTime.Kind != DateTimeKind.Local) return false;            

        var now = DateTime.Now;

        var lt = new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, localDateTime.Second);
        var nt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        return (nt - lt).Duration() <= TimeSpan.FromMinutes(TOLERANCE_IN_MINS);
    }
}

internal static class RetryPolicy
{
    private static ISyncPolicy<IEnumerable<PowerTrade>> retryExceptions = Policy<IEnumerable<PowerTrade>>
        .Handle<Exception>()
        .WaitAndRetry(
            3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
        );

    private static ISyncPolicy<IEnumerable<PowerTrade>> retryEmptyOrNull = Policy<IEnumerable<PowerTrade>>
        .HandleResult(r => r == null || !r.Any())
        .WaitAndRetry(
            3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
        );

    internal static ISyncPolicy<IEnumerable<PowerTrade>> SyncRetry = Policy.Wrap(retryExceptions, retryEmptyOrNull);

    private static IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetryExceptions = Policy<IEnumerable<PowerTrade>>
        .Handle<Exception>()
        .WaitAndRetryAsync(
            3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
        );

    private static IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetryEmptyOrNull = Policy<IEnumerable<PowerTrade>>
        .HandleResult(r => r == null || !r.Any())
        .WaitAndRetryAsync(
            3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
        );

    internal static IAsyncPolicy<IEnumerable<PowerTrade>> AsyncRetry = Policy.WrapAsync(asyncRetryExceptions, asyncRetryEmptyOrNull);

}