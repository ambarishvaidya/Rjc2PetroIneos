using PowerPeriodService;
using PowerPeriodInterface;
using TradePositionData;
using Services;
using TradePositionPersistence;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IPowerService, PowerService>();
builder.Services.AddTransient<ITradePositionDataProvider<IAggregatedPositionResult>, TradePositionAggregator>();
builder.Services.AddTransient<ITradePositionDataPersistence, TradePositionCsvWriter>();

var host = builder.Build();
host.Run();
