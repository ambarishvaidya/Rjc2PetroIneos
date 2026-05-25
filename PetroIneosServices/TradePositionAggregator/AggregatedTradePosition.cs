using PowerPeriodInterface;

namespace TradePositionData;

public class AggregatedTradePosition : IAggregatedTradePosition
{
    private AggregatedTradePosition() { }
    internal AggregatedTradePosition(DateTime requestedDateTime)
    {
        Id = Guid.NewGuid();
        RequestedDateTime = requestedDateTime;
        Status = AggregatedTradePositionStatus.Failure;
        Errors = new List<string>();
    }

    public DateTime RequestedDateTime { get; }
    public int TradePositionCount { get; set; }
    public bool IsSuccessful { get; set; }
    public Dictionary<string, double> TradePositions { get; set; }
    public List<string> Errors { get; set; }
    public AggregatedTradePositionStatus Status { get; set; }

    public Guid Id { get; }

    public override string ToString()
    {
        return $"{Id} : {RequestedDateTime} [{Enum.GetName(typeof(AggregatedTradePositionStatus), Status)}]";
    }
}