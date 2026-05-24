using PowerPeriodInterface;
using Services;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<PowerPeriod>
{
    public IEnumerable<PowerPeriod> GetTradePositions(DateTime localDateTime)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PowerPeriod>> GetTradePositionsAsync(DateTime localDateTime)
    {
        throw new NotImplementedException();
    }
}
