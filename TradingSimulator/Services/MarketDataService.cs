using System.Net.Http;
using Newtonsoft.Json;
using TradingSimulator.Models;

namespace TradingSimulator.Services;

/// <summary>
/// 行情数据服务 - 支持真实API和模拟数据
/// </summary>
public class MarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly bool _useMockData = false;

    // 备用静态价格（非交易时间使用）
    private readonly Dictionary<string, (string name, decimal basePrice)> _staticStocks = new()
    {
        // 银行
        ["600000"] = ("浦发银行", 10.50m),
        ["600036"] = ("招商银行", 35.80m),
        ["000001"] = ("平安银行", 12.30m),
        ["601398"] = ("工商银行", 5.20m),
        ["601939"] = ("建设银行", 5.80m),
        // 白酒消费
        ["600519"] = ("贵州茅台", 1458.00m),
        ["000858"] = ("五粮液", 148.50m),
        ["601888"] = ("中国中免", 68.90m),
        // 新能源
        ["300750"] = ("宁德时代", 180.20m),
        ["601318"] = ("中国平安", 48.60m),
        ["600850"] = ("华东医药", 28.50m),
        // 核电
        ["601985"] = ("中国核电", 8.50m),
        ["600175"] = ("中核技术", 12.80m),
        ["600875"] = ("东方电气", 15.60m),
        ["601727"] = ("上海电气", 10.20m),
        // 半导体/芯片 (剔除科创板)
        ["603986"] = ("兆易创新", 120.00m),
        ["603160"] = ("汇顶科技", 65.00m),
        ["603501"] = ("北京君正", 85.00m),
        // 医药医疗
        ["600276"] = ("恒瑞医药", 52.00m),
        ["300760"] = ("迈瑞医疗", 280.00m),
        ["002044"] = ("江苏国泰", 16.50m),
        ["300015"] = ("爱尔眼科", 25.00m),
        // 房地产
        ["000002"] = ("万科A", 10.50m),
        ["600048"] = ("保利发展", 15.20m),
        ["601155"] = ("新城控股", 25.80m),
        // 军工-航空航天
        ["600893"] = ("航发动力", 45.00m),
        ["600038"] = ("中直股份", 55.00m),
        ["000547"] = ("航天发展", 12.00m),
        ["600879"] = ("航天机电", 8.50m),
        ["600271"] = ("航天信息", 18.00m),
        ["600118"] = ("中国卫星", 28.00m),
        // 机器人
        ["300124"] = ("汇川技术", 65.00m),
        ["000333"] = ("美的集团", 58.00m),
        ["600835"] = ("上海机电", 14.00m),
        ["002139"] = ("拓普集团", 32.00m),
        ["300005"] = ("聚龙股份", 8.50m),
        // AI应用
        ["300033"] = ("同花顺", 95.00m),
        ["300229"] = ("拓尔思", 15.00m),
        ["300459"] = ("汤姆猫", 6.50m),
        ["300467"] = ("迅游科技", 25.00m),
        ["002195"] = ("二三四五", 3.80m),
        ["002027"] = ("分众传媒", 6.20m),
        // 科技互联网
        ["600570"] = ("恒生电子", 58.00m),
        ["002410"] = ("广联达", 45.00m),
        // 煤炭
        ["601001"] = ("大同煤业", 6.50m),
        ["601088"] = ("陕西煤业", 18.00m),
        ["600395"] = ("盘江股份", 8.20m),
        // 钢铁
        ["600019"] = ("宝钢股份", 7.50m),
        ["600581"] = ("南钢股份", 5.80m),
        // 化工
        ["600309"] = ("万华化学", 95.00m),
        ["600096"] = ("云天化", 25.00m),
    };

    private readonly Dictionary<string, (string name, decimal basePrice)> _staticFunds = new()
    {
        ["161039"] = ("易方达上证50ETF", 3.256m),
        ["510300"] = ("华泰柏瑞沪深300ETF", 3.890m),
        ["159915"] = ("易方达创业板ETF", 2.145m),
        ["161725"] = ("招商中证白酒指数", 1.234m),
        ["110022"] = ("易方达消费行业股票", 1.567m),
    };

    private readonly Random _random = new();

    public MarketDataService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// 获取股票实时行情
    /// </summary>
    public async Task<Security?> GetStockQuoteAsync(string code)
    {
        // 非交易时间用静态价格
        if (!MarketTime.IsTradingTime())
            return GetStaticStockQuote(code);

        try
        {
            // 尝试东方财富API
            var quote = await GetFromEastMoneyAsync(code);
            // 如果API返回0价格（非交易时间），使用静态价格
            if (quote != null && quote.CurrentPrice > 0)
                return quote;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[行情] {code} 东方财富失败: {ex.Message}");
        }

        // 返回静态价格
        return GetStaticStockQuote(code);
    }

    /// <summary>
    /// 获取基金实时行情
    /// </summary>
    public async Task<Security?> GetFundQuoteAsync(string code)
    {
        if (!MarketTime.IsTradingTime())
            return GetStaticFundQuote(code);

        try
        {
            var quote = await GetFromEastMoneyFundAsync(code);
            if (quote != null) return quote;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[基金] {code} 失败: {ex.Message}");
        }

        return GetStaticFundQuote(code);
    }

    /// <summary>
    /// 东方财富API - 股票
    /// </summary>
    private async Task<Security?> GetFromEastMoneyAsync(string code)
    {
        if (code.StartsWith("688"))
            return null;

        var symbol = code.StartsWith("6") ? "1." + code : "0." + code;
        var url = $"https://push2.eastmoney.com/api/qt/stock/get?secid={symbol}&fields=f43,f44,f45,f46,f47,f48,f49,f50,f51,f52,f55,f57,f58,f59,f60,f116,f117,f162,f167,f168,f169,f170,f171,f173,f177,f178,f187,f188,f189,f190,f191,f192,f193";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        request.Headers.Add("Referer", "https://quote.eastmoney.com/");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<dynamic>(json);
        if (data?.data == null) return null;

        var stockData = data.data;
        var name = (string)stockData.f58;
        var current = (decimal)stockData.f43 / 100m;
        var preClose = (decimal)stockData.f44 / 100m;
        var open = (decimal)stockData.f46 / 100m;
        var high = (decimal)stockData.f45 / 100m;
        var low = (decimal)stockData.f47 / 100m;
        var volume = (decimal)stockData.f48 / 10000m;
        var amount = (decimal)stockData.f49 / 10000m;
        var change = (decimal)stockData.f170 / 100m;

        return new Security
        {
            Code = code.StartsWith("6") ? "sh" + code : "sz" + code,
            Name = name,
            Type = SecurityType.Stock,
            OpenPrice = open,
            CurrentPrice = current,
            HighPrice = high,
            LowPrice = low,
            Volume = volume,
            Amount = amount,
            ChangePercent = change,
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 东方财富API - 基金
    /// </summary>
    private async Task<Security?> GetFromEastMoneyFundAsync(string code)
    {
        var url = $"https://fund.eastmoney.com/pingzhongdata/{code}.js";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0");

        var response = await _httpClient.SendAsync(request);
        var jsContent = await response.Content.ReadAsStringAsync();

        // 解析JS中的数据
        var gszMatch = System.Text.RegularExpressions.Regex.Match(jsContent, @"gsz=""([0-9.]+)""");
        var gszPercentMatch = System.Text.RegularExpressions.Regex.Match(jsContent, @"gszPercent=""(-?[0-9.]+)""");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(jsContent, @"name=""([^""]+)""");

        if (!gszMatch.Success) return null;

        var current = decimal.Parse(gszMatch.Groups[1].Value);
        var change = gszPercentMatch.Success ? decimal.Parse(gszPercentMatch.Groups[1].Value) : 0m;
        var name = nameMatch.Success ? nameMatch.Groups[1].Value : $"基金{code}";

        return new Security
        {
            Code = code,
            Name = name,
            Type = SecurityType.Fund,
            OpenPrice = current / (1 + change / 100),
            CurrentPrice = current,
            HighPrice = current * 1.01m,
            LowPrice = current * 0.99m,
            Volume = 0,
            ChangePercent = change,
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 静态股票价格（非交易时间）
    /// </summary>
    private Security? GetStaticStockQuote(string code)
    {
        if (code.StartsWith("688"))
            return null;

        if (!_staticStocks.TryGetValue(code, out var stock))
        {
            stock = ($"股票{code}", 10.00m);
        }

        // 使用昨日收盘价作为基准，计算涨跌幅
        // 这里简化处理：假设静态价格就是收盘价，涨跌幅为0
        // 如果需要真实涨跌幅，需要在每天收盘时更新静态价格
        var changePercent = 0m;  // 静态价格无法计算当日涨跌幅，默认0

        return new Security
        {
            Code = code.StartsWith("6") ? "sh" + code : "sz" + code,
            Name = stock.name,
            Type = SecurityType.Stock,
            OpenPrice = stock.basePrice,
            CurrentPrice = stock.basePrice,
            HighPrice = stock.basePrice,
            LowPrice = stock.basePrice,
            Volume = 0,
            Amount = 0,
            ChangePercent = 0,
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 静态基金价格
    /// </summary>
    private Security? GetStaticFundQuote(string code)
    {
        if (!_staticFunds.TryGetValue(code, out var fund))
        {
            fund = ($"基金{code}", 1.50m);
        }

        return new Security
        {
            Code = code,
            Name = fund.name,
            Type = SecurityType.Fund,
            OpenPrice = fund.basePrice,
            CurrentPrice = fund.basePrice,
            HighPrice = fund.basePrice,
            LowPrice = fund.basePrice,
            Volume = 0,
            Amount = 0,
            ChangePercent = 0,
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 批量获取行情
    /// </summary>
    public async Task<List<Security>> GetQuotesAsync(List<string> codes)
    {
        var results = new List<Security>();

        foreach (var code in codes)
        {
            Security? security;
            if (code.StartsWith("16") || code.StartsWith("15") || code.StartsWith("11"))
                security = await GetFundQuoteAsync(code);
            else
                security = await GetStockQuoteAsync(code);

            if (security != null)
                results.Add(security);
        }

        return results;
    }
}
