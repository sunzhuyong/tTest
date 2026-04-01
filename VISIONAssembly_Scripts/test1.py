from ScImageShow import ScImageShow

# ==================== 1. 获取脚本变量 ====================
# 从系统中获取位置坐标和索引值
# #X, #Y: 文本显示的位置坐标
# #i: 圆形的索引序号
pos_i = int(GvVar.GetVar("#i"))          # 圆形索引（从0开始）

# ==================== 2. 获取找圆工具的结果 ====================
# 从"找圆工具_004"获取圆心坐标和半径
# 方法1：直接获取各分量（通过参数名访问）
center = GvTool.GetToolData("找圆工具_004.圆心")  # 圆心
r = GvTool.GetToolData("找圆工具_004.半径")    # 圆半径

# 打印调试信息
#print(">>>>>: ", x, y, r)

# ==================== 3. 创建 GUI 图形元素 ====================

# 3.1 创建圆形
guiCircle = GvVisionAssembly.GsScriptGuiCircle()           # 创建圆形对象
guiCircle.circle = GvVisionAssembly.scCircle(center, r)     # 设置圆形（圆心+半径）

# 3.2 创建文本标注
gui_text = GvVisionAssembly.GsScriptGuiText()                # 创建文本对象
gui_text.strText = f"第 {pos_i + 1} 个圆"                    # 显示内容（索引+1，从1开始计数）
gui_text.posX = center.GetX()- 100                                        # 文本X位置
gui_text.posY = center.GetY()


# ==================== 4. 设置样式 ====================
# 创建样式对象并设置颜色（绿色）
gui_style = GvVisionAssembly.GsScriptGuiStyle()              # 创建样式对象
gui_style.clrLineColor = [0, 255, 0]                        # RGB颜色：绿色
gui_style.lFontSize = 50

# 将样式应用到圆形和文本
guiCircle.sScriptGuiStyle = gui_style
gui_text.sScriptGuiStyle = gui_style

# ==================== 5. 添加到显示数组 ====================
gui_array = GvVisionAssembly.GcScriptGuiArray()              # 创建图形数组
gui_array.Add(gui_text)                                       # 添加文本
gui_array.Add(guiCircle)                                     # 添加圆形

# ==================== 6. 显示结果 ====================
# 6.1 显示十字交点（可选，用于调试定位）

ScImageShow.ImagechowCrossVec(
    ScImageShow,
    gui_array,           # 图形数组
    center,              # 十字中心位置
    [0, 255, 0],         # 颜色（绿色）
    3                    # 线宽
)

# 6.2 在 View-1 窗口显示所有图形
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)