using PowerPeriodService;
using PowerPeriodInterface;
using TradePositionData;
using Services;
using TradePositionPersistence;
using System.IO.Abstractions;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IPowerService, PowerService>();
builder.Services.AddTransient<ITradePositionDataProvider<IAggregatedPositionResult>, TradePositionAggregator>();
builder.Services.AddTransient<ITradePositionDataPersistence, TradePositionCsvWriter>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();

var host = builder.Build();
host.Run();
