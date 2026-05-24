namespace PowerPeriodInterface;

public interface IAggregatedTradePosition
{
    bool IsSuccessful { get; }
    DateTime RequestedDateTime { get; }
    int TradePositionCount { get; }
    Dictionary<string, double> TradePositions { get; }
    List<string> Errors { get; }
}