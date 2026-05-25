using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using PowerPeriodInterface;
using Services;

namespace TradePositionData.Tests;

public class LoggingTests
{
    ITradePositionDataProvider<IAggregatedTradePosition> _aggregator;
    Mock<ILogger<TradePositionAggregator>> _logger;
    Mock<IPowerService> _powerServiceMock;

    [SetUp]
    public void Setup()
    {
        _powerServiceMock = new Mock<IPowerService>();
        _logger = new Mock<ILogger<TradePositionAggregator>>();
        _aggregator = new TradePositionAggregator(_powerServiceMock.Object, _logger.Object,
            TwoSWaitOneSecRetry.AsyncRetry, TwoSWaitOneSecRetry.SyncRetry);

        _logger.Setup(l => l.IsEnabled(LogLevel.Critical)).Returns(true);
        _logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        _logger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        _logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        _logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
    }
        
    [Test]
    public void GetTradePositions_WhenCriticalError_LogRecords()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => null)
            .Throws(() => new Exception());            
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        _logger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenCriticalError_LogRecords()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => null)
            .ReturnsAsync(() => null)
            .ReturnsAsync(() => null);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        _logger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}

public class RetryTests
{
    ITradePositionDataProvider<IAggregatedTradePosition> _aggregator;
    Mock<ILogger<TradePositionAggregator>> _logger;
    Mock<IPowerService> _powerServiceMock;

    [SetUp]
    public void Setup()
    {
        _powerServiceMock = new Mock<IPowerService>();
        _logger = new Mock<ILogger<TradePositionAggregator>>();
        _aggregator = new TradePositionAggregator(_powerServiceMock.Object, _logger.Object, 
            TwoSWaitOneSecRetry.AsyncRetry, TwoSWaitOneSecRetry.SyncRetry);
    }

    [Test]
    public void GetTradePositionsRetries_WhenPowerServiceReturnsNullOnce_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => null)
            .Returns(() => simglePowerTrade);
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsSingleResponse_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => null)
            .ReturnsAsync(() => simglePowerTrade);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public void GetTradePositionsRetries_WhenPowerServiceThrowsExceptionOnce_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Throws(new Exception())
            .Returns(() => simglePowerTrade);
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceThrowsExceptionOnce_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception())
            .ReturnsAsync(() => simglePowerTrade);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public void GetTradePositionsRetries_WhenPowerServiceReturnsEmptyOnce_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => Array.Empty<PowerTrade>())
            .Returns(() => simglePowerTrade);
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsEmptyOnce_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => Array.Empty<PowerTrade>())
            .ReturnsAsync(() => simglePowerTrade);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public void GetTradePositionsRetries_WhenPowerServiceReturnsEmptyTwice_ReturnAggregatedTradePositionWithError()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => Array.Empty<PowerTrade>())
            .Returns(() => Array.Empty<PowerTrade>());
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        Assert.That(obj.Status, Is.EqualTo(AggregatedTradePositionStatus.Failure));        
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsEmptyTwice_ReturnAggregatedTradePositionWithError()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => Array.Empty<PowerTrade>())
            .ReturnsAsync(() => Array.Empty<PowerTrade>());
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.Status, Is.EqualTo(AggregatedTradePositionStatus.Failure));
    }

    [Test]
    public void GetTradePositionsRetries_WhenPowerServiceReturnsExceptionTwice_ReturnAggregatedTradePositionWithError()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Throws(new Exception())
            .Throws(new Exception());
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        Assert.That(obj.Status, Is.EqualTo(AggregatedTradePositionStatus.Failure));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsExceptionTwice_ReturnAggregatedTradePositionWithError()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception())
            .ThrowsAsync(new Exception());
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.Status, Is.EqualTo(AggregatedTradePositionStatus.Failure));
    }

    [Test]
    public void GetTradePositionsRetries_WhenPowerServiceReturnsNullAndException_ReturnAggregatedTradePositionWithError()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => null)
            .Throws(new Exception());
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        Assert.That(obj.Status, Is.EqualTo(AggregatedTradePositionStatus.Failure));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsNullAndException_ReturnAggregatedTradePositionWithError()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.SetupSequence(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => null)
            .ThrowsAsync(new Exception());
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.Status, Is.EqualTo(AggregatedTradePositionStatus.Failure));
    }
}

public class ValidResponseTests
{
    ITradePositionDataProvider<IAggregatedTradePosition> _aggregator;
    Mock<ILogger<TradePositionAggregator>> _logger;
    Mock<IPowerService> _powerServiceMock;

    [SetUp]
    public void Setup()
    {
        _powerServiceMock = new Mock<IPowerService>();
        _logger = new Mock<ILogger<TradePositionAggregator>>();
        _aggregator = new TradePositionAggregator(_powerServiceMock.Object, _logger.Object, ZeroWaitOneMsRetry.AsyncRetry, ZeroWaitOneMsRetry.SyncRetry);
    }

    [Test]
    public void GetTradePositions_WhenPowerServiceReturnsSingleResponse_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.Setup(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => simglePowerTrade);
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsSingleResponse_ReturnAggregatedTradePositionWithSinglePosition()
    {
        var simglePowerTrade = new List<PowerTrade>() { PowerTrade.Create(DateTime.Now, 1) };
        _powerServiceMock.Setup(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => simglePowerTrade);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(1));
    }

    [Test]
    public void GetTradePositions_WhenPowerServiceReturnsMultipleResponse_ReturnAggregatedTradePosition()
    {
        var date = DateTime.Now;
        var multiPowerTrade = 
            new List<PowerTrade>() { PowerTrade.Create(date, 10), PowerTrade.Create(date, 10), PowerTrade.Create(date, 10) };
        _powerServiceMock.Setup(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => multiPowerTrade);
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(30));

    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsMultipleResponse_ReturnAggregatedTradePosition()
    {
        var date = DateTime.Now;
        var simglePowerTrade =
            new List<PowerTrade>() { PowerTrade.Create(date, 10), PowerTrade.Create(date, 10), PowerTrade.Create(date, 10) };
        _powerServiceMock.Setup(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => simglePowerTrade);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);
        //Assert.That(obj, Is.Not.Null);
        //Assert.That(obj.IsSuccessful, Is.True);
        Assert.That(obj.TradePositionCount, Is.EqualTo(30));
    }

    [Test]
    public async Task GetTradePositions_WhenPowerServiceReturnsMultipleResponse_VerifyResults()
    {
        var date = DateTime.Now;
        var multiPowerTrades =
            new List<PowerTrade>() { PowerTrade.Create(date, 10), PowerTrade.Create(date, 10), PowerTrade.Create(date, 10) };
        _powerServiceMock.Setup(s => s.GetTrades(It.IsAny<DateTime>()))
            .Returns(() => multiPowerTrades);
        var obj = _aggregator.GetTradePositions(DateTime.Now);
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj.TradePositions.Count, Is.EqualTo(10));
        Assert.That(obj.TradePositions.Keys.Contains("23:00"), Is.True);
        Assert.That(obj.TradePositions.Keys.Contains("00:00"), Is.True);
    }

    [Test]
    public async Task GetTradePositionsAsync_WhenPowerServiceReturnsMultipleResponse_VerifyResults()
    {
        var date = DateTime.Now;
        var simglePowerTrade =
            new List<PowerTrade>() { PowerTrade.Create(date, 10), PowerTrade.Create(date, 10), PowerTrade.Create(date, 10) };
        _powerServiceMock.Setup(s => s.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => simglePowerTrade);
        var obj = await _aggregator.GetTradePositionsAsync(DateTime.Now);        
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj.TradePositions.Count, Is.EqualTo(10));
        Assert.That(obj.TradePositions.Keys.Contains("23:00"), Is.True);
        Assert.That(obj.TradePositions.Keys.Contains("00:00"), Is.True);
    }
}
public class InvalidResponseTests
{
    ITradePositionDataProvider<IAggregatedTradePosition> _aggregator;
    Mock<ILogger<TradePositionAggregator>> _logger;
    Mock<IPowerService> _powerServiceMock;

    [SetUp]
    public void Setup()
    {
        _powerServiceMock = new Mock<IPowerService>();
        _logger = new Mock<ILogger<TradePositionAggregator>>();
        _aggregator = new TradePositionAggregator(_powerServiceMock.Object, _logger.Object, ZeroWaitOneMsRetry.AsyncRetry, ZeroWaitOneMsRetry.SyncRetry);
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
    ITradePositionDataProvider<IAggregatedTradePosition> _aggregator;
    Mock<ILogger<TradePositionAggregator>> _logger;

    [SetUp]
    public void Setup()
    {
        var powerServiceMock = new Mock<IPowerService>();
        _logger = new Mock<ILogger<TradePositionAggregator>>();
        _aggregator = new TradePositionAggregator(powerServiceMock.Object, _logger.Object, 
            ZeroWaitOneMsRetry.AsyncRetry, ZeroWaitOneMsRetry.SyncRetry);
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

internal static class ZeroWaitOneMsRetry
{
    private static ISyncPolicy<IEnumerable<PowerTrade>> retryExceptions = Policy<IEnumerable<PowerTrade>>
        .Handle<Exception>()
        .WaitAndRetry(
            0,
            attempt => TimeSpan.FromMicroseconds(1)
        );

    private static ISyncPolicy<IEnumerable<PowerTrade>> retryEmptyOrNull = Policy<IEnumerable<PowerTrade>>
        .HandleResult(r => r == null || !r.Any())
        .WaitAndRetry(
            0,
            attempt => TimeSpan.FromMicroseconds(1)
        );

    internal static ISyncPolicy<IEnumerable<PowerTrade>> SyncRetry = Policy.Wrap(retryExceptions, retryEmptyOrNull);

    private static IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetryExceptions = Policy<IEnumerable<PowerTrade>>
        .Handle<Exception>()
        .WaitAndRetryAsync(
            0,
            attempt => TimeSpan.FromMicroseconds(1)
        );

    private static IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetryEmptyOrNull = Policy<IEnumerable<PowerTrade>>
        .HandleResult(r => r == null || !r.Any())
        .WaitAndRetryAsync(
            0,
            attempt => TimeSpan.FromMicroseconds(1)
        );

    internal static IAsyncPolicy<IEnumerable<PowerTrade>> AsyncRetry = Policy.WrapAsync(asyncRetryExceptions, asyncRetryEmptyOrNull);

}

internal static class TwoSWaitOneSecRetry
{
    private static ISyncPolicy<IEnumerable<PowerTrade>> retryExceptions = Policy<IEnumerable<PowerTrade>>
        .Handle<Exception>()
        .WaitAndRetry(
            2,
            attempt => TimeSpan.FromSeconds(1)
        );

    private static ISyncPolicy<IEnumerable<PowerTrade>> retryEmptyOrNull = Policy<IEnumerable<PowerTrade>>
        .HandleResult(r => r == null || !r.Any())
        .WaitAndRetry(
            2,
            attempt => TimeSpan.FromSeconds(1)
        );

    internal static ISyncPolicy<IEnumerable<PowerTrade>> SyncRetry = Policy.Wrap(retryExceptions, retryEmptyOrNull);

    private static IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetryExceptions = Policy<IEnumerable<PowerTrade>>
        .Handle<Exception>()
        .WaitAndRetryAsync(
            2,
            attempt => TimeSpan.FromSeconds(1)
        );

    private static IAsyncPolicy<IEnumerable<PowerTrade>> asyncRetryEmptyOrNull = Policy<IEnumerable<PowerTrade>>
        .HandleResult(r => r == null || !r.Any())
        .WaitAndRetryAsync(
            2,
            attempt => TimeSpan.FromSeconds(1)
        );

    internal static IAsyncPolicy<IEnumerable<PowerTrade>> AsyncRetry = Policy.WrapAsync(asyncRetryExceptions, asyncRetryEmptyOrNull);

}