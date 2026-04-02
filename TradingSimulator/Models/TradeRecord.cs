namespace TradingSimulator.Models;

/// <summary>
/// 交易记录
/// </summary>
public class TradeRecord
{
    public int Id { get; set; }
    public string Code { get; set; } = "";          // 证券代码
    public string Name { get; set; } = "";          // 证券名称
    public SecurityType Type { get; set; }          // 类型
    public TradeDirection Direction { get; set; }   // 买入/卖出
    public decimal Price { get; set; }              // 成交价格
    public decimal Quantity { get; set; }           // 成交数量
    public decimal Amount { get; set; }             // 成交金额
    public decimal Commission { get; set; }          // 手续费
    public decimal ProfitLoss { get; set; }          // 本次交易盈亏
    public DateTime TradeTime { get; set; }         // 交易时间
}
