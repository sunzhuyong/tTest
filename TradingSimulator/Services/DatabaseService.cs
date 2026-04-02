using Microsoft.Data.Sqlite;
using TradingSimulator.Models;

namespace TradingSimulator.Services;

/// <summary>
/// 数据库服务 - SQLite
/// </summary>
public class DatabaseService
{
    private readonly string _connectionString;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "trading.db");
        var directory = Path.GetDirectoryName(_dbPath)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 持仓表
        var createPositionTable = @"
            CREATE TABLE IF NOT EXISTS Positions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Quantity REAL NOT NULL,
                CostPrice REAL NOT NULL,
                PurchaseDate TEXT NOT NULL
            )";

        // 交易记录表
        var createTradeTable = @"
            CREATE TABLE IF NOT EXISTS TradeRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Direction INTEGER NOT NULL,
                Price REAL NOT NULL,
                Quantity REAL NOT NULL,
                Amount REAL NOT NULL,
                Commission REAL NOT NULL,
                ProfitLoss REAL NOT NULL DEFAULT 0,
                TradeTime TEXT NOT NULL
            )";

        // 账户设置表
        var createAccountTable = @"
            CREATE TABLE IF NOT EXISTS AccountSettings (
                Id INTEGER PRIMARY KEY,
                InitialCapital REAL NOT NULL,
                AvailableCash REAL NOT NULL
            )";

        using var cmd1 = new SqliteCommand(createPositionTable, connection);
        cmd1.ExecuteNonQuery();

        using var cmd2 = new SqliteCommand(createTradeTable, connection);
        cmd2.ExecuteNonQuery();

        using var cmd3 = new SqliteCommand(createAccountTable, connection);
        cmd3.ExecuteNonQuery();

        // 初始化账户
        InitializeAccount(connection);
    }

    private void InitializeAccount(SqliteConnection connection)
    {
        var checkSql = "SELECT COUNT(*) FROM AccountSettings";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (count == 0)
        {
            var insertSql = "INSERT INTO AccountSettings (Id, InitialCapital, AvailableCash) VALUES (1, 30000, 30000)";
            using var insertCmd = new SqliteCommand(insertSql, connection);
            insertCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// 获取账户信息
    /// </summary>
    public Account GetAccount()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = "SELECT InitialCapital, AvailableCash FROM AccountSettings WHERE Id = 1";
        using var cmd = new SqliteCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        var account = new Account();
        if (reader.Read())
        {
            account.InitialCapital = (decimal)reader.GetDouble(0);
            account.AvailableCash = (decimal)reader.GetDouble(1);
        }

        // 计算持仓市值
        account.MarketValue = GetTotalMarketValue(connection);

        return account;
    }

    /// <summary>
    /// 更新账户可用资金
    /// </summary>
    public void UpdateAccountCash(decimal newCash)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = "UPDATE AccountSettings SET AvailableCash = @Cash WHERE Id = 1";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Cash", (double)newCash);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取所有持仓
    /// </summary>
    public List<Position> GetPositions()
    {
        var positions = new List<Position>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = "SELECT Id, Code, Name, Type, Quantity, CostPrice, PurchaseDate FROM Positions";
        using var cmd = new SqliteCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            positions.Add(new Position
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Type = (SecurityType)reader.GetInt32(3),
                Quantity = (decimal)reader.GetDouble(4),
                CostPrice = (decimal)reader.GetDouble(5),
                PurchaseDate = DateTime.Parse(reader.GetString(6))
            });
        }

        return positions;
    }

    /// <summary>
    /// 添加持仓
    /// </summary>
    public void AddPosition(Position position)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 检查是否已存在
        var checkSql = "SELECT Id, Quantity, CostPrice FROM Positions WHERE Code = @Code";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@Code", position.Code);
        using var reader = checkCmd.ExecuteReader();

        if (reader.Read())
        {
            // 更新现有持仓
            var existingId = reader.GetInt32(0);
            var existingQty = (decimal)reader.GetDouble(1);
            var existingCost = (decimal)reader.GetDouble(2);

            reader.Close();

            // 计算新的平均成本
            var totalQty = existingQty + position.Quantity;
            var totalCost = existingQty * existingCost + position.Quantity * position.CostPrice;
            var newCostPrice = totalCost / totalQty;

            var updateSql = "UPDATE Positions SET Quantity = @Qty, CostPrice = @Cost WHERE Id = @Id";
            using var updateCmd = new SqliteCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@Qty", (double)totalQty);
            updateCmd.Parameters.AddWithValue("@Cost", (double)newCostPrice);
            updateCmd.Parameters.AddWithValue("@Id", existingId);
            updateCmd.ExecuteNonQuery();
        }
        else
        {
            reader.Close();

            var insertSql = @"INSERT INTO Positions (Code, Name, Type, Quantity, CostPrice, PurchaseDate)
                              VALUES (@Code, @Name, @Type, @Qty, @Cost, @Date)";
            using var insertCmd = new SqliteCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@Code", position.Code);
            insertCmd.Parameters.AddWithValue("@Name", position.Name);
            insertCmd.Parameters.AddWithValue("@Type", (int)position.Type);
            insertCmd.Parameters.AddWithValue("@Qty", (double)position.Quantity);
            insertCmd.Parameters.AddWithValue("@Cost", (double)position.CostPrice);
            insertCmd.Parameters.AddWithValue("@Date", position.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// 减少持仓
    /// </summary>
    public void ReducePosition(string code, decimal quantity)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var selectSql = "SELECT Quantity FROM Positions WHERE Code = @Code";
        using var selectCmd = new SqliteCommand(selectSql, connection);
        selectCmd.Parameters.AddWithValue("@Code", code);
        var qty = Convert.ToDecimal(selectCmd.ExecuteScalar());

        if (qty <= quantity)
        {
            // 全部卖出，删除持仓
            var deleteSql = "DELETE FROM Positions WHERE Code = @Code";
            using var deleteCmd = new SqliteCommand(deleteSql, connection);
            deleteCmd.Parameters.AddWithValue("@Code", code);
            deleteCmd.ExecuteNonQuery();
        }
        else
        {
            // 部分卖出
            var updateSql = "UPDATE Positions SET Quantity = Quantity - @Qty WHERE Code = @Code";
            using var updateCmd = new SqliteCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@Qty", (double)quantity);
            updateCmd.Parameters.AddWithValue("@Code", code);
            updateCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// 添加交易记录
    /// </summary>
    public void AddTradeRecord(TradeRecord record)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = @"INSERT INTO TradeRecords (Code, Name, Type, Direction, Price, Quantity, Amount, Commission, ProfitLoss, TradeTime)
                    VALUES (@Code, @Name, @Type, @Dir, @Price, @Qty, @Amount, @Comm, @PL, @Time)";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Code", record.Code);
        cmd.Parameters.AddWithValue("@Name", record.Name);
        cmd.Parameters.AddWithValue("@Type", (int)record.Type);
        cmd.Parameters.AddWithValue("@Dir", (int)record.Direction);
        cmd.Parameters.AddWithValue("@Price", (double)record.Price);
        cmd.Parameters.AddWithValue("@Qty", (double)record.Quantity);
        cmd.Parameters.AddWithValue("@Amount", (double)record.Amount);
        cmd.Parameters.AddWithValue("@Comm", (double)record.Commission);
        cmd.Parameters.AddWithValue("@PL", (double)record.ProfitLoss);
        cmd.Parameters.AddWithValue("@Time", record.TradeTime.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取交易记录
    /// </summary>
    public List<TradeRecord> GetTradeRecords()
    {
        var records = new List<TradeRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = "SELECT * FROM TradeRecords ORDER BY TradeTime DESC";
        using var cmd = new SqliteCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var record = new TradeRecord
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Type = (SecurityType)reader.GetInt32(3),
                Direction = (TradeDirection)reader.GetInt32(4),
                Price = (decimal)reader.GetDouble(5),
                Quantity = (decimal)reader.GetDouble(6),
                Amount = (decimal)reader.GetDouble(7),
                Commission = (decimal)reader.GetDouble(8),
                ProfitLoss = reader.GetDouble(9) > 0 ? (decimal)reader.GetDouble(9) : (decimal)reader.GetDouble(9),
                TradeTime = DateTime.Parse(reader.GetString(10))
            };
            records.Add(record);
        }

        return records;
    }

    /// <summary>
    /// 重置账户
    /// </summary>
    public void ResetAccount(decimal initialCapital)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 重置账户资金
        var updateSql = "UPDATE AccountSettings SET InitialCapital = @Capital, AvailableCash = @Cash WHERE Id = 1";
        using var updateCmd = new SqliteCommand(updateSql, connection);
        updateCmd.Parameters.AddWithValue("@Capital", (double)initialCapital);
        updateCmd.Parameters.AddWithValue("@Cash", (double)initialCapital);
        updateCmd.ExecuteNonQuery();

        // 清空持仓和交易记录
        using var deletePosCmd = new SqliteCommand("DELETE FROM Positions", connection);
        deletePosCmd.ExecuteNonQuery();

        using var deleteTradeCmd = new SqliteCommand("DELETE FROM TradeRecords", connection);
        deleteTradeCmd.ExecuteNonQuery();
    }

    private decimal GetTotalMarketValue(SqliteConnection connection)
    {
        var sql = "SELECT Code, Quantity FROM Positions";
        using var cmd = new SqliteCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        var total = 0m;
        var codes = new List<string>();

        while (reader.Read())
        {
            codes.Add(reader.GetString(0));
        }

        // 这里简化处理，实际需要实时更新价格
        return total;
    }
}
