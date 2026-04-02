using System.Net.Http;
using Newtonsoft.Json;

namespace TradingSimulator.Services;

/// <summary>
/// 飞书通知服务
/// </summary>
public class FeishuNotifyService
{
    private readonly string _webhookUrl = "https://open.feishu.cn/open-apis/bot/v2/hook/0260cbcc-f091-4a82-8ad3-27bd664af86c";
    private readonly HttpClient _httpClient;
    private readonly bool _enabled = true;

    public FeishuNotifyService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    /// <summary>
    /// 发送交易通知
    /// </summary>
    public async Task SendTradeNotification(string title, string message)
    {
        if (!_enabled) return;

        try
        {
            var data = new
            {
                msg_type = "text",
                content = new { text = $"📈 {title}\n{message}" }
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[飞书通知] 发送结果: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[飞书通知] 发送失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送买入通知
    /// </summary>
    public async Task SendBuyNotification(string code, string name, int quantity, decimal price, decimal amount)
    {
        var message = $"买入 {name} ({code})\n数量: {quantity}股\n价格: ¥{price:N2}\n金额: ¥{amount:N2}";
        await SendTradeNotification("【买入提醒】", message);
    }

    /// <summary>
    /// 发送卖出通知
    /// </summary>
    public async Task SendSellNotification(string code, string name, int quantity, decimal price, decimal amount, decimal profitLoss)
    {
        var plText = profitLoss >= 0 ? $"盈利 ¥{profitLoss:N2}" : $"亏损 ¥{Math.Abs(profitLoss):N2}";
        var message = $"卖出 {name} ({code})\n数量: {quantity}股\n价格: ¥{price:N2}\n金额: ¥{amount:N2}\n{plText}";
        await SendTradeNotification("【卖出提醒】", message);
    }

    /// <summary>
    /// 发送账户变动通知
    /// </summary>
    public async Task SendAccountNotification(decimal totalAssets, decimal profitLoss, decimal profitPercent)
    {
        var plText = profitLoss >= 0 ? $"盈利 +¥{profitLoss:N2}" : $"亏损 -¥{Math.Abs(profitLoss):N2}";
        var message = $"总资产: ¥{totalAssets:N2}\n{plText} ({profitPercent:+0.00;-0.00}%)";
        await SendTradeNotification("【账户变动】", message);
    }

    /// <summary>
    /// 发送系统状态通知
    /// </summary>
    public async Task SendSystemNotification(string message)
    {
        await SendTradeNotification("【系统通知】", message);
    }
}
