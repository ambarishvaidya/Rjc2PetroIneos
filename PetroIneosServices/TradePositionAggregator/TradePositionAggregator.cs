using PowerPeriodInterface;
using Services;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<PowerPeriod>
{
    private int tolerance = 1; 

    public IEnumerable<PowerPeriod> GetTradePositions(DateTime localDateTime)
    {
        if(!IsPassedLocalDateTimeValid(localDateTime))         
            throw new ArgumentException("The date cannot be in the past.");

        return null;
    }

    public Task<IEnumerable<PowerPeriod>> GetTradePositionsAsync(DateTime localDateTime)
    {
        if (!IsPassedLocalDateTimeValid(localDateTime))        
            throw new ArgumentException("The date cannot be in the past.");

        return Task.FromResult<IEnumerable<PowerPeriod>>(null);
    }

    private bool IsPassedLocalDateTimeValid(DateTime localDateTime)
    {
        var now = DateTime.Now;

        var lt = new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, localDateTime.Second);
        var nt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        return (nt - lt).Duration() <= TimeSpan.FromMinutes(tolerance);
    }
}
