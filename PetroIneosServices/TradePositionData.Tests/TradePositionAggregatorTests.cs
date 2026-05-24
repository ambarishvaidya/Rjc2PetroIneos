namespace TradePositionData.Tests;

public class DateTimeInputTests
{   
    TradePositionAggregator _aggregator;

    [SetUp]
    public void Setup()
    {
        _aggregator = new TradePositionAggregator();
    }

    [Test]
    public void GetTradePositions_WhenDateIsInPast_ThrowArgumentException()
    {        
        var pastDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => _aggregator.GetTradePositions(pastDate));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenDateIsInPast_ThrowArgumentException()
    {
        var pastDate = DateTime.Now.AddDays(-1);
        Assert.ThrowsAsync<ArgumentException>(async () => await _aggregator.GetTradePositionsAsync(pastDate));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    public void GetTradePositions_WhenDateIsWithinTolerance_DoesNotThrowArgumentException(int min)
    {
        var pastDate = DateTime.Now.AddMinutes(min);
        Assert.DoesNotThrow(() => _aggregator.GetTradePositions(pastDate));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    public async Task GetTradePositionsAsync_WhenDateIsWithinTolerance_DoesNotThrowArgumentException(int min)
    {
        var pastDate = DateTime.Now.AddMinutes(min);
        Assert.DoesNotThrowAsync(async () => await _aggregator.GetTradePositionsAsync(pastDate));
    }


    [Test]
    public void GetTradePositions_WhenDateKindIsNotLocal_ThrowArgumentException()
    {
        var pastDate = DateTime.UtcNow;
        Assert.Throws<ArgumentException>(() => _aggregator.GetTradePositions(pastDate));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenDateKindIsNotLocal_ThrowArgumentException()
    {
        var pastDate = DateTime.UtcNow;
        Assert.ThrowsAsync<ArgumentException>(async () => await _aggregator.GetTradePositionsAsync(pastDate));
    }
}
