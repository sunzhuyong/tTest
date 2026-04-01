using System.Net.Http;
using Newtonsoft.Json;
using TradingSimulator.Models;

namespace TradingSimulator.Services;

/// <summary>
/// 行情数据服务 - 使用新浪财经API
/// </summary>
public class MarketDataService
{
    private readonly HttpClient _httpClient;

    public MarketDataService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// 获取股票实时行情
    /// </summary>
    /// <param name="code">股票代码，如 600000</param>
    public async Task<Security?> GetStockQuoteAsync(string code)
    {
        try
        {
            // 新浪财经股票接口
            var url = $"http://hq.sinajs.cn/list=sh{code},sz{code}";
            var response = await _httpClient.GetStringAsync(url);

            return ParseSinaStockResponse(response, code);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取股票行情失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取基金实时行情
    /// </summary>
    /// <param name="code">基金代码，如 161039</param>
    public async Task<Security?> GetFundQuoteAsync(string code)
    {
        try
        {
            // 新浪财经基金接口
            var url = $"http://hq.sinajs.cn/list=f_{code}";
            var response = await _httpClient.GetStringAsync(url);

            return ParseSinaFundResponse(response, code);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取基金行情失败: {ex.Message}");
            return null;
        }
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
            if (code.StartsWith("16") || code.StartsWith("00") || code.StartsWith("15"))
            {
                // 基金代码特征
                security = await GetFundQuoteAsync(code);
            }
            else
            {
                security = await GetStockQuoteAsync(code);
            }

            if (security != null)
                results.Add(security);
        }

        return results;
    }

    /// <summary>
    /// 解析新浪股票响应
    /// </summary>
    private Security? ParseSinaStockResponse(string response, string code)
    {
        try
        {
            // 格式: var hq_str_sh600000="浦发银行,10.50,10.48,10.52,10.45,10.51,50000000,..."
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
            var volume = decimal.Parse(fields[8]) / 10000; // 手
            var amount = decimal.Parse(fields[9]) / 10000; // 万元

            // 判断上海/深圳
            var isShanghai = code.StartsWith("6");
            var fullCode = isShanghai ? "sh" + code : "sz" + code;

            return new Security
            {
                Code = fullCode,
                Name = name,
                Type = SecurityType.Stock,
                OpenPrice = open,
                CurrentPrice = current,
                HighPrice = high,
                LowPrice = low,
                Volume = volume,
                Amount = amount,
                ChangePercent = preClose == 0 ? 0 : (current - preClose) / preClose * 100,
                UpdateTime = DateTime.Now
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 解析新浪基金响应
    /// </summary>
    private Security? ParseSinaFundResponse(string response, string code)
    {
        try
        {
            // 格式: var f_161039="1.2345,1.2300,1.2350,1.2280,5000000,2025-01-15..."
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
                Name = $"基金{code}",
                Type = SecurityType.Fund,
                OpenPrice = preClose,
                CurrentPrice = current,
                HighPrice = high,
                LowPrice = low,
                Volume = volume,
                ChangePercent = preClose == 0 ? 0 : (current - preClose) / preClose * 100,
                UpdateTime = DateTime.Now
            };
        }
        catch
        {
            return null;
        }
    }
}
