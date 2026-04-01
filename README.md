# tTest 项目集合

本解决方案包含多个 C# 项目，用于学习和演示目的。

## 项目列表

| 项目名称 | 路径 | 描述 |
|---------|------|------|
| FaceRecognitionDemo | `FaceRecognitionDemo/` | 人脸识别演示程序 |
| CSharpLearningDemo | `CSharpLearningDemo/CSharpLearningDemo/` | C# 学习示例 |
| TradingSimulator | `TradingSimulator/` | A股模拟交易平台 |
| VISIONAssembly_Scripts | `VISIONAssembly_Scripts/` | 凌云光视觉脚本 |

## 快速启动

### 解决方案整体构建

```bash
# 在 d:\tTest 目录下
dotnet build tTest.sln
```

### FaceRecognitionDemo

人脸识别演示程序，基于 OpenCvSharp4 实现人脸检测和识别功能。

```bash
# 方式1: 使用已编译的 exe
d:\tTest\FaceRecognitionDemo\bin\Debug\net10.0-windows\FaceRecognitionDemo.exe

# 方式2: 命令行运行
dotnet run --project FaceRecognitionDemo\FaceRecognitionDemo.csproj

# 方式3: 在 Visual Studio 中打开解决方案运行
```

**功能特性：**
- 人脸检测 (使用 Haar Cascade)
- 人脸特征提取与识别
- 人脸数据存储 (SQLite)
- 导出 Excel 报表 (ClosedXML)

**依赖项：**
- OpenCvSharp4 4.9.0
- Microsoft.Data.Sqlite 8.0.0
- ClosedXML 0.102.2

---

### TradingSimulator

A股模拟交易平台，支持股票和基金的模拟交易。

```bash
# 方式1: 使用已编译的 exe
d:\tTest\TradingSimulator\bin\Debug\net10.0-windows\TradingSimulator.exe

# 方式2: 命令行运行
dotnet run --project TradingSimulator\TradingSimulator.csproj
```

**功能特性：**
- 实时行情查询（股票/基金）
- 模拟买入/卖出交易
- 持仓管理
- 交易记录查询
- 账户盈亏分析
- 账户重置功能

**数据来源：**
- 新浪财经API（免费，延迟15秒）

**初始资金：** 10万元（可重置）

**依赖项：**
- Microsoft.Data.Sqlite 8.0.0
- Newtonsoft.Json 13.0.3

---

### CSharpLearningDemo

C# 面向对象编程学习示例，展示类、接口、继承等概念。

```bash
# 方式1: 使用已编译的 exe
d:\tTest\CSharpLearningDemo\CSharpLearningDemo\bin\Debug\net10.0-windows\CSharpLearningDemo.exe

# 方式2: 命令行运行
dotnet run --project CSharpLearningDemo\CSharpLearningDemo\CSharpLearningDemo.csproj
```

**学习内容：**
- 类的定义与继承
- 接口的使用
- 多态性
- 面向对象设计模式

---

### VISIONAssembly_Scripts

凌云光 VISIONAssembly 视觉软件的 Python 脚本集。

**使用方法：**
1. 打开 VISIONAssembly 软件
2. 进入"脚本工具"
3. 导入或运行对应的 Python 脚本

**脚本说明：**
- `显示文字颜色.py` - 演示 GUI 文字颜色显示

---

## 项目结构

```
tTest/
├── tTest.sln                     # 解决方案文件
├── SPEC.md                       # 项目规格说明
├── FaceRecognitionDemo/          # 人脸识别项目
│   ├── FaceRecognitionDemo.csproj
│   ├── Models/                   # 数据模型
│   │   └── FaceRecord.cs
│   ├── Services/                 # 业务服务
│   │   └── FaceService.cs
│   ├── MainForm.cs               # 主窗体
│   ├── InputNameForm.cs           # 输入姓名窗体
│   ├── Program.cs                 # 程序入口
│   └── haarcascade_frontalface_default.xml  # 人脸检测模型
├── CSharpLearningDemo/           # C# 学习项目
│   └── CSharpLearningDemo/
│       ├── CSharpLearningDemo.csproj
│       ├── Models/                # 示例模型类
│       ├── MainForm.cs
│       └── Program.cs
├── TradingSimulator/             # A股模拟交易平台
│   ├── TradingSimulator.csproj
│   ├── Models/                    # 数据模型
│   │   ├── Security.cs
│   │   ├── Position.cs
│   │   ├── TradeRecord.cs
│   │   └── Account.cs
│   ├── Services/                  # 业务服务
│   │   ├── MarketDataService.cs  # 行情服务
│   │   ├── DatabaseService.cs    # 数据库服务
│   │   └── TradingService.cs     # 交易服务
│   ├── Forms/
│   │   └── MainForm.cs           # 主窗体
│   └── Program.cs
└── VISIONAssembly_Scripts/       # 视觉脚本
    └── 显示文字颜色.py
```

---

## 环境要求

- .NET 10.0 SDK (或更高版本)
- Windows 10/11
- Visual Studio 2022 (推荐)

## 编译注意事项

1. **FaceRecognitionDemo** 需要额外下载 OpenCV 本地库，首次运行会自动下载
2. 确保 `haarcascade_frontalface_default.xml` 文件在输出目录中

---

## 常见问题

### Q: 编译报错 "找不到 OpenCvSharp"
A: 确保 NuGet 包已正确还原，运行 `dotnet restore`

### Q: 人脸检测不工作
A: 检查 `haarcascade_frontalface_default.xml` 是否在运行目录

### Q: 如何添加新项目到解决方案
A: 使用 `dotnet sln add 项目路径/项目.csproj` 命令

---

## 相关文档

- [FaceRecognitionDemo 详细说明](FaceRecognitionDemo/README.md)
- [VisionPro 脚本封装说明](C:\Users\Administrator\Documents\xwechat_files\SZY5324HI_c8e6\msg\file\2026-03\脚本封装说明V1.3.pdf)
- [VISIONAssembly 帮助文档](C:\Program Files\VISIONAssembly_x64\Doc\index.html)
