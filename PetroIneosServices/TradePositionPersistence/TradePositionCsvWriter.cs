using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerPeriodInterface;
using System.Text;

namespace TradePositionPersistence;

public class TradePositionCsvWriter : ITradePositionDataPersistence
{
    private readonly ILogger<TradePositionCsvWriter> _logger;
    private static string HEADER = "Local Time,Volume";
    private readonly string _csvPowerPositionFolder;
    private string _logKey = string.Empty;
    private IAggregatedPositionResult _position;

    public TradePositionCsvWriter(ILogger<TradePositionCsvWriter> logger, IConfiguration configuration)
    {
        _logger = logger;        
        _csvPowerPositionFolder = configuration["CsvPowerPositionPath"] ?? string.Empty;
        ValidatePath();
    }

    public async Task SaveAggregatedPositions(IAggregatedPositionResult position)
    {
        _position = position;
        _logKey = _position.Id.ToString();        

        LogInformation($"Saving aggregated positions for {_position}");

        var fileName = Path.Combine(_csvPowerPositionFolder, ConstructFileName(_position.RequestedDateTime));

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(HEADER);

        if (_position.Status != AggregatedTradePositionStatus.Failure)
        {
            if (_position.Errors != null && _position.Errors.Any())
                LogWarn($"{fileName} will have some Data Issues. Issues [{string.Join(" | ", _position.Errors)}]");

            LogInformation($"Saving {fileName} with {_position.TradePositions.Count} positions.");
            foreach (var kvp in _position.TradePositions)
            {
                builder.AppendLine($"{kvp.Key},{kvp.Value}");
            }
        }
        else
        {
            LogError($"Request {_position.RequestedDateTime} was not processed Successfully. Errors [{string.Join(" | ", _position.Errors)}]");
        }
        try
        {
            if (File.Exists(fileName))
            {
                var renameFileName = fileName.Replace(".csv", $"_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
                LogWarn($"File {fileName} already exists. Old file is Renamed to {renameFileName}.");
                File.Move(fileName, renameFileName);
            }
            await File.WriteAllTextAsync(fileName, builder.ToString());
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
            _logger.LogInformation($"{_logKey} : {message}");
    }

    private void LogWarn(string message)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning($"{_logKey} : {message}");
    }

    private void LogError(string message)
    {
        if (_logger.IsEnabled(LogLevel.Error))
            _logger.LogError($"{_logKey} : {message}");
    }

    private void LogDebug(string message)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"{_logKey} : {message}");
    }
    private void LogCritical(string message)
    {
        if (_logger.IsEnabled(LogLevel.Critical))
            _logger.LogCritical($"{_logKey} : {message}");
    }
}
