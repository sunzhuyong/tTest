using System.Net.Http;
using Newtonsoft.Json;

namespace TradingSimulator.Services;

/// <summary>
/// 每日复盘服务 - A股收市后自动执行
/// </summary>
public class DailyReviewService
{
    private readonly TradingService _tradingService;
    private readonly MarketDataService _marketService;
    private readonly FeishuNotifyService _feishuNotify;
    private readonly List<ReviewRecord> _reviewHistory = new();
    private readonly string _reviewFile;

    public DailyReviewService(TradingService tradingService, MarketDataService marketService, FeishuNotifyService feishuNotify)
    {
        _tradingService = tradingService;
        _marketService = marketService;
        _feishuNotify = feishuNotify;
        _reviewFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "reviews.json");
        LoadHistory();
    }

    /// <summary>
    /// 执行每日复盘
    /// </summary>
    public async Task<ReviewRecord> ExecuteDailyReviewAsync()
    {
        var account = _tradingService.GetAccount();
        var positions = _tradingService.GetPositions();
        var trades = _tradingService.GetTradeRecords();

        // 获取今日交易
        var todayTrades = trades.Where(t => t.TradeTime.Date == DateTime.Today).ToList();
        var buyCount = todayTrades.Count(t => t.Direction == Models.TradeDirection.Buy);
        var sellCount = todayTrades.Count(t => t.Direction == Models.TradeDirection.Sell);

        // 计算今日盈亏
        var todayProfitLoss = todayTrades.Sum(t => t.ProfitLoss);

        // 持仓分析
        var winningPositions = positions.Where(p => p.ProfitLoss > 0).ToList();
        var losingPositions = positions.Where(p => p.ProfitLoss < 0).ToList();

        // 生成复盘总结
        var review = new ReviewRecord
        {
            Date = DateTime.Today,
            TotalAssets = account.TotalAssets,
            TotalProfitLoss = account.TotalProfitLoss,
            ProfitLossPercent = account.ProfitLossPercent,
            TradeCount = todayTrades.Count,
            BuyCount = buyCount,
            SellCount = sellCount,
            TodayProfitLoss = todayProfitLoss,
            PositionCount = positions.Count,
            WinningPositions = winningPositions.Count,
            LosingPositions = losingPositions.Count,
            TopWinner = winningPositions.OrderByDescending(p => p.ProfitLossPercent).FirstOrDefault()?.Name,
            TopLoser = losingPositions.OrderBy(p => p.ProfitLossPercent).FirstOrDefault()?.Name,
            ReviewContent = GenerateReviewContent(account, positions, todayTrades),
            StrategySuggestions = GenerateStrategySuggestions(account, positions, todayTrades),
            CreatedAt = DateTime.Now
        };

        _reviewHistory.Add(review);
        SaveHistory();

        // 发送飞书通知
        await SendReviewNotification(review);

        return review;
    }

    /// <summary>
    /// 生成复盘内容
    /// </summary>
    private string GenerateReviewContent(Models.Account account, List<Models.Position> positions, List<Models.TradeRecord> todayTrades)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("📊 今日交易回顾");
        sb.AppendLine($"今日交易 {todayTrades.Count} 笔");

        if (todayTrades.Count > 0)
        {
            foreach (var trade in todayTrades)
            {
                var direction = trade.Direction == Models.TradeDirection.Buy ? "买入" : "卖出";
                var plText = trade.ProfitLoss >= 0 ? $"盈利 ¥{trade.ProfitLoss:N2}" : $"亏损 ¥{Math.Abs(trade.ProfitLoss):N2}";
                sb.AppendLine($"  - {trade.Name}({trade.Code}) {direction} {trade.Quantity}股 @ ¥{trade.Price:N2} | {plText}");
            }
        }
        else
        {
            sb.AppendLine("  - 今日无交易");
        }

        sb.AppendLine();
        sb.AppendLine("📈 持仓状况");
        sb.AppendLine($"  持仓 {positions.Count} 只，总市值 ¥{account.MarketValue:N2}");
        sb.AppendLine($"  盈利 {positions.Count(p => p.ProfitLoss > 0)} 只，亏损 {positions.Count(p => p.ProfitLoss < 0)} 只");

        return sb.ToString();
    }

    /// <summary>
    /// 生成策略建议
    /// </summary>
    private string GenerateStrategySuggestions(Models.Account account, List<Models.Position> positions, List<Models.TradeRecord> todayTrades)
    {
        var sb = new System.Text.StringBuilder();

        // 分析亏损原因
        var losingPositions = positions.Where(p => p.ProfitLoss < 0).ToList();
        if (losingPositions.Any())
        {
            var worst = losingPositions.OrderBy(p => p.ProfitLoss).First();
            sb.AppendLine($"⚠️ 注意: {worst.Name} 亏损 {Math.Abs(worst.ProfitLossPercent):.1f}%");

            // 如果亏损超过5%，建议止损
            if (worst.ProfitLossPercent < -5)
            {
                sb.AppendLine($"  建议: 考虑止损 {worst.Code}");
            }
        }

        // 分析盈利
        var winningPositions = positions.Where(p => p.ProfitLoss > 0).ToList();
        if (winningPositions.Any())
        {
            var best = winningPositions.OrderByDescending(p => p.ProfitLoss).First();
            sb.AppendLine($"✅ 亮点: {best.Name} 盈利 {best.ProfitLossPercent:.1f}%");
        }

        // 资金使用率
        var usedPercent = (account.InitialCapital - account.AvailableCash) / account.InitialCapital * 100;
        if (usedPercent < 30)
        {
            sb.AppendLine($"💡 建议: 仓位偏低({usedPercent:.0f}%)，可考虑加仓");
        }
        else if (usedPercent > 80)
        {
            sb.AppendLine($"💡 建议: 仓位偏高({usedPercent:.0f}%)，注意风险");
        }

        // 策略迭代建议
        if (todayTrades.Count == 0)
        {
            sb.AppendLine("📝 今日无交易，可能是策略过于保守，可适当放宽买入条件");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 发送复盘通知
    /// </summary>
    private async Task SendReviewNotification(ReviewRecord review)
    {
        var plText = review.TotalProfitLoss >= 0
            ? $"盈利 +¥{review.TotalProfitLoss:N2}"
            : $"亏损 -¥{Math.Abs(review.TotalProfitLoss):N2}";

        var message = $"""
            📊 每日复盘报告 - {review.Date:MM-dd}
            ═══════════════════════════════

            💰 账户状况
            总资产: ¥{review.TotalAssets:N2}
            {plText} ({review.ProfitLossPercent:+0.00;-0.00}%)

            📈 交易统计
            今日交易: {review.TradeCount}笔
            买入: {review.BuyCount} | 卖出: {review.SellCount}
            今日盈亏: ¥{review.TodayProfitLoss:N2}

            📊 持仓状况
            持仓: {review.PositionCount}只
            盈利: {review.WinningPositions} | 亏损: {review.LosingPositions}

            {"".PadRight(20, '─')}
            {review.ReviewContent}

            {"".PadRight(20, '─')}
            {review.StrategySuggestions}
            """;

        await _feishuNotify.SendMessage("【每日复盘】", message);
    }

    /// <summary>
    /// 获取复盘历史
    /// </summary>
    public List<ReviewRecord> GetReviewHistory(int days = 30)
    {
        return _reviewHistory.OrderByDescending(r => r.Date).Take(days).ToList();
    }

    /// <summary>
    /// 获取最新复盘
    /// </summary
    public ReviewRecord? GetLatestReview()
    {
        return _reviewHistory.LastOrDefault();
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(_reviewFile))
            {
                var json = File.ReadAllText(_reviewFile);
                _reviewHistory.AddRange(JsonConvert.DeserializeObject<List<ReviewRecord>>(json) ?? new List<ReviewRecord>());
            }
        }
        catch { }
    }

    private void SaveHistory()
    {
        try
        {
            var dir = Path.GetDirectoryName(_reviewFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(_reviewHistory, Formatting.Indented);
            File.WriteAllText(_reviewFile, json);
        }
        catch { }
    }
}

/// <summary>
/// 复盘记录
/// </summary>
public class ReviewRecord
{
    public DateTime Date { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal ProfitLossPercent { get; set; }
    public int TradeCount { get; set; }
    public int BuyCount { get; set; }
    public int SellCount { get; set; }
    public decimal TodayProfitLoss { get; set; }
    public int PositionCount { get; set; }
    public int WinningPositions { get; set; }
    public int LosingPositions { get; set; }
    public string? TopWinner { get; set; }
    public string? TopLoser { get; set; }
    public string ReviewContent { get; set; } = "";
    public string StrategySuggestions { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
