using TradingSimulator.Models;

namespace TradingSimulator.Services;

/// <summary>
/// 自动交易引擎
/// </summary>
public class AutoTraderService
{
    private readonly TradingService _tradingService;
    private readonly MarketDataService _marketService;
    private readonly List<BaseStrategy> _strategies;
    private System.Windows.Forms.Timer? _timer;
    private System.Windows.Forms.Timer? _fundTimer;
    private bool _isRunning;
    public bool IsRunning => _isRunning;
    private readonly List<string> _tradeLogs = new();

    // 自选股列表
    private readonly List<string> _stockWatchList = new()
    {
        "600000", // 浦发银行
        "600036", // 招商银行
        "600519", // 贵州茅台
        "000001", // 平安银行
        "000858", // 五粮液
        "300750", // 宁德时代
        "601318", // 中国平安
        "601888"  // 中国中免
    };

    // 自选基金列表
    private readonly List<string> _fundWatchList = new()
    {
        "161039", // 易方达上证50ETF
        "510300", // 华泰柏瑞沪深300ETF
        "159915", // 易方达创业板ETF
        "161725", // 招商中证白酒指数
        "110022"  // 易方达消费行业股票
    };

    private readonly FeishuNotifyService _feishuNotify;
    private DailyReviewService? _reviewService;
    private System.Windows.Forms.Timer? _reviewTimer;

    public event Action<string>? OnTradeExecuted;
    public event Action<string>? OnLogUpdated;

    public AutoTraderService(TradingService tradingService, MarketDataService marketService, FeishuNotifyService feishuNotify)
    {
        _tradingService = tradingService;
        _marketService = marketService;
        _feishuNotify = feishuNotify;
        _strategies = new List<BaseStrategy>
        {
            new StockMidTermStrategy(),
            new FundMidTermStrategy()
        };
    }

    /// <summary>
    /// 添加交易日志
    /// </summary>
    public void AddLog(string message)
    {
        var log = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _tradeLogs.Insert(0, log);
        if (_tradeLogs.Count > 500)
            _tradeLogs.RemoveAt(_tradeLogs.Count - 1);

        OnLogUpdated?.Invoke(log);
    }

    /// <summary>
    /// 获取所有日志
    /// </summary>
    public List<string> GetLogs() => new(_tradeLogs);

    /// <summary>
    /// 启动自动交易
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        AddLog("========== 自动交易系统启动 ==========");

        // 股票扫描定时器 - 每30分钟扫描一次
        _timer = new System.Windows.Forms.Timer { Interval = 30 * 60 * 1000 };
        _timer.Tick += async (s, e) => await ScanAndTradeAsync();
        _timer.Start();

        // 基金定时器 - 14:50执行
        StartFundTimer();

        // 复盘定时器 - 每天15:10执行（收市后）
        StartReviewTimer();

        // 立即执行一次
        _ = ScanAndTradeAsync();

        AddLog("已启动股票扫描 (每30分钟) 和基金定时 (14:50) 和复盘 (15:10)");
    }

    /// <summary>
    /// 设置复盘服务
    /// </summary>
    public void SetReviewService(DailyReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// 启动复盘定时器
    /// </summary>
    private void StartReviewTimer()
    {
        // 每分钟检查一次是否是15:10
        _reviewTimer = new System.Windows.Forms.Timer { Interval = 60 * 1000 };
        _reviewTimer.Tick += async (s, e) =>
        {
            // 检查是否收市后（15:10左右）
            if (DateTime.Now.Hour == 15 && DateTime.Now.Minute == 10)
            {
                // 检查今天是否已复盘
                var latest = _reviewService?.GetLatestReview();
                if (latest == null || latest.Date.Date < DateTime.Today)
                {
                    await ExecuteDailyReviewAsync();
                }
            }
        };
        _reviewTimer.Start();
    }

    /// <summary>
    /// 执行每日复盘
    /// </summary>
    public async Task ExecuteDailyReviewAsync()
    {
        if (_reviewService == null)
        {
            AddLog("复盘服务未初始化");
            return;
        }

        AddLog("========== 开始每日复盘 ==========");
        try
        {
            var review = await _reviewService.ExecuteDailyReviewAsync();
            AddLog($"复盘完成: 总资产 ¥{review.TotalAssets:N2}, 盈亏 {review.TotalProfitLoss:N2}");
            AddLog($"今日交易: {review.TradeCount}笔, 持仓: {review.PositionCount}只");
        }
        catch (Exception ex)
        {
            AddLog($"复盘失败: {ex.Message}");
        }
    }
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _timer?.Stop();
        _fundTimer?.Stop();
        AddLog("========== 自动交易系统已停止 ==========");
    }

    /// <summary>
    /// 启动基金定时器
    /// </summary>
    private void StartFundTimer()
    {
        // 每分钟检查一次是否是14:50
        _fundTimer = new System.Windows.Forms.Timer { Interval = 60 * 1000 };
        _fundTimer.Tick += async (s, e) =>
        {
            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 50)
            {
                await ExecuteFundTradingAsync();
            }
        };
        _fundTimer.Start();
    }

    /// <summary>
    /// 扫描市场并执行交易（公开给Web API调用）
    /// </summary>
    public async Task ScanAndTradeAsync()
    {
        if (!_isRunning) return;

        // 检查是否在交易时间（A股开盘时段）
        if (!MarketTime.IsTradingTime())
        {
            var nextTime = MarketTime.GetNextTradingTime();
            var reason = MarketTime.IsTradingDay(DateTime.Now)
                ? "当前不在交易时间 (9:30-11:30, 13:00-15:00)"
                : "今日不是交易日";
            AddLog($"[跳过] {reason}");
            if (nextTime.HasValue)
                AddLog($"[下次交易时间] {nextTime:MM-dd HH:mm}");
            return;
        }

        AddLog(">>> 开始扫描市场...");
        AddLog($"股票自选: {string.Join(", ", _stockWatchList)}");
        AddLog($"基金自选: {string.Join(", ", _fundWatchList)}");

        var positions = _tradingService.GetPositions();
        var trades = _tradingService.GetTradeRecords();

        // 扫描股票
        await ScanStocksAsync(positions, trades);

        AddLog(">>> 本次扫描完成");
    }

    /// <summary>
    /// 扫描股票
    /// </summary>
    private async Task ScanStocksAsync(List<Position> positions, List<TradeRecord> trades)
    {
        foreach (var code in _stockWatchList)
        {
            try
            {
                var quote = await _marketService.GetStockQuoteAsync(code);
                if (quote == null) continue;

                // 检查是否已持仓
                var position = positions.FirstOrDefault(p =>
                    p.Code.Contains(code) || p.Code == $"sh{code}" || p.Code == $"sz{code}");

                // 如果已持仓，检查是否需要卖出（涨幅过大）
                if (position != null)
                {
                    if (quote.ChangePercent > 8) // 涨幅>8%可以考虑卖出
                    {
                        AddLog($"[卖出信号] {quote.Name} 涨幅{quote.ChangePercent:N2}%，达到止盈点");
                        // 可以自动卖出，这里先记录
                        AddLog($"  -> 建议卖出: 数量{position.Quantity}, 当前价格{quote.CurrentPrice:N2}");
                    }
                    continue;
                }

                // 使用策略分析
                var strategy = _strategies.FirstOrDefault(s => s.Name.Contains("中线均线"));
                if (strategy == null) continue;

                var signal = await strategy.AnalyzeAsync(quote);
                if (signal != null && strategy.ShouldTrade(positions, trades))
                {
                    AddLog($"[买入信号] {signal.Name} ({signal.Code})");
                    AddLog($"  原因: {signal.Reason}");
                    AddLog($"  当前价格: {signal.CurrentPrice:N2}, 评分: {signal.Score}");

                    // 执行买入（每次买1手试试）
                    var result = _tradingService.Buy(code, quote.Name, SecurityType.Stock, 100, signal.CurrentPrice);
                    if (result.Success)
                    {
                        AddLog($"[交易成功] 买入{quote.Name} 100股 @ {signal.CurrentPrice:N2}");
                        OnTradeExecuted?.Invoke(result.Message);
                        _ = _feishuNotify.SendBuyNotification(
                            code, quote.Name, 100, signal.CurrentPrice, 100 * signal.CurrentPrice,
                            signal.Reason, signal.Score,
                            _tradingService.GetAccount().AvailableCash,
                            _tradingService.GetAccount().TotalAssets,
                            _tradingService.GetAccount().TotalProfitLoss);
                    }
                    else
                    {
                        AddLog($"[交易失败] {result.Message}");
                    }
                }

                await Task.Delay(1000); // 避免请求过快
            }
            catch (Exception ex)
            {
                AddLog($"扫描股票{code}出错: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 执行基金交易（14:50）
    /// </summary>
    private async Task ExecuteFundTradingAsync()
    {
        AddLog("========== 基金定投时间到 (14:50) ==========");

        var positions = _tradingService.GetPositions();
        var fundPositions = positions.Where(p => p.Type == SecurityType.Fund).ToList();

        // 检查是否需要加仓
        foreach (var code in _fundWatchList)
        {
            try
            {
                var quote = await _marketService.GetFundQuoteAsync(code);
                if (quote == null) continue;

                // 检查持仓
                var position = fundPositions.FirstOrDefault(p => p.Code == code);

                var strategy = _strategies.FirstOrDefault(s => s.Name.Contains("基金"));
                if (strategy == null) continue;

                var signal = await strategy.AnalyzeAsync(quote);
                if (signal != null)
                {
                    AddLog($"[基金信号] {signal.Name} ({signal.Code})");
                    AddLog($"  原因: {signal.Reason}");

                    // 每次定投1000元
                    var amount = 1000m;
                    var quantity = (int)(amount / signal.CurrentPrice);

                    if (quantity > 0)
                    {
                        var result = _tradingService.Buy(code, quote.Name, SecurityType.Fund, quantity, signal.CurrentPrice);
                        if (result.Success)
                        {
                            AddLog($"[定投成功] 买入{quote.Name} {quantity}份 @ {signal.CurrentPrice:N2}");
                            OnTradeExecuted?.Invoke(result.Message);
                            _ = _feishuNotify.SendBuyNotification(
                                code, quote.Name, quantity, signal.CurrentPrice, quantity * signal.CurrentPrice,
                                signal.Reason, signal.Score,
                                _tradingService.GetAccount().AvailableCash,
                                _tradingService.GetAccount().TotalAssets,
                                _tradingService.GetAccount().TotalProfitLoss);
                        }
                        else
                        {
                            AddLog($"[定投失败] {result.Message}");
                        }
                    }
                }

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                AddLog($"基金定投{code}出错: {ex.Message}");
            }
        }

        AddLog("========== 基金定投完成 ==========");
    }

    /// <summary>
    /// 获取自选股列表
    /// </summary>
    public List<string> GetStockWatchList() => new(_stockWatchList);

    /// <summary>
    /// 获取自选基列表
    /// </summary>
    public List<string> GetFundWatchList() => new(_fundWatchList);

    /// <summary>
    /// 添加自选股
    /// </summary>
    public void AddStock(string code)
    {
        if (!_stockWatchList.Contains(code))
        {
            _stockWatchList.Add(code);
            AddLog($"已添加自选股: {code}");
        }
    }

    /// <summary>
    /// 添加自选基
    /// </summary>
    public void AddFund(string code)
    {
        if (!_fundWatchList.Contains(code))
        {
            _fundWatchList.Add(code);
            AddLog($"已添加自选基: {code}");
        }
    }
}
