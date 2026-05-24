using PowerPeriodInterface;
using Services;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<IAggregatedTradePosition>
{
    private int tolerance = 1;
    private readonly IPowerService _powerService;    

    public TradePositionAggregator(IPowerService powerService)
    {
        _powerService = powerService;
    }

    public IAggregatedTradePosition GetTradePositions(DateTime localDateTime)
    {
        if(!IsPassedLocalDateTimeValid(localDateTime))         
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        try
        {
            var resp = _powerService.GetTrades(localDateTime);
        }
        catch (Exception ex)
        {
            return new AggregatedTradePosition(localDateTime, 0, false) { Errors = new List<string> { ex.Message } };
        }
        return null;
    }

    public async Task<IAggregatedTradePosition> GetTradePositionsAsync(DateTime localDateTime)
    {
        if (!IsPassedLocalDateTimeValid(localDateTime))        
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        try
        {
            var resp = await _powerService.GetTradesAsync(localDateTime);
        }
        catch (Exception ex)
        {
            return new AggregatedTradePosition(localDateTime, 0, false) { Errors = new List<string> { ex.Message } };
        }
        
        return null;
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