namespace TradingSimulator.Models;

/// <summary>
/// 持仓记录
/// </summary>
public class Position
{
    public int Id { get; set; }
    public string Code { get; set; } = "";         // 证券代码
    public string Name { get; set; } = "";          // 证券名称
    public SecurityType Type { get; set; }          // 类型
    public decimal Quantity { get; set; }          // 持有数量
    public decimal CostPrice { get; set; }         // 成本价
    public decimal CurrentPrice { get; set; }      // 当前价
    public DateTime PurchaseDate { get; set; }     // 买入日期

    // 计算属性
    public decimal MarketValue => Quantity * CurrentPrice;           // 市值
    public decimal Cost => Quantity * CostPrice;                    // 成本
    public decimal ProfitLoss => MarketValue - Cost;                  // 浮动盈亏
    public decimal ProfitLossPercent => Cost == 0 ? 0 : ProfitLoss / Cost * 100;  // 盈亏比例
}
