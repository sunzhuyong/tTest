using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace TradingSimulator.Services;

/// <summary>
/// 策略迭代服务 - 记录策略调整历史
/// </summary>
public class StrategyIterationService
{
    private readonly string _connectionString;
    private readonly string _dataFile;
    private List<StrategyIteration> _iterations = new();

    public StrategyIterationService()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "trading.db");
        var directory = Path.GetDirectoryName(dbPath)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        _connectionString = $"Data Source={dbPath}";
        _dataFile = Path.Combine(directory, "strategy_iterations.json");

        InitializeTable();
        LoadData();
    }

    private void InitializeTable()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = @"
            CREATE TABLE IF NOT EXISTS StrategyIterations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                Type TEXT NOT NULL,
                BeforeParams TEXT NOT NULL,
                AfterParams TEXT NOT NULL,
                Reason TEXT NOT NULL,
                Result TEXT,
                CreatedAt TEXT NOT NULL
            )";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(_dataFile))
            {
                var json = File.ReadAllText(_dataFile);
                _iterations = JsonConvert.DeserializeObject<List<StrategyIteration>>(json) ?? new List<StrategyIteration>();
            }
        }
        catch { }
    }

    private void SaveData()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_iterations, Formatting.Indented);
            File.WriteAllText(_dataFile, json);
        }
        catch { }
    }

    /// <summary>
    /// 记录策略调整
    /// </summary>
    public void RecordIteration(string type, object beforeParams, object afterParams, string reason, string? result = null)
    {
        var iteration = new StrategyIteration
        {
            Date = DateTime.Today,
            Type = type,
            BeforeParams = JsonConvert.SerializeObject(beforeParams),
            AfterParams = JsonConvert.SerializeObject(afterParams),
            Reason = reason,
            Result = result,
            CreatedAt = DateTime.Now
        };

        _iterations.Add(iteration);
        SaveData();

        // 同时存数据库
        SaveToDatabase(iteration);

        Console.WriteLine($"[策略迭代] {type}: {reason}");
    }

    private void SaveToDatabase(StrategyIteration iteration)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"INSERT INTO StrategyIterations (Date, Type, BeforeParams, AfterParams, Reason, Result, CreatedAt)
                        VALUES (@Date, @Type, @Before, @After, @Reason, @Result, @Created)";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Date", iteration.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Type", iteration.Type);
            cmd.Parameters.AddWithValue("@Before", iteration.BeforeParams);
            cmd.Parameters.AddWithValue("@After", iteration.AfterParams);
            cmd.Parameters.AddWithValue("@Reason", iteration.Reason);
            cmd.Parameters.AddWithValue("@Result", iteration.Result ?? "");
            cmd.Parameters.AddWithValue("@Created", iteration.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    /// <summary>
    /// 获取策略迭代历史
    /// </summary>
    public List<StrategyIteration> GetHistory(int days = 90)
    {
        var cutoff = DateTime.Today.AddDays(-days);
        return _iterations.Where(i => i.Date >= cutoff).OrderByDescending(i => i.Date).ToList();
    }

    /// <summary>
    /// 获取最新迭代
    /// </summary>
    public StrategyIteration? GetLatest()
    {
        return _iterations.LastOrDefault();
    }

    /// <summary>
    /// 分析策略效果并给出建议
    /// </summary>
    public List<string> AnalyzeAndSuggest(decimal totalProfitLoss, int tradeCount, int winningTrades, int losingTrades)
    {
        var suggestions = new List<string>();

        // 计算胜率
        var winRate = tradeCount > 0 ? (decimal)winningTrades / tradeCount * 100 : 0;

        // 亏损严重，建议收紧
        if (totalProfitLoss < -100)
        {
            suggestions.Add("⚠️ 亏损较大，建议收紧止损线（-5%改为-3%）");
            suggestions.Add("⚠️ 减少仓位，控制在50%以下");
        }

        // 盈利良好，建议放宽
        if (totalProfitLoss > 200 && winRate > 50)
        {
            suggestions.Add("✅ 策略表现良好，可考虑适当放宽买入条件");
            suggestions.Add("✅ 增加仓位到70%");
        }

        // 胜率低
        if (winRate < 30 && tradeCount > 5)
        {
            suggestions.Add("⚠️ 胜率较低，建议优化选股条件");
            suggestions.Add("⚠️ 减少追高买入，提高安全边际");
        }

        // 无交易
        if (tradeCount == 0)
        {
            suggestions.Add("💡 无交易，可能是策略过于保守，可放宽买入条件");
        }

        return suggestions;
    }
}

/// <summary>
/// 策略迭代记录
/// </summary>
public class StrategyIteration
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = "";  // 如 "止损策略", "选股条件", "仓位管理"
    public string BeforeParams { get; set; } = "";
    public string AfterParams { get; set; } = "";
    public string Reason { get; set; } = "";
    public string? Result { get; set; }
    public DateTime CreatedAt { get; set; }
}
