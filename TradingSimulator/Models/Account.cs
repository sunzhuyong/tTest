namespace TradingSimulator.Models;

/// <summary>
/// 模拟账户
/// </summary>
public class Account
{
    public decimal InitialCapital { get; set; } = 100000m;  // 初始资金 10万
    public decimal AvailableCash { get; set; }              // 可用资金
    public decimal MarketValue { get; set; }                // 持仓市值
    public decimal TotalAssets => AvailableCash + MarketValue;  // 总资产
    public decimal TotalProfitLoss => TotalAssets - InitialCapital;  // 总盈亏
    public decimal ProfitLossPercent => InitialCapital == 0 ? 0 : TotalProfitLoss / InitialCapital * 100;  // 盈亏比例
}
