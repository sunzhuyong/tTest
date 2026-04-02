using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingSimulator.Services;
using TradingSimulator.Controllers;

namespace TradingSimulator;

static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();

        // 添加服务
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // 添加业务服务
        var db = new DatabaseService();
        var market = new MarketDataService();
        var trading = new TradingService(db, market);
        var feishuNotify = new FeishuNotifyService();
        var reviewService = new DailyReviewService(trading, market, feishuNotify);
        var strategyIterationService = new StrategyIterationService();
        var marketSummaryService = new MarketSummaryService(market);
        var autoTrader = new AutoTraderService(trading, market, feishuNotify);
        autoTrader.SetReviewService(reviewService);
        autoTrader.SetMarketSummaryService(marketSummaryService);

        builder.Services.AddSingleton(db);
        builder.Services.AddSingleton(trading);
        builder.Services.AddSingleton(market);
        builder.Services.AddSingleton(feishuNotify);
        builder.Services.AddSingleton(reviewService);
        builder.Services.AddSingleton(strategyIterationService);
        builder.Services.AddSingleton(marketSummaryService);
        builder.Services.AddSingleton(autoTrader);

        var app = builder.Build();

        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.MapRazorPages();

        // 启动自动交易
        autoTrader.Start();

        Console.WriteLine("========================================");
        Console.WriteLine("  模拟炒股 Web 服务启动成功");
        Console.WriteLine("  API: http://localhost:5000/api/trading");
        Console.WriteLine("  Web界面: http://localhost:5000/Web/");
        Console.WriteLine("  Swagger: http://localhost:5000/swagger");
        Console.WriteLine("========================================");

        app.Run("http://localhost:5000");
    }
}