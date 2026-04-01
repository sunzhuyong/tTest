namespace TradingSimulator.Models;

/// <summary>
/// 交易标的类型
/// </summary>
public enum SecurityType
{
    Stock,  // 股票
    Fund     // 基金
}

/// <summary>
/// 交易方向
/// </summary>
public enum TradeDirection
{
    Buy,   // 买入
    Sell   // 卖出
}

/// <summary>
/// 交易标的
/// </summary>
public class Security
{
    public string Code { get; set; } = "";        // 证券代码 (如 600000, 161039)
    public string Name { get; set; } = "";         // 证券名称
    public SecurityType Type { get; set; }         // 类型
    public decimal CurrentPrice { get; set; }      // 当前价格
    public decimal ChangePercent { get; set; }     // 涨跌幅
    public decimal OpenPrice { get; set; }         // 开盘价
    public decimal HighPrice { get; set; }         // 最高价
    public decimal LowPrice { get; set; }          // 最低价
    public decimal Volume { get; set; }            // 成交量
    public decimal Amount { get; set; }            // 成交额
    public DateTime UpdateTime { get; set; }       // 更新时间
}
