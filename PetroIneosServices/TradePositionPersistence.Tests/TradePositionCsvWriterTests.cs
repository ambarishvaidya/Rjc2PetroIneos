
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PowerPeriodInterface;
using System.IO.Abstractions;

namespace TradePositionPersistence.Tests;

public class TestSaveAggregatedPositions
{
    Mock<ILogger<TradePositionCsvWriter>> _loggerMock;
    Mock<IAggregatedPositionResult> _positionMock;
    ITradePositionDataPersistence _dataPersistence;
    TradePositionCsvWriter _csvWriter;
    IConfiguration _configuration;
    CancellationToken _cancellationToken;

    IFileSystem _fileSystem;

    string _path;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TradePositionCsvWriter>>();
        _path = $@"C:\AggregatedPositionsTestSuite_{DateTime.Now:yyyyMMdd_HHmmss}";
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CsvPowerPositionPath", _path }
            })
            .Build();
        
        _fileSystem = new FileSystem();

        _positionMock = new Mock<IAggregatedPositionResult>();
        _positionMock.SetupGet(p => p.Id).Returns(Guid.NewGuid());        
        _positionMock.SetupGet(p => p.Errors).Returns(new List<string> { "Error1", "Error2" });

        _cancellationToken = CancellationToken.None;
    }

    [TearDown]
    public void TearDown()
    {
        if(Directory.Exists(_path))
        {
            Directory.Delete(_path, true);
        }
    }

    [Test]
    public async Task SaveAggregatedPositions_WhenCalledWithFailureStatus_WritesAnEmptyCsvFileWithHeaders()
    {
        _positionMock.SetupGet(p => p.RequestedDateTime).Returns(new DateTime(2024, 12, 1, 12, 30, 59));
        _positionMock.SetupGet(p => p.Status).Returns(AggregatedTradePositionStatus.Failure);

        _csvWriter = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);
        _dataPersistence = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);

        var fileName = _csvWriter.ConstructFileName(_positionMock.Object.RequestedDateTime);

        await _dataPersistence.SaveAggregatedPositions(_positionMock.Object, _cancellationToken);

        Assert.That(_fileSystem.File.Exists(Path.Combine(_path, fileName)), Is.True);
    }

    [Test]
    public async Task SaveAggregatedPositions_WithFileNameExists_RenamesExistingFileAndWritesNewFile()
    {
        _positionMock.SetupGet(p => p.RequestedDateTime).Returns(new DateTime(2024, 12, 20, 15, 03, 59));
        _positionMock.SetupGet(p => p.Status).Returns(AggregatedTradePositionStatus.Success);
        _positionMock.SetupGet(p => p.TradePositions).Returns(new Dictionary<string, double>
        {
            { "15:00", 100 },
            { "16:00", 150 }
        });

        _csvWriter = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);
        _dataPersistence = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);

        var fileName = _csvWriter.ConstructFileName(_positionMock.Object.RequestedDateTime);
        _fileSystem.File.WriteAllText(Path.Combine(_path, fileName), "Existing file content");

        await _dataPersistence.SaveAggregatedPositions(_positionMock.Object, _cancellationToken);

        Assert.That(_fileSystem.Directory.GetFiles(_path).Length, Is.EqualTo(2));
        Assert.That(_fileSystem.Directory.GetFiles(_path).
            All(_ => Path.GetFileName(_).StartsWith(Path.GetFileNameWithoutExtension(fileName))), Is.True);
    }

    [Test]
    public async Task SaveAggregatedPositions_WhenCalledWithSuccessStatus_WritesCsvFileWithData()
    {
        _positionMock.SetupGet(p => p.RequestedDateTime).Returns(new DateTime(2024, 12, 20, 15, 03, 59));
        _positionMock.SetupGet(p => p.Status).Returns(AggregatedTradePositionStatus.Success);
        _positionMock.SetupGet(p => p.TradePositions).Returns(new Dictionary<string, double>
        {
            { "15:00", 100 },
            { "16:00", 150 }
        });

        _dataPersistence = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);

        _csvWriter = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);
        var fileName = _csvWriter.ConstructFileName(_positionMock.Object.RequestedDateTime);
        
        await _dataPersistence.SaveAggregatedPositions(_positionMock.Object, _cancellationToken);

        string[] lines = _fileSystem.File.ReadAllLines(Path.Combine(_path, fileName));

        Assert.That(_fileSystem.Directory.GetFiles(_path).Length, Is.EqualTo(1));
        Assert.That(lines.Any(_ => _.Contains("15:00,100")), Is.True);
        Assert.That(lines.Any(_ => _.Contains("16:00,150")), Is.True);
    }

    [TestCase(AggregatedTradePositionStatus.Success)]
    [TestCase(AggregatedTradePositionStatus.Failure)]
    [TestCase(AggregatedTradePositionStatus.SuccessWithErrors)]
    public async Task SaveAggregatedPositions_WhenCalledWithAnyStatus_WritesCsvFileWithHeader(AggregatedTradePositionStatus status)
    {
        _positionMock.SetupGet(p => p.RequestedDateTime).Returns(new DateTime(2024, 12, 20, 15, 03, 59));
        _positionMock.SetupGet(p => p.Status).Returns(status);
        _positionMock.SetupGet(p => p.TradePositions).Returns(new Dictionary<string, double>());

        _dataPersistence = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);

        _csvWriter = new TradePositionCsvWriter(_loggerMock.Object, _configuration, _fileSystem);
        var fileName = _csvWriter.ConstructFileName(_positionMock.Object.RequestedDateTime);
        
        await _dataPersistence.SaveAggregatedPositions(_positionMock.Object, _cancellationToken);

        string[] lines = _fileSystem.File.ReadAllLines(Path.Combine(_path, fileName));

        Assert.That(_fileSystem.Directory.GetFiles(_path).Length, Is.EqualTo(1));
        Assert.That(lines.Length, Is.EqualTo(1));
        Assert.That(lines.Any(_ => _.Contains("Local Time,Volume")), Is.True);        
    }
}
public class TestCsvFolderPath
{
    Mock<ILogger<TradePositionCsvWriter>> _loggerMock;
    CancellationToken _cancellationToken;
    IFileSystem _fileSystem;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TradePositionCsvWriter>>();  
        _cancellationToken = CancellationToken.None;
        _fileSystem = new FileSystem();
    }

    [TestCase(null)]
    [TestCase("Abc")]
    [TestCase("")]
    [TestCase(@"G:\Dopamine")]
    public void Constructor_WhenCalledWithInvalidInputs_ThrowsArgumentException(string? invalidPath)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CsvPowerPositionPath", invalidPath }
            })
            .Build();
        
        Assert.Throws<ArgumentException>(() => new TradePositionCsvWriter(_loggerMock.Object, configuration, _fileSystem));
    }
}

public class TestFileNameCreation
{
    Mock<ILogger<TradePositionCsvWriter>> _loggerMock;    
    TradePositionCsvWriter _writer;
    CancellationToken _cancellationToken;
    IFileSystem _fileSystem;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TradePositionCsvWriter>>();        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CsvPowerPositionPath", "C:\\Temp" }
            })
            .Build();

        _cancellationToken = CancellationToken.None;

        _fileSystem = new FileSystem();

        _writer = new TradePositionCsvWriter(_loggerMock.Object, configuration, _fileSystem);
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
