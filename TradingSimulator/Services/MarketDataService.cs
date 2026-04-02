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
    private readonly bool _useMockData = false;  // 默认使用真实数据

    // 模拟备用数据（真实API失败时使用）
    private readonly Dictionary<string, (string name, decimal basePrice)> _mockStocks = new()
    {
        ["600000"] = ("浦发银行", 10.50m),
        ["600036"] = ("招商银行", 35.80m),
        ["600519"] = ("贵州茅台", 1680.00m),
        ["000001"] = ("平安银行", 12.30m),
        ["000858"] = ("五粮液", 148.50m),
        ["300750"] = ("宁德时代", 180.20m),
        ["601318"] = ("中国平安", 48.60m),
        ["601888"] = ("中国中免", 68.90m),
    };

    private readonly Dictionary<string, (string name, decimal basePrice)> _mockFunds = new()
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
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    /// <summary>
    /// 获取股票实时行情
    /// </summary>
    public async Task<Security?> GetStockQuoteAsync(string code)
    {
        if (_useMockData)
            return GetMockStockQuote(code);

        try
        {
            // 尝试新浪财经API
            var quote = await GetFromSinaAsync(code);
            if (quote != null) return quote;

            // 尝试腾讯财经API
            quote = await GetFromTencentAsync(code);
            if (quote != null) return quote;

            // 尝试网易财经API
            quote = await GetFrom163Async(code);
            if (quote != null) return quote;

            Console.WriteLine($"[行情] {code} 获取真实报价失败，使用模拟数据");
            return GetMockStockQuote(code);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[行情] {code} 获取失败: {ex.Message}，使用模拟数据");
            return GetMockStockQuote(code);
        }
    }

    /// <summary>
    /// 获取基金实时行情
    /// </summary>
    public async Task<Security?> GetFundQuoteAsync(string code)
    {
        if (_useMockData)
            return GetMockFundQuote(code);

        try
        {
            // 尝试新浪基金API
            var quote = await GetFromSinaFundAsync(code);
            if (quote != null) return quote;

            Console.WriteLine($"[基金] {code} 获取失败，使用模拟数据");
            return GetMockFundQuote(code);
        }
        catch
        {
            return GetMockFundQuote(code);
        }
    }

    /// <summary>
    /// 新浪财经API
    /// </summary>
    private async Task<Security?> GetFromSinaAsync(string code)
    {
        // 剔除科创板
        if (code.StartsWith("688"))
            return null;

        var url = $"http://hq.sinajs.cn/list=sh{code},sz{code}";
        var response = await _httpClient.GetStringAsync(url);

        var start = response.IndexOf('=');
        if (start < 0) return null;

        var data = response.Substring(start + 1).Trim('"', '\n', '\r');
        var fields = data.Split(',');

        if (fields.Length < 10 || string.IsNullOrEmpty(fields[0]))
            return null;

        var name = fields[0];
        var open = decimal.Parse(fields[1]);
        var preClose = decimal.Parse(fields[2]);
        var current = decimal.Parse(fields[3]);
        var high = decimal.Parse(fields[4]);
        var low = decimal.Parse(fields[5]);
        var volume = decimal.Parse(fields[8]) / 10000;
        var amount = decimal.Parse(fields[9]) / 10000;

        var isShanghai = code.StartsWith("6");

        return new Security
        {
            Code = isShanghai ? "sh" + code : "sz" + code,
            Name = name,
            Type = SecurityType.Stock,
            OpenPrice = open,
            CurrentPrice = current,
            HighPrice = high,
            LowPrice = low,
            Volume = volume,
            Amount = amount,
            ChangePercent = preClose == 0 ? 0 : Math.Round((current - preClose) / preClose * 100, 2),
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 腾讯财经API
    /// </summary>
    private async Task<Security?> GetFromTencentAsync(string code)
    {
        if (code.StartsWith("688"))
            return null;

        var qs = code.StartsWith("6") ? "sh" + code : "sz" + code;
        var url = $"https://qt.gtimg.cn/q={qs}";

        var response = await _httpClient.GetStringAsync(url);
        if (string.IsNullOrEmpty(response) || response.Length < 50)
            return null;

        // 格式: v_sh600000="1~浦发银行~10.50~10.48~10.52~10.45~10.51~50000000~500000000~..."
        var parts = response.Split('"');
        if (parts.Length < 2) return null;

        var fields = parts[1].Split('~');
        if (fields.Length < 50) return null;

        return new Security
        {
            Code = code.StartsWith("6") ? "sh" + code : "sz" + code,
            Name = fields[1],
            OpenPrice = decimal.Parse(fields[5]),
            CurrentPrice = decimal.Parse(fields[3]),
            HighPrice = decimal.Parse(fields[33]),
            LowPrice = decimal.Parse(fields[34]),
            Volume = decimal.Parse(fields[6]) / 10000,
            Amount = decimal.Parse(fields[7]) / 10000,
            ChangePercent = decimal.Parse(fields[38]),
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 网易财经API
    /// </summary>
    private async Task<Security?> GetFrom163Async(string code)
    {
        if (code.StartsWith("688"))
            return null;

        var qs = code.StartsWith("6") ? "1" + code : "0" + code;
        var url = $"http://quotes.money.163.com/service/chddata.html?code={qs}&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER";

        var response = await _httpClient.GetStringAsync(url);
        if (string.IsNullOrEmpty(response) || !response.Contains(","))
            return null;

        var lines = response.Split('\n');
        if (lines.Length < 2) return null;

        var lastLine = lines[^2];
        var fields = lastLine.Split(',');

        if (fields.Length < 10) return null;

        return new Security
        {
            Code = code.StartsWith("6") ? "sh" + code : "sz" + code,
            Name = fields[2],
            CurrentPrice = decimal.Parse(fields[3]),
            HighPrice = decimal.Parse(fields[4]),
            LowPrice = decimal.Parse(fields[5]),
            OpenPrice = decimal.Parse(fields[6]),
            ChangePercent = decimal.Parse(fields[9].Trim('%')),
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 新浪基金API
    /// </summary>
    private async Task<Security?> GetFromSinaFundAsync(string code)
    {
        var url = $"http://hq.sinajs.cn/list=f_{code}";
        var response = await _httpClient.GetStringAsync(url);

        var start = response.IndexOf('=');
        if (start < 0) return null;

        var data = response.Substring(start + 1).Trim('"', '\n', '\r');
        var fields = data.Split(',');

        if (fields.Length < 6 || string.IsNullOrEmpty(fields[0]))
            return null;

        var current = decimal.Parse(fields[0]);
        var preClose = decimal.Parse(fields[1]);
        var high = decimal.Parse(fields[2]);
        var low = decimal.Parse(fields[3]);
        var volume = decimal.Parse(fields[4]) / 10000;

        return new Security
        {
            Code = code,
            Name = _mockFunds.TryGetValue(code, out var f) ? f.name : $"基金{code}",
            Type = SecurityType.Fund,
            OpenPrice = preClose,
            CurrentPrice = current,
            HighPrice = high,
            LowPrice = low,
            Volume = volume,
            ChangePercent = Math.Round((current - preClose) / preClose * 100, 2),
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 模拟股票行情
    /// </summary>
    private Security? GetMockStockQuote(string code)
    {
        if (code.StartsWith("688"))
            return null;

        if (!_mockStocks.TryGetValue(code, out var stock))
        {
            stock = ($"股票{code}", 10.00m + (decimal)(_random.NextDouble() * 90));
        }

        var changePercent = (decimal)(_random.NextDouble() * 10 - 5);
        var currentPrice = stock.basePrice * (1 + changePercent / 100);

        return new Security
        {
            Code = code.StartsWith("6") ? "sh" + code : "sz" + code,
            Name = stock.name,
            Type = SecurityType.Stock,
            OpenPrice = stock.basePrice,
            CurrentPrice = Math.Round(currentPrice, 2),
            HighPrice = Math.Round(currentPrice * 1.02m, 2),
            LowPrice = Math.Round(currentPrice * 0.98m, 2),
            Volume = _random.Next(1000, 100000),
            Amount = _random.Next(10000, 1000000),
            ChangePercent = Math.Round(changePercent, 2),
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 模拟基金行情
    /// </summary>
    private Security? GetMockFundQuote(string code)
    {
        if (!_mockFunds.TryGetValue(code, out var fund))
        {
            fund = ($"基金{code}", 1.00m + (decimal)(_random.NextDouble() * 3));
        }

        var changePercent = (decimal)(_random.NextDouble() * 6 - 3);
        var currentPrice = fund.basePrice * (1 + changePercent / 100);

        return new Security
        {
            Code = code,
            Name = fund.name,
            Type = SecurityType.Fund,
            OpenPrice = fund.basePrice,
            CurrentPrice = Math.Round(currentPrice, 3),
            HighPrice = Math.Round(currentPrice * 1.01m, 3),
            LowPrice = Math.Round(currentPrice * 0.99m, 3),
            Volume = _random.Next(1000, 50000),
            Amount = _random.Next(1000, 50000),
            ChangePercent = Math.Round(changePercent, 2),
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
