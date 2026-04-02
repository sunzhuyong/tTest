using Microsoft.AspNetCore.Mvc;
using TradingSimulator.Models;
using TradingSimulator.Services;

namespace TradingSimulator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingController : ControllerBase
{
    private readonly TradingService _tradingService;
    private readonly MarketDataService _marketService;
    private readonly AutoTraderService _autoTrader;
    private readonly DatabaseService _db;
    private readonly FeishuNotifyService _feishuNotify;
    private readonly DailyReviewService _reviewService;
    private readonly StrategyIterationService _strategyIterationService;
    private readonly MarketSummaryService _marketSummaryService;

    public TradingController(
        TradingService tradingService,
        MarketDataService marketService,
        AutoTraderService autoTrader,
        DatabaseService db,
        FeishuNotifyService feishuNotify,
        DailyReviewService reviewService,
        StrategyIterationService strategyIterationService,
        MarketSummaryService marketSummaryService)
    {
        _tradingService = tradingService;
        _marketService = marketService;
        _autoTrader = autoTrader;
        _db = db;
        _feishuNotify = feishuNotify;
        _reviewService = reviewService;
        _strategyIterationService = strategyIterationService;
        _marketSummaryService = marketSummaryService;
    }

    /// <summary>
    /// 获取账户信息
    /// </summary>
    [HttpGet("account")]
    public ActionResult<Account> GetAccount()
    {
        var account = _tradingService.GetAccount();
        return Ok(account);
    }

    /// <summary>
    /// 获取持仓列表
    /// </summary>
    [HttpGet("positions")]
    public ActionResult<List<Position>> GetPositions()
    {
        var positions = _tradingService.GetPositions();
        return Ok(positions);
    }

    /// <summary>
    /// 获取交易记录
    /// </summary>
    [HttpGet("trades")]
    public ActionResult<List<TradeRecord>> GetTrades([FromQuery] int limit = 50)
    {
        var trades = _tradingService.GetTradeRecords();
        return Ok(trades.TakeLast(limit).Reverse().ToList());
    }

    /// <summary>
    /// 获取日志
    /// </summary>
    [HttpGet("logs")]
    public ActionResult<List<string>> GetLogs([FromQuery] int limit = 100)
    {
        var logs = _autoTrader.GetLogs();
        return Ok(logs.Take(limit).ToList());
    }

    /// <summary>
    /// 获取自动交易状态
    /// </summary>
    [HttpGet("status")]
    public ActionResult<object> GetStatus()
    {
        var account = _tradingService.GetAccount();
        var positions = _tradingService.GetPositions();
        var trades = _tradingService.GetTradeRecords();

        return Ok(new
        {
            AutoTraderRunning = _autoTrader.IsRunning,
            Account = new
            {
                account.InitialCapital,
                account.AvailableCash,
                account.MarketValue,
                account.TotalAssets,
                account.TotalProfitLoss,
                account.ProfitLossPercent
            },
            PositionCount = positions.Count,
            TradeCount = trades.Count,
            StockWatchList = _autoTrader.GetStockWatchList(),
            FundWatchList = _autoTrader.GetFundWatchList()
        });
    }

    /// <summary>
    /// 手动买入
    /// </summary>
    [HttpPost("buy")]
    public async Task<ActionResult<object>> Buy([FromBody] TradeRequest request)
    {
        var quote = _marketService.GetStockQuoteAsync(request.Code).Result;
        if (quote == null)
            return NotFound(new { success = false, message = "股票代码不存在" });

        var result = _tradingService.Buy(request.Code, quote.Name, SecurityType.Stock, request.Quantity, quote.CurrentPrice);
        if (result.Success)
        {
            var account = _tradingService.GetAccount();
            _ = _feishuNotify.SendBuyNotification(
                request.Code, quote.Name, (int)request.Quantity, quote.CurrentPrice, (int)request.Quantity * quote.CurrentPrice,
                "手动买入", 100,
                account.AvailableCash, account.TotalAssets, account.TotalProfitLoss);
        }
        return Ok(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// 手动卖出
    /// </summary>
    [HttpPost("sell")]
    public async Task<ActionResult<object>> Sell([FromBody] SellRequest request)
    {
        // 获取当前价格
        var quote = await _marketService.GetStockQuoteAsync(request.Code);
        if (quote == null)
            return NotFound(new { success = false, message = "股票代码不存在" });

        var result = _tradingService.Sell(request.Code, request.Quantity, quote.CurrentPrice);
        if (result.Success)
        {
            var account = _tradingService.GetAccount();
            _ = _feishuNotify.SendSellNotification(
                request.Code, quote.Name, (int)request.Quantity, quote.CurrentPrice, (int)request.Quantity * quote.CurrentPrice,
                "手动卖出", 0,
                account.AvailableCash, account.TotalAssets, account.TotalProfitLoss);
        }
        return Ok(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// 启动自动交易
    /// </summary>
    [HttpPost("auto/start")]
    public ActionResult<object> StartAutoTrader()
    {
        _autoTrader.Start();
        return Ok(new { success = true, message = "自动交易已启动" });
    }

    /// <summary>
    /// 停止自动交易
    /// </summary>
    [HttpPost("auto/stop")]
    public ActionResult<object> StopAutoTrader()
    {
        _autoTrader.Stop();
        return Ok(new { success = true, message = "自动交易已停止" });
    }

    /// <summary>
    /// 手动触发扫描
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult<object>> ScanMarket()
    {
        await _autoTrader.ScanAndTradeAsync();
        return Ok(new { success = true, message = "扫描完成" });
    }

    /// <summary>
    /// 添加自选股
    /// </summary>
    [HttpPost("watch/stock")]
    public ActionResult<object> AddStockWatch([FromBody] string code)
    {
        _autoTrader.AddStock(code);
        return Ok(new { success = true, message = $"已添加 {code}" });
    }

    /// <summary>
    /// 添加自选基
    /// </summary>
    [HttpPost("watch/fund")]
    public ActionResult<object> AddFundWatch([FromBody] string code)
    {
        _autoTrader.AddFund(code);
        return Ok(new { success = true, message = $"已添加 {code}" });
    }

    /// <summary>
    /// 获取实时行情（模拟）
    /// </summary>
    [HttpGet("quote/{code}")]
    public async Task<ActionResult<object>> GetQuote(string code)
    {
        var quote = await _marketService.GetStockQuoteAsync(code);
        if (quote == null)
            return NotFound(new { message = "股票不存在" });

        return Ok(new
        {
            code = quote.Code,
            name = quote.Name,
            price = quote.CurrentPrice,
            change = quote.ChangePercent
        });
    }

    /// <summary>
    /// 复盘数据
    /// </summary>
    [HttpGet("review")]
    public ActionResult<object> GetReview()
    {
        var trades = _tradingService.GetTradeRecords();
        var positions = _tradingService.GetPositions();
        var account = _tradingService.GetAccount();

        // 获取历史复盘记录
        var reviewHistory = _reviewService.GetReviewHistory(30);

        // 按日期统计
        var dailyTrades = trades
            .GroupBy(t => t.TradeTime.Date)
            .OrderByDescending(g => g.Key)
            .Take(30)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count(),
                BuyCount = g.Count(t => t.Direction == TradeDirection.Buy),
                SellCount = g.Count(t => t.Direction == TradeDirection.Sell),
                TotalAmount = g.Sum(t => t.Amount)
            })
            .ToList();

        return Ok(new
        {
            TotalTrades = trades.Count,
            TotalProfitLoss = account.TotalProfitLoss,
            ProfitLossPercent = account.ProfitLossPercent,
            CurrentPositions = positions.Count,
            DailyTrades = dailyTrades,
            ReviewHistory = reviewHistory.Select(r => new
            {
                r.Date,
                r.TotalAssets,
                r.TotalProfitLoss,
                r.ProfitLossPercent,
                r.TradeCount,
                r.TodayProfitLoss,
                r.PositionCount,
                r.WinningPositions,
                r.LosingPositions,
                r.TopWinner,
                r.TopLoser,
                r.StrategySuggestions
            }).ToList()
        });
    }

    /// <summary>
    /// 手动触发复盘
    /// </summary>
    [HttpPost("review/execute")]
    public async Task<ActionResult<object>> ExecuteReview()
    {
        var review = await _reviewService.ExecuteDailyReviewAsync();
        return Ok(new { success = true, message = "复盘完成", review });
    }

    /// <summary>
    /// 获取策略迭代历史
    /// </summary>
    [HttpGet("strategy/history")]
    public ActionResult<object> GetStrategyHistory([FromQuery] int days = 90)
    {
        var history = _strategyIterationService.GetHistory(days);
        var latest = _strategyIterationService.GetLatest();

        return Ok(new
        {
            History = history.Select(h => new
            {
                h.Date,
                h.Type,
                h.BeforeParams,
                h.AfterParams,
                h.Reason,
                h.Result,
                h.CreatedAt
            }).ToList(),
            Latest = latest != null ? new
            {
                latest.Type,
                latest.Reason,
                latest.Result
            } : null
        });
    }

    /// <summary>
    /// 手动记录策略调整
    /// </summary>
    [HttpPost("strategy/record")]
    public ActionResult<object> RecordStrategy([FromBody] StrategyRecordRequest request)
    {
        _strategyIterationService.RecordIteration(
            request.Type,
            request.BeforeParams,
            request.AfterParams,
            request.Reason,
            request.Result);

        return Ok(new { success = true, message = "策略调整已记录" });
    }

    /// <summary>
    /// 获取市场总结
    /// </summary>
    [HttpGet("market/summary")]
    public async Task<ActionResult<object>> GetMarketSummary([FromQuery] bool isMorning = false)
    {
        var summary = await _marketSummaryService.GenerateSummaryAsync(isMorning);
        return Ok(summary);
    }

    /// <summary>
    /// 手动发送市场总结
    /// </summary>
    [HttpPost("market/summary/send")]
    public async Task<ActionResult<object>> SendMarketSummary([FromQuery] bool isMorning = false)
    {
        var summary = await _marketSummaryService.GenerateSummaryAsync(isMorning);
        await _feishuNotify.SendMessage(
            isMorning ? "【午间市场总结】" : "【收盘市场总结】",
            summary.Summary);
        return Ok(new { success = true, message = "市场总结已发送", summary });
    }
}

public class TradeRequest
{
    public string Code { get; set; } = "";
    public int Quantity { get; set; }
}

public class SellRequest
{
    public string Code { get; set; } = "";
    public decimal Quantity { get; set; }
}

public class StrategyRecordRequest
{
    public string Type { get; set; } = "";
    public object BeforeParams { get; set; } = new { };
    public object AfterParams { get; set; } = new { };
    public string Reason { get; set; } = "";
    public string? Result { get; set; }
}
