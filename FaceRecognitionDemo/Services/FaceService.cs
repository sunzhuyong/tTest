using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Data.Sqlite;    // SQLite 数据库 - 类似于 Java 的 SQLite JDBC
using OpenCvSharp;             // OpenCV - 图像处理
using FaceRecognitionDemo.Models;
using ClosedXML.Excel;         // Excel 导出库 - 类似于 Java 的 Apache POI

namespace FaceRecognitionDemo.Services
{
    /// <summary>
    /// 人脸服务类 - 业务层 (Service Layer)
    /// 负责数据持久化和文件操作
    ///
    /// 类似于 Java 的:
    /// - Service 类 (@Service)
    /// - DAO 类 (@Repository)
    /// - 使用 JDBC 操作数据库
    /// </summary>
    public class FaceService
    {
        // ==================== 成员变量 ====================

        /// <summary>
        /// 数据库文件路径
        /// 类似于 Java 中配置的数据源路径
        /// </summary>
        private readonly string _dbPath;

        /// <summary>
        /// 人脸图片存储文件夹
        /// 类似于 Java 中配置的上传目录路径
        /// </summary>
        private readonly string _facesFolder;

        // ==================== 构造函数 ====================

        /// <summary>
        /// 构造函数 - 初始化数据库和存储目录
        /// 类似于 Java 的 @PostConstruct 或初始化块
        /// </summary>
        public FaceService()
        {
            // 获取程序运行目录 - 类似于 Java: System.getProperty("user.dir")
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 创建 Data 文件夹存储数据库
            // 类似于 Java: new File(baseDir + "/Data").mkdirs()
            string dataFolder = Path.Combine(baseDir, "Data");
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            // 数据库文件路径 - SQLite 是一个文件数据库
            // 类似于 Java: "jdbc:sqlite:" + baseDir + "/Data/faces.db"
            _dbPath = Path.Combine(dataFolder, "faces.db");

            // 创建 Faces 文件夹存储人脸图片
            _facesFolder = Path.Combine(baseDir, "Faces");
            if (!Directory.Exists(_facesFolder))
            {
                Directory.CreateDirectory(_facesFolder);
            }

            // 初始化数据库表
            InitializeDatabase();
        }

        // ==================== 数据库初始化 ====================

        /// <summary>
        /// 初始化数据库 - 创建表结构
        ///
        /// SQLite 使用 SQL 语句操作数据库
        /// 类似于 Java 使用 JDBC 执行 CREATE TABLE
        /// </summary>
        private void InitializeDatabase()
        {
            // 创建数据库连接
            // 类似于 Java:
            // Connection conn = DriverManager.getConnection("jdbc:sqlite:" + dbPath);
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();  // 打开连接

            // SQL 建表语句
            // 类似于 Java:
            // String sql = "CREATE TABLE IF NOT EXISTS Faces (...)";
            // Statement stmt = conn.createStatement();
            // stmt.execute(sql);
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Faces (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,  -- 自增主键
                    Name TEXT NOT NULL,                    -- 姓名
                    FaceImagePath TEXT NOT NULL,          -- 图片路径
                    RegisterTime TEXT NOT NULL            -- 注册时间
                )";

            // 执行 SQL
            using var command = new SqliteCommand(createTableSql, connection);
            command.ExecuteNonQuery();  // 执行建表语句
        }

        // ==================== OpenCV: 保存人脸图片 ====================

        /// <summary>
        /// 保存人脸图片到本地
        ///
        /// 使用 OpenCV 的 ImWrite 方法保存图像
        /// 对比 Java:
        /// - C#: Cv2.ImWrite(filePath, mat)
        /// - Java: High.imwrite(filePath, mat)
        /// </summary>
        /// <param name="faceImage">人脸图像 (Mat)</param>
        /// <param name="name">姓名</param>
        /// <returns>保存的文件路径</returns>
        public string SaveFaceImage(Mat faceImage, string name)
        {
            // 生成文件名: 姓名_时间戳.jpg
            string fileName = $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = Path.Combine(_facesFolder, fileName);

            // 使用 OpenCV 保存图像
            // 类似于 Java: ImageIO.write(bufferedImage, "jpg", new File(filePath))
            // 或者 OpenCV: High.imwrite(filePath, faceImage)
            Cv2.ImWrite(filePath, faceImage);

            return filePath;
        }

        // ==================== 数据库操作: 添加 ====================

        /// <summary>
        /// 添加人脸记录到数据库
        ///
        /// 使用参数化查询防止 SQL 注入
        /// 类似于 Java 使用 PreparedStatement
        /// </summary>
        /// <param name="face">人脸记录对象</param>
        public void AddFace(FaceRecord face)
        {
            // 创建连接 - 类似于 Java: DriverManager.getConnection()
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            // 参数化 SQL 语句
            // 使用 @Name @FaceImagePath @RegisterTime 作为参数占位符
            // 类似于 Java:
            // String sql = "INSERT INTO Faces VALUES (?, ?, ?)";
            // PreparedStatement pstmt = conn.prepareStatement(sql);
            // pstmt.setString(1, face.getName());
            string insertSql = @"
                INSERT INTO Faces (Name, FaceImagePath, RegisterTime)
                VALUES (@Name, @FaceImagePath, @RegisterTime)";

            // 创建命令
            using var command = new SqliteCommand(insertSql, connection);

            // 绑定参数 - 类似于 Java: pstmt.setString()
            command.Parameters.AddWithValue("@Name", face.Name);
            command.Parameters.AddWithValue("@FaceImagePath", face.FaceImagePath);
            command.Parameters.AddWithValue("@RegisterTime", face.RegisterTime.ToString("yyyy-MM-dd HH:mm:ss"));

            // 执行插入 - 类似于 Java: pstmt.executeUpdate()
            command.ExecuteNonQuery();
        }

        // ==================== 数据库操作: 查询 ====================

        /// <summary>
        /// 查询所有已注册的人脸
        ///
        /// 类似于 Java:
        /// - 使用 SELECT 语句
        /// - ResultSet 遍历结果
        /// - 封装到 List<FaceRecord>
        /// </summary>
        /// <returns>人脸记录列表</returns>
        public List<FaceRecord> GetAllFaces()
        {
            var faces = new List<FaceRecord>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            // SQL 查询 - 按注册时间倒序
            string selectSql = "SELECT Id, Name, FaceImagePath, RegisterTime FROM Faces ORDER BY RegisterTime DESC";

            using var command = new SqliteCommand(selectSql, connection);

            // 执行查询 - 类似于 Java: ResultSet rs = stmt.executeQuery()
            using var reader = command.ExecuteReader();

            // 遍历结果集
            // 类似于 Java: while (rs.next())
            while (reader.Read())
            {
                // 读取各列数据 - 类似于 Java: rs.getInt(), rs.getString()
                // 注意: 列索引从 0 开始
                faces.Add(new FaceRecord
                {
                    Id = reader.GetInt32(0),           // 获取第一列 (Id)
                    Name = reader.GetString(1),       // 获取第二列 (Name)
                    FaceImagePath = reader.GetString(2), // 获取第三列
                    RegisterTime = DateTime.Parse(reader.GetString(3)) // 获取第四列
                });
            }

            return faces;
        }

        // ==================== 数据库操作: 删除 ====================

        /// <summary>
        /// 根据 ID 删除人脸记录
        ///
        /// 类似于 Java:
        /// - DELETE FROM Faces WHERE Id = ?
        /// - PreparedStatement.setInt(1, id)
        /// </summary>
        /// <param name="id">要删除的记录 ID</param>
        public void DeleteFace(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string deleteSql = "DELETE FROM Faces WHERE Id = @Id";

            using var command = new SqliteCommand(deleteSql, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        // ==================== Excel 导出 ====================

        /// <summary>
        /// 导出数据到 Excel 文件
        ///
        /// 使用 ClosedXML 库 (类似于 Java 的 Apache POI)
        /// - ClosedXML: 操作 .xlsx 格式
        /// - 类似于 Java: Workbook workbook = new XSSFWorkbook()
        ///
        /// 对比 Apache POI (Java):
        /// - C#: workbook.Worksheets.Add() -> worksheet.Cell(row, col).Value = ...
        /// - Java: workbook.createSheet() -> row.createCell(col).setCellValue(...)
        /// </summary>
        /// <param name="faces">人脸记录列表</param>
        /// <param name="filePath">保存路径</param>
        public void ExportToExcel(List<FaceRecord> faces, string filePath)
        {
            // 创建工作簿 - 类似于 Java: new XSSFWorkbook()
            using var workbook = new XLWorkbook();

            // 创建工作表 - 类似于 Java: workbook.createSheet("人脸数据")
            var worksheet = workbook.Worksheets.Add("人脸数据");

            // ==================== 设置表头样式 ====================
            //
            // 设置表头:
            // - 字体加粗
            // - 背景蓝色 (#2196F3)
            // - 字体白色
            // - 居中对齐
            //
            // 类似于 Java:
            // CellStyle style = workbook.createCellStyle();
            // style.setFillForegroundColor(IndexedColors.BLUE.getIndex());
            // style.setFillPattern(FillPatternType.SOLID_FOREGROUND);
            // style.setAlignment(HorizontalAlignment.CENTER);
            //
            var headerRange = worksheet.Range("A1:D1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2196F3");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ==================== 写入表头 ====================
            //
            // 设置单元格值
            // 类似于 Java: row.createCell(0).setCellValue("序号")
            //
            worksheet.Cell(1, 1).Value = "序号";
            worksheet.Cell(1, 2).Value = "姓名";
            worksheet.Cell(1, 3).Value = "注册时间";
            worksheet.Cell(1, 4).Value = "照片路径";

            // 设置列宽 - 类似于 Java: sheet.setColumnWidth()
            worksheet.Column(1).Width = 10;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 22;
            worksheet.Column(4).Width = 50;

            // ==================== 写入数据行 ====================
            //
            // 从第 2 行开始写入数据 (第 1 行是表头)
            // 类似于 Java:
            // for (FaceRecord face : faces) {
            //     Row row = sheet.createRow(rowNum++);
            //     row.createCell(0).setCellValue(face.getId());
            //     ...
            // }
            //
            int row = 2;  // 从第 2 行开始
            foreach (var face in faces)
            {
                worksheet.Cell(row, 1).Value = face.Id;
                worksheet.Cell(row, 2).Value = face.Name;
                worksheet.Cell(row, 3).Value = face.RegisterTime.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 4).Value = face.FaceImagePath;

                row++;
            }

            // ==================== 保存文件 ====================
            //
            // 保存到指定路径
            // 类似于 Java: FileOutputStream fos = new FileOutputStream(filePath);
            //             workbook.write(fos);
            //             fos.close();
            //
            workbook.SaveAs(filePath);
        }
    }
}
