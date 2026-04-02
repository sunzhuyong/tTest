using TradingSimulator.Models;

namespace TradingSimulator.Services;

/// <summary>
/// 交易服务
/// </summary>
public class TradingService
{
    private readonly DatabaseService _db;
    private readonly MarketDataService _market;
    private const decimal CommissionRate = 0.0003m;  // 手续费率 (万三)

    public TradingService(DatabaseService db, MarketDataService market)
    {
        _db = db;
        _market = market;
    }

    /// <summary>
    /// 买入
    /// </summary>
    public (bool Success, string Message) Buy(string code, string name, SecurityType type, decimal quantity, decimal price)
    {
        if (quantity <= 0)
            return (false, "买入数量必须大于0");

        // A股最少买入100股
        if (type == SecurityType.Stock && quantity < 100)
            return (false, "A股股票最少买入100股");

        var account = _db.GetAccount();
        var amount = quantity * price;
        var commission = amount * CommissionRate;
        var totalCost = amount + commission;

        if (totalCost > account.AvailableCash)
        {
            return (false, $"可用资金不足，需要 {totalCost:N2} 元，可用 {account.AvailableCash:N2} 元");
        }

        // 扣除资金
        var newCash = account.AvailableCash - totalCost;
        _db.UpdateAccountCash(newCash);

        // 添加持仓
        var position = new Position
        {
            Code = code,
            Name = name,
            Type = type,
            Quantity = quantity,
            CostPrice = price,
            PurchaseDate = DateTime.Now
        };
        _db.AddPosition(position);

        // 添加交易记录
        var record = new TradeRecord
        {
            Code = code,
            Name = name,
            Type = type,
            Direction = TradeDirection.Buy,
            Price = price,
            Quantity = quantity,
            Amount = amount,
            Commission = commission,
            ProfitLoss = -commission,  // 买入时盈亏为负（手续费）
            TradeTime = DateTime.Now
        };
        _db.AddTradeRecord(record);

        return (true, $"买入成功！\n证券: {name} ({code})\n数量: {quantity}\n价格: {price:N2}\n金额: {amount:N2}\n手续费: {commission:N2}");
    }

    /// <summary>
    /// 卖出
    /// </summary>
    public (bool Success, string Message) Sell(string code, decimal quantity, decimal price)
    {
        if (quantity <= 0)
            return (false, "卖出数量必须大于0");

        // A股必须100股整数倍
        if (quantity % 100 != 0)
            return (false, "A股卖出数量必须是100的整数倍");

        var positions = _db.GetPositions();
        var position = positions.FirstOrDefault(p => p.Code == code);

        if (position == null)
        {
            return (false, "该证券不存在持仓");
        }

        if (quantity > position.Quantity)
        {
            return (false, $"持仓不足，当前持仓 {position.Quantity} 股");
        }

        var amount = quantity * price;
        var commission = amount * CommissionRate;
        var netAmount = amount - commission;

        // 增加可用资金
        var account = _db.GetAccount();
        var newCash = account.AvailableCash + netAmount;
        _db.UpdateAccountCash(newCash);

        // 减少持仓
        _db.ReducePosition(code, quantity);

        // 添加交易记录
        var profitLoss = (price - position.CostPrice) * quantity;
        var record = new TradeRecord
        {
            Code = code,
            Name = position.Name,
            Type = position.Type,
            Direction = TradeDirection.Sell,
            Price = price,
            Quantity = quantity,
            Amount = amount,
            Commission = commission,
            ProfitLoss = profitLoss - commission,  // 卖出盈亏减去手续费
            TradeTime = DateTime.Now
        };
        _db.AddTradeRecord(record);
        var profitText = profitLoss >= 0 ? $"盈利 {profitLoss:N2}" : $"亏损 {Math.Abs(profitLoss):N2}";

        return (true, $"卖出成功！\n证券: {position.Name} ({code})\n数量: {quantity}\n价格: {price:N2}\n金额: {amount:N2}\n手续费: {commission:N2}\n净得: {netAmount:N2}\n{profitText}");
    }

    /// <summary>
    /// 获取账户信息
    /// </summary>
    public Account GetAccount()
    {
        var account = _db.GetAccount();

        // 更新持仓的实时价格
        var positions = _db.GetPositions();
        var marketValue = 0m;

        foreach (var pos in positions)
        {
            var quote = pos.Type == SecurityType.Stock
                ? _market.GetStockQuoteAsync(pos.Code.Replace("sh", "").Replace("sz", "")).Result
                : _market.GetFundQuoteAsync(pos.Code).Result;

            if (quote != null)
            {
                pos.CurrentPrice = quote.CurrentPrice;
                marketValue += pos.MarketValue;
            }
        }

        account.MarketValue = marketValue;
        return account;
    }

    /// <summary>
    /// 获取持仓列表（带实时价格）
    /// </summary>
    public List<Position> GetPositions()
    {
        var positions = _db.GetPositions();

        foreach (var pos in positions)
        {
            try
            {
                var code = pos.Code.Replace("sh", "").Replace("sz", "");
                Security? quote = null;

                if (pos.Type == SecurityType.Stock)
                    quote = _market.GetStockQuoteAsync(code).Result;
                else
                    quote = _market.GetFundQuoteAsync(code).Result;

                if (quote != null)
                {
                    pos.CurrentPrice = quote.CurrentPrice;
                }
            }
            catch
            {
                // 忽略行情获取失败
            }
        }

        return positions;
    }

    /// <summary>
    /// 获取交易记录
    /// </summary>
    public List<TradeRecord> GetTradeRecords()
    {
        return _db.GetTradeRecords();
    }

    /// <summary>
    /// 重置账户
    /// </summary>
    public void ResetAccount(decimal initialCapital = 30000)
    {
        _db.ResetAccount(initialCapital);
    }
}
