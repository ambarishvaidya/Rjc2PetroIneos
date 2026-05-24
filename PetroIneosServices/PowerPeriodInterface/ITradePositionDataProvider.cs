namespace PowerPeriodInterface;

public interface ITradePositionDataProvider<T> where T : class
{
    IEnumerable<T> GetTradePositions();
    Task<IEnumerable<T>> GetTradePositionsAsync();
}
