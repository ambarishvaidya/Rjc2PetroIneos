using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using PowerPeriodInterface;
using System.IO.Abstractions;
using System.Text;

namespace TradePositionPersistence;

public class TradePositionCsvWriter : ITradePositionDataPersistence
{
    private readonly ILogger<TradePositionCsvWriter> _logger;
    private readonly IFileSystem _fileSystem;
    private static string HEADER = "Local Time,Volume";
    private readonly string _csvPowerPositionFolder;
    private readonly IAsyncPolicy _asyncPolicy;


    public TradePositionCsvWriter(ILogger<TradePositionCsvWriter> logger, IConfiguration configuration, IFileSystem fileSystem)
        : this(logger, configuration, fileSystem, RetryPolicy.asyncRetryIOException)
    { }

    internal TradePositionCsvWriter(ILogger<TradePositionCsvWriter> logger, IConfiguration configuration, IFileSystem fileSystem, 
        IAsyncPolicy asyncPolicy)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _csvPowerPositionFolder = configuration["CsvPowerPositionPath"] ?? string.Empty;
        _asyncPolicy = asyncPolicy;
        ValidatePath();
    }

    public async Task SaveAggregatedPositions(IAggregatedPositionResult position, CancellationToken cancellationToken)
    {
        LogInformation($"Saving aggregated positions for {position}");

        var fileName = Path.Combine(_csvPowerPositionFolder, ConstructFileName(position.RequestedDateTime));

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(HEADER);

        if (position.Status != AggregatedTradePositionStatus.Failure)
        {
            if (position.Errors != null && position.Errors.Any())
                LogWarn($"{fileName} will have some Data Issues. Issues [{string.Join(" | ", position.Errors)}]");

            LogInformation($"Saving {fileName} with {position.TradePositions.Count} positions.");
            foreach (var kvp in position.TradePositions)
            {
                builder.AppendLine($"{kvp.Key},{kvp.Value}");
            }
        }
        else
        {
            LogError($"Request {position.RequestedDateTime} was not processed Successfully. Errors [{string.Join(" | ", position.Errors)}]");
        }
        try
        {
            if (_fileSystem.File.Exists(fileName))
            {
                var renameFileName = fileName.Replace(".csv", $"_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
                LogWarn($"File {fileName} already exists. Old file is Renamed to {renameFileName}.");
                _fileSystem.File.Move(fileName, renameFileName);
            }
            await _asyncPolicy.ExecuteAsync(() => _fileSystem.File.WriteAllTextAsync(fileName, builder.ToString(), cancellationToken));
        }
        catch(IOException ex)
        {
            LogCritical($"Failed to save file {fileName} due to IO error. Exception: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            LogCritical($"Failed to save file {fileName}. Access is denied. Exception: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogError($"Failed to save file {fileName}. Operation was canceled. Exception: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to save file {fileName}. Exception: {ex.Message}");
        }
    }

    internal void ValidatePath()
    {
        if (string.IsNullOrEmpty(_csvPowerPositionFolder))
        {
            LogCritical("CSV Power Position path is not configured.");
            throw new ArgumentException("CSV Power Position path is not configured.");
        }

        if (!Path.Exists(_csvPowerPositionFolder))
        {
            LogCritical($"Path {_csvPowerPositionFolder} does not exist. ");
            throw new ArgumentException($"Path {_csvPowerPositionFolder} does not exist.");
        }
    }

    internal string ConstructFileName(DateTime dt)
    {
        return $"PowerPosition_{dt.ToString("yyyyMMdd")}_{dt.ToString("HHmm")}.csv";
    }

    private void LogInformation(string message)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation($"{message}");
    }

    private void LogWarn(string message)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning($"{message}");
    }

    private void LogError(string message)
    {
        if (_logger.IsEnabled(LogLevel.Error))
            _logger.LogError($"{message}");
    }

    private void LogDebug(string message)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"{message}");
    }
    private void LogCritical(string message)
    {
        if (_logger.IsEnabled(LogLevel.Critical))
            _logger.LogCritical($"{message}");
    }
}

internal static class RetryPolicy
{
    public static IAsyncPolicy asyncRetryIOException = Policy
        .Handle<IOException>()
        .WaitAndRetryAsync(
            3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
        );       

}