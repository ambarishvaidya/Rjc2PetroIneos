using PowerPeriodInterface;
using Services;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<IAggregatedTradePosition>
{
    private int tolerance = 1;
    private readonly IPowerService _powerService;    
    private static FrozenDictionary<int, string> PeriodTimeMap = new Dictionary<int, string>()
    {
        { 1 , "23:00" }, { 2 , "00:00" }, { 3 , "01:00" }, { 4 , "02:00" }, { 5 , "03:00" }, { 6 , "04:00" }, 
        { 7 , "05:00" }, { 8 , "06:00" }, { 9 , "07:00" }, { 10 , "08:00" }, { 11 , "09:00" }, { 12 , "10:00" },
        { 13 , "11:00" }, { 14 , "12:00" }, { 15 , "13:00" }, { 16 , "14:00" }, { 17 , "15:00" }, { 18 , "16:00" },
        { 19 , "17:00" }, { 20 , "18:00" }, { 21 , "19:00" }, { 22 , "20:00" }, { 23 , "21:00" }, { 24 , "22:00" }
    }.ToFrozenDictionary();

    public TradePositionAggregator(IPowerService powerService)
    {
        _powerService = powerService;
    }

    public IAggregatedTradePosition GetTradePositions(DateTime localDateTime)
    {
        if(!IsPassedLocalDateTimeValid(localDateTime))         
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");
        IAggregatedTradePosition aggregatedTradePosition = new AggregatedTradePosition(localDateTime);
        aggregatedTradePosition.IsSuccessful = false;
        aggregatedTradePosition.TradePositionCount = 0;

        try
        {
            var resp = _powerService.GetTrades(localDateTime);
            if (resp == null)
            {
                aggregatedTradePosition.Errors = new List<string> { "Received null response from PowerService." };
                return aggregatedTradePosition;
            }

            if (!resp.Any())
            { 
                aggregatedTradePosition.Errors = new List<string> { "Received empty response from PowerService." }; 
                return aggregatedTradePosition;
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
            var resp = await _powerService.GetTradesAsync(localDateTime);
            if (resp == null)
            {
                aggregatedTradePosition.Errors = new List<string> { "Received null response from PowerService." };
                return aggregatedTradePosition;
            }

            if (!resp.Any())
            {
                aggregatedTradePosition.Errors = new List<string> { "Received empty response from PowerService." };
                return aggregatedTradePosition;
            }

            ProcessPowerTrades(resp, aggregatedTradePosition);

        }
        catch (Exception ex)
        {
            aggregatedTradePosition.Errors = new List<string> { ex.Message } ;
        }
        
        return aggregatedTradePosition;
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
        aggregatedTradePosition.IsSuccessful = !errors.Any();
    }

    private bool IsPassedLocalDateTimeValid(DateTime localDateTime)
    {
        if (localDateTime.Kind != DateTimeKind.Local) return false;            

        var now = DateTime.Now;

        var lt = new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, localDateTime.Second);
        var nt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        return (nt - lt).Duration() <= TimeSpan.FromMinutes(tolerance);
    }
}