using System.ComponentModel;

namespace PowerPeriodInterface;

public interface IAggregatedTradePosition
{
    AggregatedTradePositionStatus Status { get; set; }
    DateTime RequestedDateTime { get; }
    int TradePositionCount { get; set; }
    Dictionary<string, double> TradePositions { get; set; }
    List<string> Errors { get; set; }
}

public enum AggregatedTradePositionStatus
{
    [Description("Aggregated Volume executed Successfully")]
    Success,
    [Description("Aggregated Volume executed successfully but with some Errors")]
    SuccessWithErrors,
    [Description("Aggregated Volume execution Failed")]
    Failure
}