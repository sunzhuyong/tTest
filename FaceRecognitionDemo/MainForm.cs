using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenCvSharp;              // OpenCV 核心库 - 图像处理、计算机视觉
using OpenCvSharp.Extensions;    // OpenCV 与 WinForms 的桥接库 - Mat转Bitmap
using FaceRecognitionDemo.Services;   // 业务服务层
using FaceRecognitionDemo.Models;     // 数据模型

namespace FaceRecognitionDemo
{
    /// <summary>
    /// 主窗体 - WinForms 应用程序入口
    /// 类似于 Java Swing 中的 JFrame
    /// </summary>
    public partial class MainForm : Form
    {
        // ==================== 成员变量 (类似 Java 的成员字段) ====================

        /// <summary>
        /// 视频捕获对象 - 相当于 OpenCV 的 VideoCapture
        /// 用于从摄像头读取视频帧，类似于 Java 中的 VideoCapture 或 FFmpeg 的捕获设备
        /// </summary>
        private VideoCapture? _capture;

        /// <summary>
        /// 捕获线程 - WinForms 默认是单线程UI，需要用独立线程读取摄像头
        /// 类似于 Java 中启动的 Thread 或 ExecutorService
        /// </summary>
        private Thread? _captureThread;

        /// <summary>
        /// 运行标志 - 控制线程停止，类似于 Java 中的 volatile boolean
        /// </summary>
        private bool _isRunning = false;

        /// <summary>
        /// Haar级联分类器 - 用于人脸检测
        /// 类似于 Java 中的 CascadeClassifier
        /// Haar Cascade 是一种经典的机器学习算法，用于快速检测目标
        /// </summary>
        private CascadeClassifier? _faceCascade;

        /// <summary>
        /// 人脸服务 - 业务层，类似于 Java 中的 Service/DAO
        /// 负责数据库操作和文件处理
        /// </summary>
        private readonly FaceService _faceService;

        /// <summary>
        /// 已注册的人脸列表 - 内存缓存
        /// 类似于 Java 中的 List<FaceRecord>
        /// </summary>
        private readonly List<FaceRecord> _registeredFaces = new();

        /// <summary>
        /// 当前视频帧 - 用于注册时获取最新帧
        /// Mat 是 OpenCV 中的核心图像容器，类似于 Java 的 Mat 或 BufferedImage
        /// </summary>
        private Mat? _currentFrame;

        /// <summary>
        /// 是否正在捕获标志
        /// </summary>
        private bool _isCapturing = false;

        // ==================== 构造函数 ====================

        /// <summary>
        /// 构造函数 - 初始化窗体
        /// 类似于 Java Swing 中的构造函数或 @PostConstruct
        /// InitializeComponent() 会加载 Designer.cs 中定义的 UI 组件
        /// </summary>
        public MainForm()
        {
            InitializeComponent();  // 初始化 UI 组件 (由 Designer.cs 自动生成)

            // 初始化业务服务 - 类似于 Spring 的依赖注入
            _faceService = new FaceService();

            // ==================== OpenCV: 加载 Haar 级联分类器 ====================
            //
            // Haar Cascade 原理:
            // - 使用 Haar 特征 (Haar-like features) 检测图像中的模式
            // - 通过"级联"多个弱分类器形成强分类器
            // - 预训练的 XML 文件包含了人脸的特征数据
            //
            // 类似于 Java 中:
            // - 使用 OpenCV 的 CascadeClassifier
            // - 或者使用 ML 库加载训练好的模型 (如 TensorFlow, PyTorch)
            //
            string cascadePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");

            if (File.Exists(cascadePath))
            {
                // 创建分类器 - 传入训练好的 XML 文件路径
                // 类似于 Java: new CascadeClassifier(cascadePath)
                _faceCascade = new CascadeClassifier(cascadePath);
            }
            else
            {
                // 备用路径 - 从 NuGet 包中查找
                string nugetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", "win-x64", "native", "haarcascade_frontalface_default.xml");
                if (File.Exists(nugetPath))
                {
                    _faceCascade = new CascadeClassifier(nugetPath);
                }
                else
                {
                    MessageBox.Show("未找到人脸检测分类器文件!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 加载已注册的人脸列表 - 类似于 Java 中的 @PostConstruct 或 initial() 方法
            LoadRegisteredFaces();
        }

        // ==================== 窗体生命周期事件 ====================

        /// <summary>
        /// 窗体加载事件 - 类似于 Java Swing 的 windowOpened 或 JavaFX 的 initialize
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // 初始化摄像头 - 程序启动时自动打开
            InitializeCamera();
        }

        // ==================== OpenCV: 摄像头初始化 ====================

        /// <summary>
        /// 初始化摄像头设备
        ///
        /// WinForms + OpenCV 的摄像头工作流程:
        /// 1. 创建 VideoCapture 对象，传入设备索引 (0 = 默认摄像头)
        /// 2. 设置分辨率和帧率
        /// 3. 在独立线程中循环读取帧 (Read 方法是非阻塞的)
        /// 4. 将 OpenCV 的 Mat 转换为 WinForms 的 Bitmap 显示
        ///
        /// 对比 Java:
        /// - JavaCV/FFmpeg: VideoCapture capture = new VideoCapture(0)
        /// - OpenCV Java: VideoCapture capture = new VideoCapture(CAP_DSHOW + 0)
        /// - 处理流程类似
        /// </summary>
        private void InitializeCamera()
        {
            // 创建视频捕获对象，参数 0 表示默认摄像头
            // 类似于 Java: VideoCapture capture = new VideoCapture(0);
            _capture = new VideoCapture(0);

            // 检查摄像头是否打开成功
            // 类似于 Java: if (!capture.isOpened())
            if (!_capture.IsOpened())
            {
                MessageBox.Show("无法打开摄像头!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 设置摄像头参数 (通过 OpenCV 的属性常量)
            // 类似于 Java: capture.set(CAP_PROP_FRAME_WIDTH, 640);
            _capture.Set(VideoCaptureProperties.FrameWidth, 640);   // 设置宽度
            _capture.Set(VideoCaptureProperties.FrameHeight, 480);  // 设置高度
            _capture.Set(VideoCaptureProperties.Fps, 30);          // 设置帧率

            // 启动捕获线程 - WinForms UI 是单线程的，不能在主线程阻塞读取视频
            // 类似于 Java 中启动新线程处理视频流
            _isRunning = true;
            _captureThread = new Thread(CaptureLoop);  // 创建线程
            _captureThread.IsBackground = true;         // 设置为后台线程 (程序退出时自动结束)
            _captureThread.Start();                     // 启动线程

            UpdateStatus("摄像头已启动");
        }

        // ==================== OpenCV: 视频帧捕获循环 ====================

        /// <summary>
        /// 视频捕获循环 - 在独立线程中运行
        ///
        /// 核心流程:
        /// 1. capture.Read(frame) - 读取下一帧
        /// 2. 检测人脸 (调用 DetectFaces)
        /// 3. 将 Mat 转换为 Bitmap (OpenCvSharp.Extensions)
        /// 4. 显示在 WinForms 的 PictureBox 上
        ///
        /// 对比 Java:
        /// - Java 中常用 JavaCV/FFmpeg 的 FrameGrabber
        /// - 或者使用 OpenCV Java 的 VideoCapture.read()
        /// - BufferedImage 转 Swing 组件
        /// </summary>
        private void CaptureLoop()
        {
            // 循环运行，直到 _isRunning 变为 false
            while (_isRunning && _capture != null && _capture.IsOpened())
            {
                try
                {
                    // 创建 Mat 容器接收帧
                    // Mat 是 OpenCV 的核心图像容器，类似 Java: new Mat()
                    using var frame = new Mat();

                    // 读取下一帧 - 返回 true 表示成功读取
                    // 类似于 Java: if (capture.read(frame))
                    if (_capture.Read(frame) && !frame.Empty())
                    {
                        // 保存当前帧的克隆，供注册人脸时使用
                        // Clone() 会复制数据，类似 Java: frame.clone()
                        _currentFrame = frame.Clone();

                        // 检测人脸并绘制框
                        if (_faceCascade != null)
                        {
                            DetectFaces(frame);
                        }

                        // ==================== OpenCV -> WinForms 显示 ====================
                        //
                        // OpenCV 使用 BGR 格式存储图像 (Mat)
                        // WinForms 的 PictureBox 需要 Bitmap
                        // 使用 OpenCvSharp.Extensions 的转换器
                        //
                        // 对比 Java:
                        // - Java: BufferedImage = MatToBufferedImage(frame)
                        // - Swing: new ImageIcon(bufferedImage)
                        //
                        var bitmap = BitmapConverter.ToBitmap(frame);  // Mat -> Bitmap 转换

                        // 显示在 PictureBox 上
                        var oldImage = pictureBoxVideo.Image;  // 保存旧图片
                        pictureBoxVideo.Image = bitmap;         // 设置新图片
                        oldImage?.Dispose();                    // 释放旧图片内存
                    }

                    // 控制帧率 ~30fps
                    // 1000ms / 30 ≈ 33ms
                    // 类似于 Java: Thread.sleep(33)
                    Thread.Sleep(33);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Capture error: {ex.Message}");
                }
            }
        }

        // ==================== OpenCV: 人脸检测 ====================

        /// <summary>
        /// 人脸检测核心方法
        ///
        /// Haar Cascade 检测原理:
        /// 1. 将图像转换为灰度图 (人脸检测只需要亮度信息)
        /// 2. 直方图均衡化 (增强对比度，使检测更准确)
        /// 3. 使用 detectMultiScale 检测不同大小的目标
        /// 4. 返回检测到的所有人脸区域 (Rect 数组)
        ///
        /// 参数说明:
        /// - scaleFactor: 图像缩放因子，1.1 表示每次缩小 10%
        /// - minNeighbors: 邻居数量，越大检测越严格
        /// - minSize: 最小人脸尺寸
        ///
        /// 对比 Java:
        /// - MatOfRect faces = cascade.detectMultiScale(gray);
        /// - for (Rect face : faces.toArray()) { ... }
        /// </summary>
        private void DetectFaces(Mat frame)
        {
            if (_faceCascade == null) return;

            // 1. 创建灰度图 - 减少计算量，人脸检测只用亮度
            // 类似于 Java: Mat gray = new Mat();
            //             Imgproc.cvtColor(frame, gray, Imgproc.COLOR_BGR2GRAY);
            using var gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

            // 2. 直方图均衡化 - 增强图像对比度
            // 类似于 Java: Imgproc.equalizeHist(gray, gray);
            Cv2.EqualizeHist(gray, gray);

            // 3. 检测人脸 - detectMultiScale 是核心方法
            // 返回 Rect 数组，每个 Rect 表示一个人脸区域 (x, y, width, height)
            //
            // 类似于 Java:
            // MatOfRect faces = new MatOfRect();
            // cascade.detectMultiScale(gray, faces, 1.1, 5, 0, new Size(60, 60), new Size());
            var faces = _faceCascade.DetectMultiScale(
                gray,                           // 输入灰度图
                scaleFactor: 1.1,               // 缩放因子
                minNeighbors: 5,                // 最小邻居数
                flags: HaarDetectionTypes.ScaleImage,  // 检测模式
                minSize: new OpenCvSharp.Size(60, 60) // 最小人脸尺寸
            );

            // 4. 遍历检测到的人脸，绘制矩形框
            // 类似于 Java: for (Rect face : faces.toArray())
            foreach (var face in faces)
            {
                // 绘制绿色矩形框
                // 类似于 Java: Imgproc.rectangle(frame, face, new Scalar(0, 255, 0), 2);
                Cv2.Rectangle(frame, face, new Scalar(0, 255, 0), 2);

                // 如果正在捕获，显示提示文字
                if (_isCapturing && face.Width > 60)
                {
                    // 在人脸上方绘制文字
                    // 类似于 Java: Imgproc.putText(frame, "Capturing...", ...)
                    Cv2.PutText(frame, "Capturing...",
                        new OpenCvSharp.Point(face.X, face.Y - 10),  // 文字位置
                        HersheyFonts.HersheySimplex,                 // 字体
                        0.8,                                         // 字体大小
                        new Scalar(0, 255, 255),                    // 颜色 (黄色)
                        2);                                          // 线宽
                }
            }
        }

        // ==================== 数据加载 ====================

        /// <summary>
        /// 从数据库加载已注册的人脸列表
        /// 类似于 Java 的 Service 层方法调用
        /// </summary>
        private void LoadRegisteredFaces()
        {
            _registeredFaces.Clear();

            // 调用服务层获取数据 - 类似 Java: faceService.getAllFaces()
            var faces = _faceService.GetAllFaces();
            _registeredFaces.AddRange(faces);

            // 更新 WinForms ListView - 类似于 Java Swing 的 JTable
            listViewFaces.Items.Clear();
            foreach (var face in faces)
            {
                var item = new ListViewItem(face.Id.ToString());
                item.SubItems.Add(face.Name);
                item.SubItems.Add(face.RegisterTime.ToString("yyyy-MM-dd HH:mm:ss"));
                item.Tag = face;  // 存储对象引用，类似 Java 的 setClientProperty
                listViewFaces.Items.Add(item);
            }

            lblTotalFaces.Text = $"已注册人脸: {_registeredFaces.Count}";
        }

        // ==================== 人脸注册 ====================

        /// <summary>
        /// 注册新人脸按钮点击事件
        ///
        /// 流程:
        /// 1. 检测当前帧中的人脸
        /// 2. 弹出输入框获取姓名
        /// 3. 裁剪人脸区域并缩放
        /// 4. 保存图片到本地
        /// 5. 保存记录到数据库
        /// </summary>
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (_currentFrame == null || _faceCascade == null)
            {
                MessageBox.Show("请先确保摄像头已启动!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 检测当前帧中的人脸 (复用 DetectFaces 的逻辑)
            using var gray = new Mat();
            Cv2.CvtColor(_currentFrame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);

            var faces = _faceCascade.DetectMultiScale(
                gray,
                scaleFactor: 1.1,
                minNeighbors: 5,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new OpenCvSharp.Size(60, 60)
            );

            if (faces.Length == 0)
            {
                MessageBox.Show("未检测到人脸，请确保脸部对准摄像头!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (faces.Length > 1)
            {
                MessageBox.Show("检测到多个人脸，请确保只有一个脸部对准摄像头!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 弹出输入框获取姓名 - WinForms 模态对话框
            // 类似于 Java: JOptionPane.showInputDialog() 或自定义 JDialog
            using var inputForm = new InputNameForm();
            if (inputForm.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(inputForm.PersonName))
            {
                return;
            }

            string name = inputForm.PersonName.Trim();

            // ==================== OpenCV: 裁剪和缩放人脸 ====================
            //
            // 1. 从原图中裁剪人脸区域 (使用 Rect)
            // 2. 缩放到统一大小 (100x100)，便于后续处理和存储
            //
            // 对比 Java:
            // - 裁剪: Mat faceImage = new Mat(frame, face);
            // - 缩放: Imgproc.resize(faceImage, resizedFace, new Size(100, 100));
            //

            // 获取人脸区域
            var faceRect = faces[0];

            // 裁剪人脸 - 使用 Mat 构造函数指定 ROI (Region of Interest)
            // 类似于 Java: Mat faceImage = new Mat(frame, new Rect(x, y, w, h));
            using var faceImage = new Mat(_currentFrame, faceRect);

            // 创建目标 Mat 用于存储缩放后的图像
            using var resizedFace = new Mat();

            // 缩放图像 - 类似于 Java: Imgproc.resize(src, dst, new Size(100, 100))
            Cv2.Resize(faceImage, resizedFace, new OpenCvSharp.Size(100, 100));

            // 保存人脸图像到文件
            string facePath = _faceService.SaveFaceImage(resizedFace, name);

            // 创建人脸记录对象 - 类似于 Java: new FaceRecord()
            var faceRecord = new FaceRecord
            {
                Name = name,
                FaceImagePath = facePath,
                RegisterTime = DateTime.Now
            };

            // 保存到数据库
            _faceService.AddFace(faceRecord);

            // 刷新界面列表
            LoadRegisteredFaces();

            MessageBox.Show($"人脸 \"{name}\" 注册成功!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateStatus($"已注册人脸: {name}");
        }

        // ==================== Excel 导出 ====================

        /// <summary>
        /// 导出 Excel 按钮点击事件
        /// 使用 SaveFileDialog 让用户选择保存路径
        ///
        /// WinForms 对比 Java Swing:
        /// - WinForms: SaveFileDialog (内置)
        /// - Java: JFileChooser (Swing) 或 FileDialog (AWT)
        /// </summary>
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (_registeredFaces.Count == 0)
            {
                MessageBox.Show("没有可导出的人脸数据!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 创建保存文件对话框 - 类似于 Java 的 JFileChooser
            // WinForms: System.Windows.Forms.SaveFileDialog
            // Java:    javax.swing.JFileChooser
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel文件(*.xlsx)|*.xlsx",  // 文件过滤器
                DefaultExt = "xlsx",                   // 默认扩展名
                FileName = $"人脸数据_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",  // 默认文件名
                Title = "导出人脸数据"                  // 对话框标题
            };

            // 显示对话框 - 类似于 Java: int result = chooser.showSaveDialog(parent);
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string filePath = saveFileDialog.FileName;  // 获取用户选择的路径

            try
            {
                // 调用服务层导出 - 类似于 Java 调用 service.exportToExcel()
                _faceService.ExportToExcel(_registeredFaces, filePath);

                MessageBox.Show($"数据已成功导出到:\n{filePath}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus($"已导出到: {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== 其他按钮事件 ====================

        /// <summary>
        /// 刷新列表按钮
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadRegisteredFaces();
            MessageBox.Show("列表已刷新!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 删除选中的人脸
        /// </summary>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (listViewFaces.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要删除的人脸!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = listViewFaces.SelectedItems[0];
            var face = selectedItem.Tag as FaceRecord;

            if (face == null) return;

            // 确认对话框 - 类似于 Java: int result = JOptionPane.showConfirmDialog(...)
            var result = MessageBox.Show($"确定要删除人脸 \"{face.Name}\" 吗?", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 从数据库删除
                _faceService.DeleteFace(face.Id);

                // 删除图片文件
                if (!string.IsNullOrEmpty(face.FaceImagePath) && File.Exists(face.FaceImagePath))
                {
                    try
                    {
                        File.Delete(face.FaceImagePath);
                    }
                    catch { }
                }

                LoadRegisteredFaces();
                UpdateStatus("已删除人脸");
            }
        }

        // ==================== UI 更新线程安全 ====================

        /// <summary>
        /// 更新状态栏 - 线程安全版本
        ///
        /// WinForms UI 更新规则:
        /// - 所有 UI 操作必须在主线程 (UI 线程) 中执行
        /// - 从其他线程更新 UI 需要使用 Invoke
        ///
        /// 对比 Java Swing:
        /// - WinForms: Control.Invoke() / BeginInvoke()
        /// - Java: SwingUtilities.invokeLater() / EventQueue.invokeLater()
        /// </summary>
        private void UpdateStatus(string message)
        {
            // 检查是否在主线程 - 类似于 Java: EventQueue.isDispatchThread()
            if (InvokeRequired)
            {
                // 切换到主线程执行 - 类似于 Java: SwingUtilities.invokeLater()
                Invoke(new Action(() => lblStatus.Text = message));
            }
            else
            {
                lblStatus.Text = message;
            }
        }

        // ==================== 窗体关闭清理 ====================

        /// <summary>
        /// 窗体关闭事件 - 释放资源
        /// 类似于 Java 的 windowClosing 或 @PreDestroy
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 停止捕获线程
            _isRunning = false;

            // 等待线程结束 - 类似于 Java: thread.join(1000)
            if (_captureThread != null && _captureThread.IsAlive)
            {
                _captureThread.Join(1000);
            }

            // 释放 OpenCV 资源 - 类似于 Java: capture.release()
            _capture?.Release();
            _capture?.Dispose();
            _faceCascade?.Dispose();
            _currentFrame?.Dispose();
        }
    }
}
