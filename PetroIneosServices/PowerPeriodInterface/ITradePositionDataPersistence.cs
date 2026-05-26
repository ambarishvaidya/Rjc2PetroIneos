namespace PowerPeriodInterface;

public interface ITradePositionDataPersistence
{
    Task SaveAggregatedPositions(IAggregatedPositionResult position);
}
