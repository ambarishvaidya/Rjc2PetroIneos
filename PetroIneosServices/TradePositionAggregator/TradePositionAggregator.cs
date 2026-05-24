using PowerPeriodInterface;
using Services;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<PowerTrade>
{
    private int tolerance = 1;
    private readonly IPowerService _powerService;

    public TradePositionAggregator(IPowerService powerService)
    {
        _powerService = powerService;
    }

    public IEnumerable<PowerTrade> GetTradePositions(DateTime localDateTime)
    {
        if(!IsPassedLocalDateTimeValid(localDateTime))         
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        return _powerService.GetTrades(localDateTime);
    }

    public async Task<IEnumerable<PowerTrade>> GetTradePositionsAsync(DateTime localDateTime)
    {
        if (!IsPassedLocalDateTimeValid(localDateTime))        
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        return await _powerService.GetTradesAsync(localDateTime);
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
