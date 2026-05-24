using PowerPeriodInterface;

namespace TradePositionData;

public class AggregatedTradePosition : IAggregatedTradePosition
{
    private AggregatedTradePosition() { }
    internal AggregatedTradePosition(DateTime requestedDateTime)
    {
        RequestedDateTime = requestedDateTime;
        IsSuccessful = false;
    }

    public DateTime RequestedDateTime { get; }
    public int TradePositionCount { get; set; }
    public bool IsSuccessful { get; set; }
    public Dictionary<string, double> TradePositions { get; set; }
    public List<string> Errors { get; set; }
}