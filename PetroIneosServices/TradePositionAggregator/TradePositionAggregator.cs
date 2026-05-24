using PowerPeriodInterface;
using Services;

namespace TradePositionData;

public class TradePositionAggregator : ITradePositionDataProvider<PowerPeriod>
{
    public IEnumerable<PowerPeriod> GetTradePositions()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PowerPeriod>> GetTradePositionsAsync()
    {
        throw new NotImplementedException();
    }
}
