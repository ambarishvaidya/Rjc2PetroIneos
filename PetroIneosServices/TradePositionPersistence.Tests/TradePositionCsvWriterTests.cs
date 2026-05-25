
using Microsoft.Extensions.Logging;
using Moq;
using PowerPeriodInterface;

namespace TradePositionPersistence.Tests;

public class TestFileNameCreation
{
    Mock<ILogger<TradePositionCsvWriter>> _loggerMock;
    Mock<IAggregatedTradePosition> _positionMock;
    TradePositionCsvWriter _writer;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TradePositionCsvWriter>>();
        _positionMock = new Mock<IAggregatedTradePosition>();
        _writer = new TradePositionCsvWriter(_loggerMock.Object, _positionMock.Object);
    }

    [TestCase(2024, 12, 1, 12, 30, 59, "PowerPosition_20241201_1230.csv")]
    [TestCase(2024, 12, 15, 13, 2, 59, "PowerPosition_20241215_1302.csv")]
    public void ConstructFileName_WhenCalled_ReturnsExpectedFileName(int y, int M, int d, int H, int m, int s, string expectedFileName) 
    {
        DateTime dt = new DateTime(y, M, d, H, m, s);

        string actualFileName = _writer.ConstructFileName(dt);
        Assert.That(actualFileName, Is.EqualTo(expectedFileName));
    }
}
