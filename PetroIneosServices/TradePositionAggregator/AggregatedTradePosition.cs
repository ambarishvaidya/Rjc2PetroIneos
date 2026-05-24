using PowerPeriodInterface;

namespace TradePositionData;

public class AggregatedTradePosition : IAggregatedTradePosition
{
    private AggregatedTradePosition() { }
    internal AggregatedTradePosition(DateTime requestedDateTime, int positionCount, bool successful)
    {
        RequestedDateTime = requestedDateTime;
        TradePositionCount = positionCount;
        IsSuccessful = successful;
    }

    public DateTime RequestedDateTime { get; }
    public int TradePositionCount { get; }
    public bool IsSuccessful { get; }
    public Dictionary<string, double> TradePositions { get; internal set; }
}