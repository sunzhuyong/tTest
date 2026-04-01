# 人脸识别系统 Demo

基于 C# WinForms + OpenCV 的人脸识别演示程序。

## 功能特性

- 实时摄像头预览
- 人脸检测（绿色矩形框标识）
- 人脸数据注册
- 人脸数据导出到 Excel（用户可选择保存路径）
- SQLite 本地数据库存储

## 环境要求

- Windows 10/11
- .NET 6.0 SDK 或更高版本
- 摄像头设备

## 安装与运行

### 1. 编译项目

```bash
cd FaceRecognitionDemo
dotnet restore
dotnet build
```

### 2. 运行程序

```bash
dotnet run
```

或者直接运行编译后的可执行文件：
```
bin/Debug/net6.0-windows/FaceRecognitionDemo.exe
```

## 使用说明

1. **启动程序**：程序会自动打开默认摄像头
2. **注册人脸**：
   - 点击右侧面板的"注册新人脸"按钮
   - 确保脸部对准摄像头
   - 在弹出的输入框中输入姓名
   - 点击确定完成注册
3. **查看列表**：右侧面板显示所有已注册的人脸
4. **删除人脸**：选中列表中的人脸，点击"删除选中"
5. **导出数据**：
   - 点击"导出Excel"按钮
   - 在弹出的保存对话框中选择保存路径
   - 点击保存即可导出为 Excel 文件

## 项目结构

```
FaceRecognitionDemo/
├── Models/
│   └── FaceRecord.cs          # 人脸数据模型
├── Services/
│   └── FaceService.cs         # 人脸服务和Excel导出
├── Data/                       # SQLite数据库目录
├── Faces/                      # 人脸图片存储目录
├── MainForm.cs                # 主窗体
├── MainForm.Designer.cs       # 主窗体设计
├── InputNameForm.cs           # 输入姓名窗体
├── Program.cs                 # 程序入口
├── FaceRecognitionDemo.csproj # 项目文件
└── haarcascade_frontalface_default.xml  # 人脸检测分类器
```

## 依赖包

- OpenCvSharp4 (4.9.0) - OpenCV C# 绑定
- Microsoft.Data.Sqlite (8.0.0) - SQLite 数据库
- ClosedXML (0.102.2) - Excel 导出

## 注意事项

1. 首次运行需要联网下载 NuGet 包
2. 确保摄像头权限已开启
3. 建议在光线充足的环境下使用人脸识别
4. 导出的 Excel 文件包含：序号、姓名、注册时间、照片路径

## 许可证

MIT License
