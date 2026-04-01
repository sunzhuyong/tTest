using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingSimulator.Services;
using TradingSimulator.Controllers;

namespace TradingSimulator;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // 检查是否是服务模式
        if (args.Length > 0 && args[0] == "--service")
        {
            RunAsService();
            return;
        }

        // 检查是否是 Web 模式
        if (args.Length > 0 && args[0] == "--web")
        {
            RunAsWeb();
            return;
        }

        // 默认 GUI 模式
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.MainForm());
    }

    /// <summary>
    /// Web 模式运行
    /// </summary>
    static void RunAsWeb()
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
        var autoTrader = new AutoTraderService(trading, market);

        builder.Services.AddSingleton(db);
        builder.Services.AddSingleton(trading);
        builder.Services.AddSingleton(market);
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
        Console.WriteLine("  Swagger: http://localhost:5000/swagger");
        Console.WriteLine("========================================");

        app.Run("http://localhost:5000");
    }

    /// <summary>
    /// 服务模式运行（Windows服务）
    /// </summary>
    static void RunAsService()
    {
        // 简化的服务模式，实际部署需要 Windows Service 安装
        RunAsWeb();
    }
}
