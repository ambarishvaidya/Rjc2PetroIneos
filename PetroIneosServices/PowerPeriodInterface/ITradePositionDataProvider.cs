namespace PowerPeriodInterface;

public interface ITradePositionDataProvider<T> where T : class
{
    /// <summary>
    /// Request is made using local date time.
    /// </summary>
    /// <param name="localDateTime"></param>
    /// <returns>Enumerable of <typeparamref name="T"/></returns>
    T GetTradePositions(DateTime localDateTime);

    /// <summary>
    /// Request is made using local date time.
    /// Async implementation.
    /// </summary>
    /// <param name="localDateTime"></param>
    /// <returns>Task of Enumerable of <typeparamref name="T"/></returns>
    Task<T> GetTradePositionsAsync(DateTime localDateTime);
}
