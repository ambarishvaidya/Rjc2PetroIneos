using PowerPeriodService;
using PowerPeriodInterface;
using TradePositionData;
using Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IPowerService, PowerService>();
builder.Services.AddTransient<ITradePositionDataProvider<IAggregatedTradePosition>, TradePositionAggregator>();

var host = builder.Build();
host.Run();
