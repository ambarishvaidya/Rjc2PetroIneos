using Moq;
using PowerPeriodInterface;
using Services;

namespace TradePositionData.Tests;

public class InvalidResponseTests
{
    TradePositionAggregator _aggregator;
    Mock<IPowerService> _powerServiceMock;

    [SetUp]
    public void Setup()
    {
        _powerServiceMock = new Mock<IPowerService>();                        
        _aggregator = new TradePositionAggregator(_powerServiceMock.Object);
    }

    [Test]
    public void GetTradePositions_WhenPowerServiceThrowsException_ReturnAggregatedTradePositionWithException()
    {
        _powerServiceMock.Setup(s => s.GetTrades(It.IsAny<DateTime>())).Throws<Exception>();
        IAggregatedTradePosition obj = _aggregator.GetTradePositions(DateTime.Now);
        Assert.That(obj, Is.Not.Null);
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceThrowsException_ReturnAggregatedTradePositionWithException()
    {
        _powerServiceMock.Setup(s => s.GetTradesAsync(It.IsAny<DateTime>())).Throws<Exception>();
        IAggregatedTradePosition obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        Assert.That(obj, Is.Not.Null);
    }

    [Test]
    public void GetTradePositions_WhenPowerServiceReturnsNull_ReturnAggregatedTradePositionWithException()
    {
        _powerServiceMock.Setup(s => s.GetTrades(It.IsAny<DateTime>())).Returns(() => null);
        IAggregatedTradePosition obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.Errors, Is.Not.Null);
        //Assert.That(obj.Errors.Count, Is.EqualTo(1));
        Assert.That(obj.Errors.First(), Is.EqualTo("Received null response from PowerService."));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsNull_ReturnAggregatedTradePositionWithException()
    {
        _powerServiceMock.Setup(s => s.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync(() => null);
        IAggregatedTradePosition obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.Errors, Is.Not.Null);
        //Assert.That(obj.Errors.Count, Is.EqualTo(1));
        Assert.That(obj.Errors.First(), Is.EqualTo("Received null response from PowerService."));
    }

    [Test]
    public void GetTradePositions_WhenPowerServiceReturnsEmpty_ReturnAggregatedTradePositionWithException()
    {
        _powerServiceMock.Setup(s => s.GetTrades(It.IsAny<DateTime>())).Returns(() => Array.Empty<PowerTrade>());
        IAggregatedTradePosition obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.Errors, Is.Not.Null);
        //Assert.That(obj.Errors.Count, Is.EqualTo(1));
        Assert.That(obj.Errors.First(), Is.EqualTo("Received empty response from PowerService."));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsEmpty_ReturnAggregatedTradePositionWithException()
    {
        _powerServiceMock.Setup(s => s.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync(() => Array.Empty<PowerTrade>());
        IAggregatedTradePosition obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.Errors, Is.Not.Null);
        //Assert.That(obj.Errors.Count, Is.EqualTo(1));
        Assert.That(obj.Errors.First(), Is.EqualTo("Received empty response from PowerService."));
    }
}

public class DateTimeInputTests
{   
    TradePositionAggregator _aggregator;

    [SetUp]
    public void Setup()
    {
        var powerServiceMock = new Mock<IPowerService>();
        _aggregator = new TradePositionAggregator(powerServiceMock.Object);
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
