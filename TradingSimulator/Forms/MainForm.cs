using TradingSimulator.Services;

namespace TradingSimulator.Forms;

public partial class MainForm : Form
{
    private readonly MarketDataService _marketService;
    private readonly Services.DatabaseService _dbService;
    private readonly Services.TradingService _tradingService;
    private readonly Services.AutoTraderService _autoTrader;

    private TextBox txtCode;
    private TextBox txtQuantity;
    private TextBox txtPrice;
    private ComboBox cmbDirection;
    private DataGridView dgvQuote;
    private DataGridView dgvPosition;
    private DataGridView dgvTrade;
    private DataGridView dgvLog;
    private Label lblAccount;
    private Label lblAutoStatus;
    private Button btnQuery;
    private Button btnTrade;
    private Button btnRefresh;
    private Button btnStartAuto;
    private Button btnStopAuto;
    private Button btnScanNow;
    private System.Windows.Forms.Timer timer;
    private CheckBox chkAutoTrade;

    public MainForm()
    {
        _marketService = new Services.MarketDataService();
        _dbService = new Services.DatabaseService();
        _tradingService = new Services.TradingService(_dbService, _marketService);
        _autoTrader = new AutoTraderService(_tradingService, _marketService);

        InitializeComponent();
        LoadData();
        StartTimer();
        SetupAutoTrader();
    }

    private void SetupAutoTrader()
    {
        _autoTrader.OnLogUpdated += log =>
        {
            if (dgvLog.InvokeRequired)
            {
                dgvLog.Invoke(() =>
                {
                    dgvLog.Rows.Insert(0, log);
                    if (dgvLog.Rows.Count > 100)
                        dgvLog.Rows.RemoveAt(dgvLog.Rows.Count - 1);
                });
            }
            else
            {
                dgvLog.Rows.Insert(0, log);
                if (dgvLog.Rows.Count > 100)
                    dgvLog.Rows.RemoveAt(dgvLog.Rows.Count - 1);
            }
        };

        _autoTrader.OnTradeExecuted += msg =>
        {
            LoadData(); // 交易成功后刷新数据
        };
    }

    private void InitializeComponent()
    {
        this.Text = "A股智能模拟交易平台";
        this.Size = new Size(1400, 850);
        this.StartPosition = FormStartPosition.CenterScreen;

        // 账户信息面板
        var panelAccount = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(30, 30, 40)
        };

        lblAccount = new Label
        {
            Location = new Point(20, 15),
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 11F)
        };
        panelAccount.Controls.Add(lblAccount);

        // 自动交易控制面板
        var panelAuto = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.FromArgb(45, 45, 55),
            Padding = new Padding(10, 5, 10, 5)
        };

        lblAutoStatus = new Label
        {
            Text = "自动交易: 已停止",
            Location = new Point(10, 12),
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei", 10F),
            Width = 150
        };

        btnStartAuto = new Button
        {
            Text = "启动自动交易",
            Location = new Point(170, 8),
            Width = 110,
            Height = 28,
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnStartAuto.Click += (s, e) =>
        {
            _autoTrader.Start();
            lblAutoStatus.Text = "自动交易: 运行中";
            lblAutoStatus.ForeColor = Color.LimeGreen;
            btnStartAuto.Enabled = false;
            btnStopAuto.Enabled = true;
        };

        btnStopAuto = new Button
        {
            Text = "停止自动交易",
            Location = new Point(290, 8),
            Width = 110,
            Height = 28,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnStopAuto.Click += (s, e) =>
        {
            _autoTrader.Stop();
            lblAutoStatus.Text = "自动交易: 已停止";
            lblAutoStatus.ForeColor = Color.Gray;
            btnStartAuto.Enabled = true;
            btnStopAuto.Enabled = false;
        };

        btnScanNow = new Button
        {
            Text = "立即扫描",
            Location = new Point(410, 8),
            Width = 90,
            Height = 28,
            BackColor = Color.FromArgb(0, 123, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnScanNow.Click += async (s, e) =>
        {
            btnScanNow.Enabled = false;
            await _autoTrader.ScanAndTradeAsync();
            btnScanNow.Enabled = true;
        };

        var lblStrategy = new Label
        {
            Text = "策略: 中线埋伏 (不追高,提前布局)",
            Location = new Point(510, 12),
            ForeColor = Color.FromArgb(255, 193, 7),
            Font = new Font("Microsoft YaHei", 9F)
        };

        panelAuto.Controls.AddRange(new Control[] { lblAutoStatus, btnStartAuto, btnStopAuto, btnScanNow, lblStrategy });

        // 交易面板
        var panelTrade = new Panel
        {
            Dock = DockStyle.Left,
            Width = 260,
            Padding = new Padding(10)
        };

        var lblTitle = new Label
        {
            Text = "手动交易",
            Location = new Point(10, 5),
            Font = new Font("Microsoft YaHei", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 70)
        };

        var lblCode = new Label { Text = "证券代码:", Location = new Point(10, 40) };
        txtCode = new TextBox { Location = new Point(90, 38), Width = 120 };

        var lblQty = new Label { Text = "数量:", Location = new Point(10, 75) };
        txtQuantity = new TextBox { Location = new Point(90, 73), Width = 120 };

        var lblPrice = new Label { Text = "价格:", Location = new Point(10, 110) };
        txtPrice = new TextBox { Location = new Point(90, 108), Width = 120 };

        var lblDir = new Label { Text = "方向:", Location = new Point(10, 145) };
        cmbDirection = new ComboBox
        {
            Location = new Point(90, 143),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbDirection.Items.AddRange(new[] { "买入", "卖出" });
        cmbDirection.SelectedIndex = 0;

        btnTrade = new Button
        {
            Text = "执行交易",
            Location = new Point(10, 180),
            Width = 240,
            Height = 35,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnTrade.Click += BtnTrade_Click;

        var btnReset = new Button
        {
            Text = "重置账户",
            Location = new Point(10, 225),
            Width = 240,
            Height = 35,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnReset.Click += BtnReset_Click;

        // 自选股列表
        var lblWatch = new Label
        {
            Text = "快速下单:",
            Location = new Point(10, 275),
            Font = new Font("Microsoft YaHei", 9F)
        };

        var btnBuy600000 = new Button { Text = "浦发银行", Location = new Point(10, 300), Width = 75, Height = 25 };
        btnBuy600000.Click += (s, e) => QuickOrder("600000", true);

        var btnBuy600036 = new Button { Text = "招商银行", Location = new Point(90, 300), Width = 75, Height = 25 };
        btnBuy600036.Click += (s, e) => QuickOrder("600036", true);

        var btnBuy510300 = new Button { Text = "沪深300ETF", Location = new Point(170, 300), Width = 80, Height = 25 };
        btnBuy510300.Click += (s, e) => QuickOrder("510300", true);

        var btnBuy161039 = new Button { Text = "上证50ETF", Location = new Point(10, 330), Width = 75, Height = 25 };
        btnBuy161039.Click += (s, e) => QuickOrder("161039", true);

        var btnBuy110022 = new Button { Text = "消费ETF", Location = new Point(90, 330), Width = 75, Height = 25 };
        btnBuy110022.Click += (s, e) => QuickOrder("110022", true);

        panelTrade.Controls.AddRange(new Control[] {
            lblTitle, lblCode, txtCode, lblQty, txtQuantity, lblPrice, txtPrice,
            lblDir, cmbDirection, btnTrade, btnReset, lblWatch,
            btnBuy600000, btnBuy600036, btnBuy510300, btnBuy161039, btnBuy110022
        });

        // 查询面板
        var panelQuery = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(10)
        };

        var lblQueryCode = new Label { Text = "代码:", Location = new Point(10, 12) };
        var txtQueryCode = new TextBox { Location = new Point(50, 10), Width = 100, Name = "txtQueryCode" };

        btnQuery = new Button
        {
            Text = "查询行情",
            Location = new Point(160, 8),
            Width = 80
        };
        btnQuery.Click += async (s, e) =>
        {
            var code = txtQueryCode.Text.Trim();
            if (!string.IsNullOrEmpty(code))
            {
                await QueryQuoteAsync(code);
            }
        };

        btnRefresh = new Button
        {
            Text = "刷新持仓",
            Location = new Point(250, 8),
            Width = 80
        };
        btnRefresh.Click += (s, e) => LoadPosition();

        var lblInfo = new Label
        {
            Text = "提示: 股票14:30后停止自动买入, 基金14:50自动定投",
            Location = new Point(350, 12),
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei", 8F)
        };

        panelQuery.Controls.AddRange(new Control[] { lblQueryCode, txtQueryCode, btnQuery, btnRefresh, lblInfo });

        // 行情表格
        dgvQuote = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 120,
            ReadOnly = true,
            AllowUserToAddRows = false,
            BackgroundColor = Color.White
        };
        dgvQuote.Columns.Add("Code", "代码");
        dgvQuote.Columns.Add("Name", "名称");
        dgvQuote.Columns.Add("Type", "类型");
        dgvQuote.Columns.Add("CurrentPrice", "现价");
        dgvQuote.Columns.Add("ChangePercent", "涨跌幅");
        dgvQuote.Columns.Add("OpenPrice", "开盘");
        dgvQuote.Columns.Add("HighPrice", "最高");
        dgvQuote.Columns.Add("Volume", "成交量");
        dgvQuote.Columns.Add("UpdateTime", "更新时间");

        // 交易日志表格 - 最重要的展示
        dgvLog = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            BackgroundColor = Color.FromArgb(40, 40, 50),
            ForeColor = Color.Lime,
            Font = new Font("Consolas", 9F),
            RowHeadersVisible = false
        };
        dgvLog.Columns.Add("Log", "交易日志");
        dgvLog.Columns[0].Width = dgvLog.Width - 20;

        // 持仓表格
        dgvPosition = new DataGridView
        {
            Dock = DockStyle.Right,
            Width = 350,
            ReadOnly = true,
            AllowUserToAddRows = false,
            BackgroundColor = Color.White
        };
        dgvPosition.Columns.Add("Code", "代码");
        dgvPosition.Columns.Add("Name", "名称");
        dgvPosition.Columns.Add("Quantity", "数量");
        dgvPosition.Columns.Add("CostPrice", "成本");
        dgvPosition.Columns.Add("CurrentPrice", "现价");
        dgvPosition.Columns.Add("ProfitLoss", "盈亏");

        // 交易记录表格
        dgvTrade = new DataGridView
        {
            Dock = DockStyle.Bottom,
            Height = 150,
            ReadOnly = true,
            AllowUserToAddRows = false,
            BackgroundColor = Color.White
        };
        dgvTrade.Columns.Add("TradeTime", "时间");
        dgvTrade.Columns.Add("Code", "代码");
        dgvTrade.Columns.Add("Name", "名称");
        dgvTrade.Columns.Add("Direction", "方向");
        dgvTrade.Columns.Add("Price", "价格");
        dgvTrade.Columns.Add("Quantity", "数量");
        dgvTrade.Columns.Add("Amount", "金额");

        // TabControl
        var tabControl = new TabControl { Dock = DockStyle.Fill };

        var tabLog = new TabPage("📊 交易日志");
        tabLog.Controls.Add(dgvLog);

        var tabPosition = new TabPage("💼 持仓管理");
        var panelPos = new Panel { Dock = DockStyle.Fill };
        panelPos.Controls.Add(dgvPosition);
        tabPosition.Controls.Add(panelPos);

        var tabTrade = new TabPage("📜 交易记录");
        tabTrade.Controls.Add(dgvTrade);

        tabControl.TabPages.Add(tabLog);
        tabControl.TabPages.Add(tabPosition);
        tabControl.TabPages.Add(tabTrade);

        // 布局
        this.Controls.Add(tabControl);
        this.Controls.Add(panelQuery);
        this.Controls.Add(dgvQuote);
        this.Controls.Add(panelTrade);
        this.Controls.Add(panelAuto);
        this.Controls.Add(panelAccount);
    }

    private async void QuickOrder(string code, bool isBuy)
    {
        var quote = code.StartsWith("16") || code.StartsWith("15")
            ? await _marketService.GetFundQuoteAsync(code)
            : await _marketService.GetStockQuoteAsync(code);

        if (quote == null)
        {
            MessageBox.Show($"未找到{code}行情", "提示");
            return;
        }

        var action = isBuy ? "买入" : "卖出";
        var result = isBuy
            ? _tradingService.Buy(code, quote.Name, quote.Type, 100, quote.CurrentPrice)
            : _tradingService.Sell(code, 100, quote.CurrentPrice);

        _autoTrader.AddLog($"[手动{action}] {quote.Name} 100{(quote.Type == Models.SecurityType.Stock ? "股" : "份")} @ {quote.CurrentPrice:N2}");

        if (result.Success)
        {
            LoadData();
            MessageBox.Show(result.Message, "成功");
        }
        else
        {
            MessageBox.Show(result.Message, "失败");
        }
    }

    private void LoadData()
    {
        LoadAccount();
        LoadPosition();
        LoadTradeRecords();
    }

    private void LoadAccount()
    {
        var account = _tradingService.GetAccount();
        lblAccount.Text = $"💰 账户 | 初始: {account.InitialCapital:N0}元 | 可用: {account.AvailableCash:N0}元 | 持仓: {account.MarketValue:N0}元 | 总资产: {account.TotalAssets:N0}元 | 盈亏: {account.TotalProfitLoss:N0}元 ({account.ProfitLossPercent:N2}%)";
        lblAccount.ForeColor = account.TotalProfitLoss >= 0 ? Color.LimeGreen : Color.Red;
    }

    private void LoadPosition()
    {
        dgvPosition.Rows.Clear();
        var positions = _tradingService.GetPositions();

        foreach (var pos in positions)
        {
            var row = new object[]
            {
                pos.Code,
                pos.Name,
                pos.Quantity,
                pos.CostPrice.ToString("N2"),
                pos.CurrentPrice.ToString("N2"),
                pos.ProfitLoss.ToString("N2") + $" ({pos.ProfitLossPercent:N1}%)"
            };
            dgvPosition.Rows.Add(row);

            var index = dgvPosition.Rows.Count - 1;
            dgvPosition.Rows[index].Cells[5].Style.ForeColor = pos.ProfitLoss >= 0 ? Color.LimeGreen : Color.Red;
        }
    }

    private void LoadTradeRecords()
    {
        dgvTrade.Rows.Clear();
        var records = _tradingService.GetTradeRecords();

        foreach (var record in records.Take(100))
        {
            var direction = record.Direction == Models.TradeDirection.Buy ? "买入" : "卖出";
            var row = new object[]
            {
                record.TradeTime.ToString("MM-dd HH:mm"),
                record.Code,
                record.Name,
                direction,
                record.Price.ToString("N2"),
                record.Quantity,
                record.Amount.ToString("N0")
            };
            dgvTrade.Rows.Add(row);

            var index = dgvTrade.Rows.Count - 1;
            dgvTrade.Rows[index].Cells[3].Style.ForeColor = record.Direction == Models.TradeDirection.Buy ? Color.Red : Color.LimeGreen;
        }
    }

    private async Task QueryQuoteAsync(string code)
    {
        var quote = code.StartsWith("16") || code.StartsWith("15") || code.StartsWith("00")
            ? await _marketService.GetFundQuoteAsync(code)
            : await _marketService.GetStockQuoteAsync(code);

        if (quote != null)
        {
            dgvQuote.Rows.Clear();
            var type = quote.Type == Models.SecurityType.Stock ? "股票" : "基金";
            var row = new object[]
            {
                quote.Code,
                quote.Name,
                type,
                quote.CurrentPrice.ToString("N2"),
                quote.ChangePercent.ToString("N2") + "%",
                quote.OpenPrice.ToString("N2"),
                quote.HighPrice.ToString("N2"),
                quote.Volume.ToString("N0"),
                quote.UpdateTime.ToString("HH:mm:ss")
            };
            dgvQuote.Rows.Add(row);

            txtPrice.Text = quote.CurrentPrice.ToString("N2");
            dgvQuote.Rows[0].Cells[4].Style.ForeColor = quote.ChangePercent >= 0 ? Color.Red : Color.LimeGreen;
        }
    }

    private async void BtnTrade_Click(object? sender, EventArgs e)
    {
        var code = txtCode.Text.Trim();
        var qtyStr = txtQuantity.Text.Trim();
        var priceStr = txtPrice.Text.Trim();

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(qtyStr) || string.IsNullOrEmpty(priceStr))
        {
            MessageBox.Show("请填写完整信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(qtyStr, out var quantity) || quantity <= 0)
        {
            MessageBox.Show("数量格式不正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(priceStr, out var price) || price <= 0)
        {
            MessageBox.Show("价格格式不正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var quote = code.StartsWith("16") || code.StartsWith("15")
            ? await _marketService.GetFundQuoteAsync(code)
            : await _marketService.GetStockQuoteAsync(code);

        if (quote == null)
        {
            MessageBox.Show("未找到该证券", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var isBuy = cmbDirection.SelectedIndex == 0;
        var result = isBuy
            ? _tradingService.Buy(code, quote.Name, quote.Type, quantity, price)
            : _tradingService.Sell(code, quantity, price);

        var action = isBuy ? "买入" : "卖出";
        _autoTrader.AddLog($"[手动{action}] {quote.Name} {quantity}{(quote.Type == Models.SecurityType.Stock ? "股" : "份")} @ {price:N2}");

        MessageBox.Show(result.Message, result.Success ? "成功" : "失败",
            MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

        if (result.Success)
        {
            LoadData();
            txtQuantity.Clear();
            txtCode.Clear();
            txtPrice.Clear();
        }
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("确定要重置账户吗？这将清空所有持仓和交易记录！", "确认",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            _tradingService.ResetAccount();
            LoadData();
            _autoTrader.AddLog("账户已重置");
            MessageBox.Show("账户已重置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void StartTimer()
    {
        timer = new System.Windows.Forms.Timer { Interval = 30000 };
        timer.Tick += (s, e) =>
        {
            try
            {
                LoadAccount();
            }
            catch { }
        };
        timer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        timer?.Stop();
        _autoTrader.Stop();
        base.OnFormClosing(e);
    }
}
