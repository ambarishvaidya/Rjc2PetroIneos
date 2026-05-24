using PowerPeriodInterface;
using Services;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<PowerPeriod>
{
    private int tolerance = 1; 

    public IEnumerable<PowerPeriod> GetTradePositions(DateTime localDateTime)
    {
        if(!IsPassedLocalDateTimeValid(localDateTime))         
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        return null;
    }

    public Task<IEnumerable<PowerPeriod>> GetTradePositionsAsync(DateTime localDateTime)
    {
        if (!IsPassedLocalDateTimeValid(localDateTime))        
            throw new ArgumentException("DateTime has to local time within 1 minute tolerance.");

        return Task.FromResult<IEnumerable<PowerPeriod>>(null);
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
