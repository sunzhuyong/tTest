using System.Net.Http;
using Newtonsoft.Json;

namespace TradingSimulator.Services;

/// <summary>
/// 飞书通知服务
/// </summary>
public class FeishuNotifyService
{
    private readonly string _webhookUrl = "https://open.feishu.cn/open-apis/bot/v2/hook/0260cbcc-f091-4a82-8ad3-27bd664af86c";
    private readonly HttpClient _httpClient;
    private readonly bool _enabled = true;

    public FeishuNotifyService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public async Task SendMessage(string title, string message)
    {
        if (!_enabled) return;

        try
        {
            var data = new
            {
                msg_type = "text",
                content = new { text = $"📈 {title}\n{message}" }
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[飞书通知] {title} - 发送结果: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[飞书通知] 发送失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送买入通知
    /// </summary>
    public async Task SendBuyNotification(
        string code, string name, int quantity, decimal price, decimal amount,
        string reason, decimal score, decimal accountBalance, decimal totalAssets, decimal profitLoss)
    {
        var plText = profitLoss >= 0 ? $"盈利 +¥{profitLoss:N2}" : $"亏损 -¥{Math.Abs(profitLoss):N2}";
        var message = $"""
            买入 {name} ({code})
            ─────────────────
            📊 数量: {quantity}股
            💰 价格: ¥{price:N2}
            💵 金额: ¥{amount:N2}

            🧠 买入理由: {reason}
            ⭐ 策略评分: {score}

            ═══════════════════
            💳 账户余额: ¥{accountBalance:N2}
            📈 总资产: ¥{totalAssets:N2}
            {plText}
            """;

        await SendMessage("【买入通知】", message);
    }

    /// <summary>
    /// 发送卖出通知
    /// </summary>
    public async Task SendSellNotification(
        string code, string name, int quantity, decimal price, decimal amount,
        string reason, decimal profitLoss, decimal accountBalance, decimal totalAssets, decimal totalProfitLoss)
    {
        var plText = profitLoss >= 0 ? $"盈利 +¥{profitLoss:N2}" : $"亏损 -¥{Math.Abs(profitLoss):N2}";
        var totalPlText = totalProfitLoss >= 0 ? $"盈利 +¥{totalProfitLoss:N2}" : $"亏损 -¥{Math.Abs(totalProfitLoss):N2}";
        var message = $"""
            卖出 {name} ({code})
            ─────────────────
            📊 数量: {quantity}股
            💰 价格: ¥{price:N2}
            💵 金额: ¥{amount:N2}

            🧠 卖出理由: {reason}

            ═══════════════════
            💰 本次{plText}
            💳 账户余额: ¥{accountBalance:N2}
            📈 总资产: ¥{totalAssets:N2}
            {totalPlText}
            """;

        await SendMessage("【卖出通知】", message);
    }

    /// <summary>
    /// 发送系统通知
    /// </summary>
    public async Task SendSystemNotification(string message, decimal totalAssets, decimal profitLoss)
    {
        var plText = profitLoss >= 0 ? $"盈利 +¥{profitLoss:N2}" : $"亏损 -¥{Math.Abs(profitLoss):N2}";
        var msg = $"""
            {message}
            ─────────────────
            📈 总资产: ¥{totalAssets:N2}
            {plText}
            """;

        await SendMessage("【系统通知】", msg);
    }

    /// <summary>
    /// 发送定时报告
    /// </summary>
    public async Task SendDailyReport(
        int tradeCount, decimal totalAssets, decimal profitLoss, decimal profitPercent,
        int positionCount, string positions)
    {
        var plText = profitLoss >= 0 ? $"盈利 +¥{profitLoss:N2}" : $"亏损 -¥{Math.Abs(profitLoss):N2}";
        var message = $"""
            📊 每日交易报告
            ─────────────────
            🔄 交易次数: {tradeCount}次
            📈 总资产: ¥{totalAssets:N2}
            {plText} ({profitPercent:+0.00;-0.00}%)
            📊 持仓数量: {positionCount}只

            {"".PadRight(20, '─')}
            {positions}
            """;

        await SendMessage("【每日报告】", message);
    }
}

/// <summary>
/// A股市场时间工具
/// </summary>
public static class MarketTime
{
    /// <summary>
    /// 是否在交易时间
    /// </summary>
    public static bool IsTradingTime()
    {
        var now = DateTime.Now;

        // 必须是交易日
        if (!IsTradingDay(now))
            return false;

        // 上午: 9:30 - 11:30
        if (now.Hour == 9 && now.Minute >= 30 || (now.Hour >= 10 && now.Hour < 11))
            return true;
        if (now.Hour == 11 && now.Minute <= 30)
            return true;

        // 下午: 13:00 - 15:00
        if (now.Hour >= 13 && now.Hour < 15)
            return true;
        if (now.Hour == 15 && now.Minute == 0)
            return true;

        return false;
    }

    /// <summary>
    /// 是否是交易日（排除周末和简单节假日）
    /// </summary>
    public static bool IsTradingDay(DateTime date)
    {
        // 周末不是交易日
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // 简单节假日检查（2026年）
        var holidays = new[]
        {
            // 元旦
            new DateTime(2026, 1, 1),
            // 春节（简化）
            new DateTime(2026, 1, 28), new DateTime(2026, 1, 29), new DateTime(2026, 1, 30),
            new DateTime(2026, 1, 31), new DateTime(2026, 2, 1), new DateTime(2026, 2, 2),
            // 清明
            new DateTime(2026, 4, 4), new DateTime(2026, 4, 5),
            // 劳动节
            new DateTime(2026, 5, 1), new DateTime(2026, 5, 2), new DateTime(2026, 5, 3),
            // 端午节
            new DateTime(2026, 5, 31), new DateTime(2026, 6, 1),
            // 中秋节
            new DateTime(2026, 9, 15), new DateTime(2026, 9, 16), new DateTime(2026, 9, 17),
            // 国庆节
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 2), new DateTime(2026, 10, 3),
            new DateTime(2026, 10, 4), new DateTime(2026, 10, 5), new DateTime(2026, 10, 6), new DateTime(2026, 10, 7),
        };

        return !holidays.Contains(date.Date);
    }

    /// <summary>
    /// 获取下次交易时间
    /// </summary>
    public static DateTime? GetNextTradingTime()
    {
        var now = DateTime.Now;

        // 如果现在是交易时间，返回现在
        if (IsTradingTime())
            return now;

        // 计算下次交易时间
        var next = now.AddMinutes(1);
        while (next.Hour < 16)
        {
            if (IsTradingDay(next) && IsTradingTime())
                return next;
            next = next.AddMinutes(1);
        }

        // 否则返回下个交易日的9:30
        for (int i = 1; i < 7; i++)
        {
            var nextDay = now.Date.AddDays(i);
            if (nextDay.DayOfWeek != DayOfWeek.Saturday && nextDay.DayOfWeek != DayOfWeek.Sunday)
                return nextDay.AddHours(9.5);
        }

        return null;
    }
}
