using TradingSimulator.Models;
using System.Threading;

namespace TradingSimulator.Services;

/// <summary>
/// 自动交易引擎
/// </summary>
public class AutoTraderService
{
    private readonly TradingService _tradingService;
    private readonly MarketDataService _marketService;
    private readonly List<BaseStrategy> _strategies;
    private System.Threading.Timer? _scanTimer;
    private System.Threading.Timer? _fundTimer;
    private System.Threading.Timer? _reviewTimer;
    private bool _isRunning;
    public bool IsRunning => _isRunning;
    private readonly List<string> _tradeLogs = new();

    // 自选股列表 (剔除科创板 688/300开头的)
    private readonly List<string> _stockWatchList = new()
    {
        // 银行
        "600000", // 浦发银行
        "600036", // 招商银行
        "000001", // 平安银行
        "601398", // 工商银行
        "601939", // 建设银行
        // 白酒消费
        "600519", // 贵州茅台
        "000858", // 五粮液
        "601888", // 中国中免
        // 新能源
        "300750", // 宁德时代
        "601318", // 中国平安
        "600850", // 华东医药
        // 核电板块
        "601985", // 中国核电
        "600175", // 中核技术
        "600875", // 东方电气
        "601727", // 上海电气
        // 半导体/芯片
        "603986", // 兆易创新
        "603160", // 汇顶科技
        "603501", // 北京君正
        // 医药医疗
        "600276", // 恒瑞医药
        "300760", // 迈瑞医疗
        "002044", // 江苏国泰
        "300015", // 爱尔眼科
        // 房地产
        "000002", // 万科A
        "600048", // 保利发展
        "601155", // 新城控股
        // 军工-航空航天
        "600893", // 航发动力
        "600038", // 中直股份
        "000547", // 航天发展
        "600879", // 航天机电
        "600271", // 航天信息
        "600118", // 中国卫星
        // 机器人
        "300124", // 汇川技术
        "000333", // 美的集团
        "600835", // 上海机电
        "002139", // 拓普集团
        // AI应用
        "300033", // 同花顺
        "300229", // 拓尔思
        "300459", // 汤姆猫
        "300467", // 迅游科技
        "002195", // 二三四五
        "002027", // 分众传媒
        // 科技互联网
        "600570", // 恒生电子
        "002410", // 广联达
        // 煤炭
        "601001", // 大同煤业
        "601088", // 陕西煤业
        "600395", // 盘江股份
        // 钢铁
        "600019", // 宝钢股份
        "600581", // 南钢股份
        // 化工
        "600309", // 万华化学
        "600096", // 云天化
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
        if (_isRunning)
        {
            AddLog("自动交易已在运行中，跳过重复启动");
            return;
        }

        _isRunning = true;
        AddLog("========== 自动交易系统启动 ==========");

        // 股票扫描定时器 - 使用 System.Threading.Timer
        // 如果定时器已存在，先释放
        _scanTimer?.Dispose();
        Console.WriteLine($"[DEBUG] 创建扫描定时器，10分钟后首次触发，之后每10分钟...");
        _scanTimer = new System.Threading.Timer(state =>
        {
            // 同步调用，避免async回调问题
            Console.WriteLine($"[定时器回调] 触发扫描... 时间: {DateTime.Now:HH:mm:ss}");
            AddLog($"[定时器] 触发自动扫描...");
            try
            {
                ScanAndTradeAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[定时器] 扫描异常: {ex.Message}");
                AddLog($"[定时器] 扫描异常: {ex.Message}");
            }
        }, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
        AddLog("[定时器] 扫描定时器已启动，每10分钟触发一次");

        // 基金定时器 - 14:50执行
        StartFundTimer();

        // 复盘定时器 - 每天15:10执行（收市后）
        StartReviewTimer();

        // 立即执行一次
        _ = ScanAndTradeAsync();

        AddLog("已启动股票扫描 (每10分钟) 和基金定时 (14:50) 和复盘 (15:10)");
    }

    /// <summary>
    /// 设置复盘服务
    /// </summary>
    public void SetReviewService(DailyReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private MarketSummaryService? _marketSummaryService;

    /// <summary>
    /// 设置市场概况服务
    /// </summary>
    public void SetMarketSummaryService(MarketSummaryService marketSummaryService)
    {
        _marketSummaryService = marketSummaryService;
    }

    /// <summary>
    /// 启动复盘定时器
    /// </summary>
    private void StartReviewTimer()
    {
        // 每分钟检查一次
        _reviewTimer = new System.Threading.Timer(async _ =>
        {
            try
            {
                var now = DateTime.Now;

                // 检查是否收市后（15:00左右）生成市场总结
                if (now.Hour == 15 && now.Minute == 0)
                {
                    await SendMarketSummaryAsync(false);
                }

                // 检查是否午盘结束（11:30）生成午间总结
                if (now.Hour == 11 && now.Minute == 30)
                {
                    await SendMarketSummaryAsync(true);
                }

                // 检查是否收市后（15:10左右）执行复盘
                if (now.Hour == 15 && now.Minute == 10)
                {
                    // 检查今天是否已复盘
                    var latest = _reviewService?.GetLatestReview();
                    if (latest == null || latest.Date.Date < DateTime.Today)
                    {
                        await ExecuteDailyReviewAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"[复盘定时器] 异常: {ex.Message}");
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// 发送市场总结
    /// </summary>
    private async Task SendMarketSummaryAsync(bool isMorning)
    {
        if (_marketSummaryService == null || _feishuNotify == null)
            return;

        try
        {
            var summary = await _marketSummaryService.GenerateSummaryAsync(isMorning);
            await _feishuNotify.SendMessage(
                isMorning ? "【午间市场总结】" : "【收盘市场总结】",
                summary.Summary);
            AddLog(isMorning ? ">>> 午间市场总结已发送" : ">>> 收盘市场总结已发送");
        }
        catch (Exception ex)
        {
            AddLog($"[错误] 生成市场总结失败: {ex.Message}");
        }
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

    /// <summary>
    /// 停止自动交易
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _scanTimer?.Dispose();
        _fundTimer?.Dispose();
        _reviewTimer?.Dispose();
        AddLog("========== 自动交易系统已停止 ==========");
    }

    /// <summary>
    /// 启动基金定时器
    /// </summary>
    private void StartFundTimer()
    {
        // 每分钟检查一次是否是14:50
        _fundTimer = new System.Threading.Timer(async _ =>
        {
            try
            {
                if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 50)
                {
                    await ExecuteFundTradingAsync();
                }
            }
            catch (Exception ex)
            {
                AddLog($"[基金定时器] 异常: {ex.Message}");
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
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
        var (buySignals, sellSignals) = await ScanStocksAsync(positions, trades);

        // 发送扫描报告到飞书
        Console.WriteLine($"[DEBUG] 发送扫描报告: 买入={buySignals}, 卖出={sellSignals}");
        await _feishuNotify.SendScanReport(
            _stockWatchList.Count,
            _fundWatchList.Count,
            buySignals,
            sellSignals,
            $"扫描完成，买入信号: {buySignals}, 卖出信号: {sellSignals}");
        Console.WriteLine("[DEBUG] 扫描报告已发送");

        AddLog(">>> 本次扫描完成");
    }

    /// <summary>
    /// 扫描股票
    /// </summary>
    private async Task<(int buySignals, int sellSignals)> ScanStocksAsync(List<Position> positions, List<TradeRecord> trades)
    {
        int buySignals = 0;
        int sellSignals = 0;

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
                        sellSignals++;
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
                    buySignals++;
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

        return (buySignals, sellSignals);
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
