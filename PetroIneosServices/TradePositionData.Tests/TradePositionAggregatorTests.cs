namespace TradePositionData.Tests;

public class DateTimeInputTests
{    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GetTradePositions_WhenDateIsInPast_ThrowArgumentException()
    {
        var agregator = new TradePositionAggregator();
        var pastDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => agregator.GetTradePositions(pastDate));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenDateIsInPast_ThrowArgumentException()
    {
        var agregator = new TradePositionAggregator();
        var pastDate = DateTime.Now.AddDays(-1);
        Assert.ThrowsAsync<ArgumentException>(async () => await agregator.GetTradePositionsAsync(pastDate));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    public void GetTradePositions_WhenDateIsWithinTolerance_DoesNotThrowArgumentException(int min)
    {
        var agregator = new TradePositionAggregator();
        var pastDate = DateTime.Now.AddMinutes(min);
        Assert.DoesNotThrow(() => agregator.GetTradePositions(pastDate));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    public async Task GetTradePositionsAsync_WhenDateIsWithinTolerance_DoesNotThrowArgumentException(int min)
    {
        var agregator = new TradePositionAggregator();
        var pastDate = DateTime.Now.AddMinutes(min);
        Assert.DoesNotThrowAsync(async () => await agregator.GetTradePositionsAsync(pastDate));
    }
}
