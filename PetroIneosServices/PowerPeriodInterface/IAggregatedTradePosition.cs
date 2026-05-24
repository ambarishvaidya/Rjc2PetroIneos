namespace PowerPeriodInterface;

public interface IAggregatedTradePosition
{
    bool IsSuccessful { get; set; }
    DateTime RequestedDateTime { get; }
    int TradePositionCount { get; set; }
    Dictionary<string, double> TradePositions { get; set; }
    List<string> Errors { get; set; }
}