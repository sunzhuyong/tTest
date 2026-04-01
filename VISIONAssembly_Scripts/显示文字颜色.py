# ============================================================
# VISIONAssembly Python 脚本案例 - 文字颜色显示
# ============================================================
# 功能：根据 "Test" 参数显示不同颜色的文字
# Test = 1: 显示绿色
# Test = 0: 显示红色
# ============================================================

# -------------------- 1. 获取参数 --------------------
# 从工具中获取 Test 参数值
# 使用 GvTool.GetToolData("参数名") 获取参数
test_value = GvTool.GetToolData("脚本测试_Tab.测试参数")

# 获取文字显示位置的 X 坐标 (可选，默认 100)
pos_x = GvTool.GetToolData("脚本测试_Tab.位置X")
if pos_x is None:
    pos_x = 100  # 默认值

# 获取文字显示位置的 Y 坐标 (可选，默认 100)
pos_y = GvTool.GetToolData("脚本测试_Tab.位置Y")
if pos_y is None:
    pos_y = 100  # 默认值

# 获取字体大小 (可选，默认 30)
font_size = GvTool.GetToolData("脚本测试_Tab.字体大小")
if font_size is None:
    font_size = 30  # 默认值

# -------------------- 2. 根据参数判断颜色 --------------------
# 颜色格式: [R, G, B]，范围 0-255
# 绿色: [0, 255, 0]
# 红色: [255, 0, 0]

if test_value == 1:
    # Test = 1，显示绿色
    text_content = "OK - 状态正常"
    text_color = [0, 255, 0]  # 绿色
elif test_value == 0:
    # Test = 0，显示红色
    text_content = "NG - 状态异常"
    text_color = [255, 0, 0]  # 红色
else:
    # 其他值，显示黄色作为警告
    text_content = "未知状态: " + str(test_value)
    text_color = [255, 255, 0]  # 黄色

# -------------------- 3. 创建 GUI 风格 --------------------
# 使用 GvVisionAssembly.GsScriptGuiStyle() 创建风格对象
gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.bVisible = True           # 设置为可见
gui_style.nLineStyle = 0            # 线型 (0=实线)
gui_style.nLineWidth = 2            # 线宽
gui_style.clrLineColor = text_color # 设置文字颜色 [R, G, B]
gui_style.bLabelVisible = True       # 显示标签
gui_style.strLabelFont = "Arial"    # 字体 (可选: 宋体, 黑体, Arial)
gui_style.lFontSize = font_size     # 字体大小

# -------------------- 4. 创建文字 GUI --------------------
# 使用 GvVisionAssembly.GsScriptGuiText() 创建文字对象
gui_text = GvVisionAssembly.GsScriptGuiText()
gui_text.sScriptGuiStyle = gui_style    # 设置风格
gui_text.strText = text_content         # 设置文字内容
gui_text.posX = pos_x                   # 设置 X 坐标
gui_text.posY = pos_y                   # 设置 Y 坐标
gui_text.deg = 0.0                      # 设置旋转角度 (0度)

# -------------------- 5. 添加到 GUI 数组并显示 --------------------
# 创建 GUI 数组
gui_array = GvVisionAssembly.GcScriptGuiArray()
gui_array.Add(gui_text)  # 添加文字到数组

# 将 GUI 显示到视图窗口 "View-1"
# 可选其他视图: "View-2", "View-3", "View-4" 等
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)

# -------------------- 6. 消息报告 (可选) --------------------
# 报告当前状态
report_type = GvVisionAssembly.GeMsgReportType
if test_value == 1:
    GvVisionAssembly.ReportMessage("显示绿色文字: " + text_content, report_type.eMRTInfo, True)
else:
    GvVisionAssembly.ReportMessage("显示红色文字: " + text_content, report_type.eMRTWarn, True)
