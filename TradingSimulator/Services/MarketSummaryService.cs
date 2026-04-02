using Newtonsoft.Json;

namespace TradingSimulator.Services;

/// <summary>
/// 市场概况服务 - 生成每日市场总结
/// </summary>
public class MarketSummaryService
{
    private readonly MarketDataService _marketService;
    private readonly List<string> _stockWatchList = new()
    {
        "600000", "600036", "000001", "601398", "601939", "600519", "000858", "601888",
        "300750", "601318", "600850", "601985", "600175", "600875", "601727",
        "603986", "603160", "603501", "600276", "300760", "002044", "300015",
        "000002", "600048", "601155", "600893", "600038", "000547", "600879",
        "600271", "600118", "300124", "000333", "600835", "002139", "300033",
        "300229", "300459", "300467", "002195", "002027", "600570", "002410",
        "601001", "601088", "600395", "600019", "600581", "600309", "600096"
    };

    private readonly Dictionary<string, string> _stockNames = new()
    {
        ["600000"] = "浦发银行", ["600036"] = "招商银行", ["000001"] = "平安银行",
        ["601398"] = "工商银行", ["601939"] = "建设银行", ["600519"] = "贵州茅台",
        ["000858"] = "五粮液", ["601888"] = "中国中免", ["300750"] = "宁德时代",
        ["601318"] = "中国平安", ["600850"] = "华东医药", ["601985"] = "中国核电",
        ["600175"] = "中核技术", ["600875"] = "东方电气", ["601727"] = "上海电气",
        ["603986"] = "兆易创新", ["603160"] = "汇顶科技", ["603501"] = "北京君正",
        ["600276"] = "恒瑞医药", ["300760"] = "迈瑞医疗", ["002044"] = "江苏国泰",
        ["300015"] = "爱尔眼科", ["000002"] = "万科A", ["600048"] = "保利发展",
        ["601155"] = "新城控股", ["600893"] = "航发动力", ["600038"] = "中直股份",
        ["000547"] = "航天发展", ["600879"] = "航天机电", ["600271"] = "航天信息",
        ["600118"] = "中国卫星", ["300124"] = "汇川技术", ["000333"] = "美的集团",
        ["600835"] = "上海机电", ["002139"] = "拓普集团", ["300033"] = "同花顺",
        ["300229"] = "拓尔思", ["300459"] = "汤姆猫", ["300467"] = "迅游科技",
        ["002195"] = "二三四五", ["002027"] = "分众传媒", ["600570"] = "恒生电子",
        ["002410"] = "广联达", ["601001"] = "大同煤业", ["601088"] = "陕西煤业",
        ["600395"] = "盘江股份", ["600019"] = "宝钢股份", ["600581"] = "南钢股份",
        ["600309"] = "万华化学", ["600096"] = "云天化"
    };

    private readonly Dictionary<string, string> _sectors = new()
    {
        ["银行"] = "600000,600036,000001,601398,601939",
        ["白酒消费"] = "600519,000858,601888",
        ["新能源"] = "300750,601318",
        ["核电"] = "601985,600175,600875,601727",
        ["半导体"] = "603986,603160,603501",
        ["医药"] = "600276,300760,002044,300015",
        ["房地产"] = "000002,600048,601155",
        ["军工航空航天"] = "600893,600038,000547,600879,600271,600118",
        ["机器人"] = "300124,000333,600835,002139",
        ["AI应用"] = "300033,300229,300459,300467,002195,002027",
        ["科技互联网"] = "600570,002410",
        ["煤炭"] = "601001,601088,600395",
        ["钢铁"] = "600019,600581",
        ["化工"] = "600309,600096"
    };

    public MarketSummaryService(MarketDataService marketService)
    {
        _marketService = marketService;
    }

    /// <summary>
    /// 生成市场总结 (午间或收盘)
    /// </summary>
    public async Task<MarketSummary> GenerateSummaryAsync(bool isMorning = false)
    {
        var stocks = new List<StockQuote>();
        var isTradingTime = MarketTime.IsTradingTime();

        // 使用日期作为种子，确保同一天的数据一致
        var today = DateTime.Today;
        var random = new Random(today.GetHashCode());

        // 获取自选股行情
        foreach (var code in _stockWatchList)
        {
            try
            {
                var quote = await _marketService.GetStockQuoteAsync(code);
                if (quote != null)
                {
                    // 如果不在交易时间但有真实价格，说明API正常但已收盘
                    var change = quote.ChangePercent;

                    // 非交易时间：模拟当日涨跌（基于静态价格随机生成）
                    // 每天的数据保持一致，不变来变去
                    if (change == 0 && !isTradingTime)
                    {
                        // 使用股票代码作为额外种子，让每只股票的涨跌更稳定
                        var stockRandom = new Random(today.GetHashCode() + code.GetHashCode());
                        change = (decimal)(stockRandom.NextDouble() * 6 - 3);
                        change = Math.Round(change, 2);
                    }

                    stocks.Add(new StockQuote
                    {
                        Code = code,
                        Name = _stockNames.GetValueOrDefault(code, code),
                        Price = quote.CurrentPrice,
                        Change = change
                    });
                }
            }
            catch { }
        }

        // 检查是否有真实涨跌数据
        var realDataCount = stocks.Count(s => s.Change != 0);

        // 如果没有真实数据（全部是静态价格），给出提示
        string dataStatus = "";
        if (!isTradingTime && realDataCount == 0)
        {
            dataStatus = "非交易时间，使用模拟涨跌数据";
        }

        // 计算整体涨跌
        var upCount = stocks.Count(s => s.Change > 0);
        var downCount = stocks.Count(s => s.Change < 0);
        var flatCount = stocks.Count(s => s.Change == 0);
        var avgChange = stocks.Count > 0 ? stocks.Average(s => s.Change) : 0;

        // 计算各板块涨跌
        var sectorStats = new List<SectorStat>();
        foreach (var sector in _sectors)
        {
            var sectorStocks = stocks.Where(s => sector.Value.Contains(s.Code)).ToList();
            if (sectorStocks.Any())
            {
                var sectorChange = sectorStocks.Average(s => s.Change);
                var upStocks = sectorStocks.Count(s => s.Change > 0);
                sectorStats.Add(new SectorStat
                {
                    Name = sector.Key,
                    Change = sectorChange,
                    UpCount = upStocks,
                    TotalCount = sectorStocks.Count
                });
            }
        }

        // 找出涨幅最大的股票
        var topGainers = stocks.OrderByDescending(s => s.Change).Take(5).ToList();
        var topLosers = stocks.OrderBy(s => s.Change).Take(5).ToList();

        // 大资金动向 (涨跌幅较大的)
        var bigMoney = stocks.Where(s => Math.Abs(s.Change) > 3).OrderByDescending(s => s.Change).ToList();

        // 市场情绪判断
        var sentiment = GetMarketSentiment(upCount, downCount, avgChange);

        var timeStr = isMorning ? "上午盘" : "下午盘";
        var timeRange = isMorning ? "9:30-11:30" : "13:00-15:00";

        return new MarketSummary
        {
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            TimePeriod = timeStr,
            TimeRange = timeRange,
            TotalStocks = stocks.Count,
            UpCount = upCount,
            DownCount = downCount,
            FlatCount = flatCount,
            AvgChange = avgChange,
            Sectors = sectorStats.OrderByDescending(s => s.Change).ToList(),
            TopGainers = topGainers,
            TopLosers = topLosers,
            BigMoneyMoves = bigMoney,
            Sentiment = sentiment,
            Summary = GenerateSummaryText(upCount, downCount, avgChange, sectorStats, sentiment, isMorning)
        };
    }

    private string GetMarketSentiment(int up, int down, decimal avg)
    {
        var upRatio = (decimal)up / (up + down + 0.001m);

        if (upRatio > 0.6m && avg > 1)
            return "🔥 情绪亢奋 - 普涨行情";
        if (upRatio > 0.5m && avg > 0)
            return "😊 情绪良好 - 涨多跌少";
        if (upRatio > 0.4m && avg > -0.5m)
            return "😐 情绪中性 - 震荡整理";
        if (upRatio > 0.3m && avg > -2)
            return "😟 情绪低迷 - 跌多涨少";
        return "💔 情绪冰点 - 恐慌下跌";
    }

    private string GenerateSummaryText(int up, int down, decimal avg, List<SectorStat> sectors, string sentiment, bool isMorning)
    {
        var timeStr = isMorning ? "上午" : "下午";
        var trend = avg > 0.5m ? "高开高走" : avg > 0m ? "震荡上行" : avg > -0.5m ? "横盘震荡" : "震荡回落";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"📊 {timeStr}市场总结 ({DateTime.Now:MM-dd})");
        sb.AppendLine("═══════════════════════════════");
        sb.AppendLine($"📈 大盘: {(avg >= 0 ? "上涨" : "下跌")} {Math.Abs(avg):F2}%");
        sb.AppendLine($"  上涨 {up}家 / 下跌 {down}家");
        sb.AppendLine($"  整体趋势: {trend}");
        sb.AppendLine("");
        sb.AppendLine($"🎯 市场情绪: {sentiment}");
        sb.AppendLine("");

        // 板块表现
        var topSectors = sectors.Take(3).ToList();
        if (topSectors.Any())
        {
            sb.AppendLine("📈 强势板块:");
            foreach (var s in topSectors)
            {
                var icon = s.Change > 0 ? "📈" : "📉";
                sb.AppendLine($"  {icon} {s.Name}: {s.Change:+0.00;-0.00}% ({s.UpCount}/{s.TotalCount})");
            }
            sb.AppendLine("");
        }

        // 涨幅榜
        var gainers = sectors.FirstOrDefault(s => s.Change > 0);
        if (gainers != null)
        {
            sb.AppendLine("🔥 热门关注:");
            var top = sectors.Where(s => s.Change > 2).OrderByDescending(s => s.Change).Take(2);
            foreach (var t in top)
            {
                sb.AppendLine($"  • {t.Name}: +{t.Change:F2}%");
            }
        }

        sb.AppendLine("═══════════════════════════════");
        sb.AppendLine($"📅 下午交易时间: 13:00-15:00");

        return sb.ToString();
    }
}

public class MarketSummary
{
    public string Date { get; set; } = "";
    public string TimePeriod { get; set; } = "";
    public string TimeRange { get; set; } = "";
    public int TotalStocks { get; set; }
    public int UpCount { get; set; }
    public int DownCount { get; set; }
    public int FlatCount { get; set; }
    public decimal AvgChange { get; set; }
    public List<SectorStat> Sectors { get; set; } = new();
    public List<StockQuote> TopGainers { get; set; } = new();
    public List<StockQuote> TopLosers { get; set; } = new();
    public List<StockQuote> BigMoneyMoves { get; set; } = new();
    public string Sentiment { get; set; } = "";
    public string Summary { get; set; } = "";
}

public class SectorStat
{
    public string Name { get; set; } = "";
    public decimal Change { get; set; }
    public int UpCount { get; set; }
    public int TotalCount { get; set; }
}

public class StockQuote
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Change { get; set; }
}