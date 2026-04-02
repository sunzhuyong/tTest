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

        // 激进策略：只要不是暴涨暴跌都可以考虑
        // 涨幅超过10%不追高
        if (security.ChangePercent > 10)
            return null;

        // 跌幅超过10%可能有风险，但如果是建仓机会也可以考虑
        // if (security.ChangePercent < -10)
        //     return null;

        var score = 50m;

        // 根据涨跌给分（接近0的更好）
        var changeScore = (8 - Math.Abs(security.ChangePercent)) * 4;
        score += changeScore;

        // 有成交量就加分
        if (security.Volume > 5000)
            score += 15;

        // 低价股更适合短线
        if (security.CurrentPrice < 50)
            score += 10;

        // 核电、AI等热门板块加分
        if (security.Code == "601985" || security.Code == "601727" ||
            security.Code == "300033" || security.Code == "300229")
            score += 10;

        // 激进模式：评分50以上就买入
        if (score >= 50)
        {
            return new StrategySignal
            {
                Code = security.Code,
                Name = security.Name,
                Type = SecurityType.Stock,
                StrategyName = Name,
                SignalType = "Buy",
                Score = score,
                Reason = $"激进策略: 评分{score:N0}，涨跌{security.ChangePercent:N2}%，符合买入条件",
                CurrentPrice = security.CurrentPrice,
                ChangePercent = security.ChangePercent,
                SignalTime = DateTime.Now
            };
        }

        return null;
    }

    public override bool ShouldTrade(List<Position> positions, List<TradeRecord> recentTrades)
    {
        // 激进模式：只要最近15分钟没交易就可以
        var recent = recentTrades.Where(t => t.TradeTime > DateTime.Now.AddMinutes(-15)).ToList();
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
