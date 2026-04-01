using TradingSimulator.Models;

namespace TradingSimulator.Services;

/// <summary>
/// 策略信号
/// </summary>
public class StrategySignal
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public SecurityType Type { get; set; }
    public string StrategyName { get; set; } = "";
    public string SignalType { get; set; } = "";  // Buy, Sell, Hold
    public decimal Score { get; set; }            // 评分 0-100
    public string Reason { get; set; } = "";      // 信号原因
    public decimal CurrentPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public DateTime SignalTime { get; set; }
}

/// <summary>
/// 策略基类
/// </summary>
public abstract class BaseStrategy
{
    public string Name { get; set; } = "";

    /// <summary>
    /// 分析证券，返回信号
    /// </summary>
    public abstract Task<StrategySignal?> AnalyzeAsync(Security security);

    /// <summary>
    /// 是否应该交易（防止频繁交易）
    /// </summary>
    public abstract bool ShouldTrade(List<Position> positions, List<TradeRecord> recentTrades);
}

/// <summary>
/// 中线股票策略 - 均线多头 + MACD金叉 + 低位埋伏
/// </summary>
public class StockMidTermStrategy : BaseStrategy
{
    public string Name => "中线均线埋伏";

    public override async Task<StrategySignal?> AnalyzeAsync(Security security)
    {
        if (security.Type != SecurityType.Stock)
            return null;

        // 模拟技术分析（实际需要历史K线数据）
        // 中线策略：不追高，涨幅大于5%不考虑
        if (security.ChangePercent > 5)
            return null;

        // 跌幅过大可能是陷阱
        if (security.ChangePercent < -8)
            return null;

        var score = 50m;

        // 根据跌幅给加分（越接近0越好，埋伏低位）
        var changeScore = (5 - Math.Abs(security.ChangePercent)) * 5;
        score += changeScore;

        // 根据成交量给加分
        if (security.Volume > 10000)
            score += 10;

        // 低价股更适合中线埋伏
        if (security.CurrentPrice < 20)
            score += 15;

        if (score >= 60)
        {
            return new StrategySignal
            {
                Code = security.Code,
                Name = security.Name,
                Type = SecurityType.Stock,
                StrategyName = Name,
                SignalType = "Buy",
                Score = score,
                Reason = $"中线埋伏策略: 评分{score:N0}，当前跌幅{security.ChangePercent:N2}%，适合逢低布局",
                CurrentPrice = security.CurrentPrice,
                ChangePercent = security.ChangePercent,
                SignalTime = DateTime.Now
            };
        }

        return null;
    }

    public override bool ShouldTrade(List<Position> positions, List<TradeRecord> recentTrades)
    {
        // 最近3天没有交易过
        var recent = recentTrades.Where(t => t.TradeTime > DateTime.Now.AddDays(-3)).ToList();
        return recent.Count == 0;
    }
}

/// <summary>
/// 基金中线策略 - 估值低位 + 均线反弹
/// </summary>
public class FundMidTermStrategy : BaseStrategy
{
    public string Name => "基金中线定投";

    public override async Task<StrategySignal?> AnalyzeAsync(Security security)
    {
        if (security.Type != SecurityType.Fund)
            return null;

        var score = 50m;

        // 基金跌幅给加分
        if (security.ChangePercent < -1)
            score += 20;

        // 跌幅越大越适合定投
        if (security.ChangePercent < -3)
            score += 15;

        // 指数基金（代码以16/15开头）
        if (security.Code.StartsWith("16") || security.Code.StartsWith("15"))
            score += 10;

        if (score >= 60)
        {
            return new StrategySignal
            {
                Code = security.Code,
                Name = security.Name,
                Type = SecurityType.Fund,
                StrategyName = Name,
                SignalType = "Buy",
                Score = score,
                Reason = $"基金定投策略: 评分{score:N0}，今日跌幅{security.ChangePercent:N2}%，适合逢低定投",
                CurrentPrice = security.CurrentPrice,
                ChangePercent = security.ChangePercent,
                SignalTime = DateTime.Now
            };
        }

        return null;
    }

    public override bool ShouldTrade(List<Position> positions, List<TradeRecord> recentTrades)
    {
        // 基金每天都可以定投
        return true;
    }
}
