# VISIONAssembly Python 脚本 API 参考手册

> 基于官方帮助文档整理，面向对象、易于查阅的脚本开发指南

---

## 目录

1. [快速入门](#1-快速入门)
2. [核心API](#2-核心api)
3. [数据类型](#3-数据类型)
4. [GUI显示](#4-gui显示)
5. [消息报告](#5-消息报告)
6. [常用示例](#6-常用示例)

---

## 1. 快速入门

### 1.1 基本语法

```python
# 变量定义（无需类型声明）
a = 1
b = 3.14
flag = True  # 注意：布尔类型首字母大写

# 条件判断
if a > b:
    print("a > b")
elif a == b:
    print("a == b")
else:
    print("a < b")

# for循环
for i in range(10):
    print(i)

# while循环
i = 0
while i < 10:
    print(i)
    i += 1

# 逻辑运算
if a > 0 and b > 0:   # 与
    print("both positive")
if a > 0 or b > 0:   # 或
    print("at least one positive")
if not flag:          # 非
    print("flag is False")
```

### 1.2 缩进规则
- **必须使用空格**，不能使用 Tab
- 缩进 4 个空格表示一个层级

---

## 2. 核心API

### 2.1 GvTool - 工具参数操作

| 方法 | 说明 | 示例 |
|------|------|------|
| `GetToolData("工具名.参数名")` | 获取工具参数 | `x = GvTool.GetToolData("找圆工具_001.圆心.X")` |
| `SetToolData("工具名.参数名", 值)` | 设置工具参数 | `GvTool.SetToolData("找圆工具_001.半径", 50.0)` |
| `RefToolData("工具名.参数名")` | 获取参数引用（不推荐） | `ref = GvTool.RefToolData("找圆工具_001.圆心")` |

#### 获取嵌套参数

```python
# 获取圆心坐标（分量的方式）
x = GvTool.GetToolData("找圆工具_001.圆心.X")
y = GvTool.GetToolData("找圆工具_001.圆心.Y")
r = GvTool.GetToolData("找圆工具_001.半径")

# 获取整体对象的方式
center = GvTool.GetToolData("找圆工具_001.圆心")
x = center.GetX()
y = center.GetY()
r = center.GetRadius()
```

#### 设置上下限参数

```python
# 方法1：直接设置值
GvTool.SetToolData("Blob工具_001.总像素数上限", GvVisionAssembly.Limit_Int(1000))
GvTool.SetToolData("找线工具_003.直线变化角度上限", GvVisionAssembly.Limit_Double(10.0))

# 方法2：获取后修改
upper = GvTool.GetToolData("Blob工具_001.结果个数上限")
upper.Value = 1000
upper.Enable = True
GvTool.SetToolData("Blob工具_001.总像素数上限", upper)
```

### 2.2 GvVar - 变量管理

| 方法 | 说明 | 示例 |
|------|------|------|
| `GetVar("#变量名")` | 获取变量 | `val = GvVar.GetVar("#L0")` |
| `SetVar("#变量名", 值)` | 设置变量 | `GvVar.SetVar("#L0", 50)` |

```python
# 获取变量
a = GvVar.GetVar("#L0")
x = GvVar.GetVar("#X")
y = GvVar.GetVar("#Y")

# 设置变量
GvVar.SetVar("#L0", 100)
```

### 2.3 GvVisionAssembly - 核心功能

| 方法/属性 | 说明 |
|----------|------|
| `Limit_Int(值)` | 创建整型上下限 |
| `Limit_Double(值)` | 创建浮点型上下限 |
| `ReportMessage(消息, 类型, 是否禁止弹窗)` | 消息报告 |
| `GsScriptGuiStyle()` | 创建GUI样式 |
| `GsScriptGuiText()` | 创建文本 |
| `GsScriptGuiRect()` | 创建矩形 |
| `GsScriptGuiCircle()` | 创建圆形 |
| `GsScriptGuiLine()` | 创建直线 |
| `GsScriptGuiCross()` | 创建十字 |
| `GcScriptGuiArray()` | 创建GUI数组 |
| `sc2Vector(x, y)` | 创建二维向量 |
| `scCircle(圆心, 半径)` | 创建圆形 |
| `scRect(左上角, 右下角)` | 创建矩形 |
| `scLine(起点, 终点)` | 创建直线 |

### 2.4 GvGuiDataAgent - GUI显示

```python
# 显示GUI到视图（会清空之前的显示）
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

---

## 3. 数据类型

### 3.1 sc2Vector - 二维向量

用于表示坐标点（圆心、直线端点等）

#### 构造函数

```python
# 默认构造，坐标为(0, 0)
vec = GvVisionAssembly.sc2Vector()

# 指定坐标
vec = GvVisionAssembly.sc2Vector(100, 200)
```

#### 方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetX()` | float | 获取X坐标 |
| `GetY()` | float | 获取Y坐标 |
| `SetX(值)` | bool | 设置X坐标 |
| `SetY(值)` | bool | 设置Y坐标 |

#### 运算

```python
a = GvVisionAssembly.sc2Vector(10, 20)
b = GvVisionAssembly.sc2Vector(5, 10)

c = a + b  # 结果: (15, 30)
d = a - b  # 结果: (5, 10)
```

### 3.2 scCircle - 圆形

#### 构造函数

```python
center = GvVisionAssembly.sc2Vector(100, 200)
circle = GvVisionAssembly.scCircle(center, 50)  # 圆心(100,200)，半径50
```

#### 方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetCenter()` | sc2Vector | 获取圆心 |
| `GetRadius()` | float | 获取半径 |
| `SetCenter(sc2Vector)` | - | 设置圆心 |
| `SetRadius(float)` | - | 设置半径 |

#### 获取圆的属性

```python
# 假设从工具获取到圆形数据
circle = GvTool.GetToolData("找圆工具_001.圆结果")

# 获取属性
center = circle.GetCenter()
x = center.GetX()
y = center.GetY()
r = circle.GetRadius()
```

### 3.3 scLine - 直线

```python
start = GvVisionAssembly.sc2Vector(0, 0)
end = GvVisionAssembly.sc2Vector(100, 100)
line = GvVisionAssembly.scLine(start, end)
```

### 3.4 scRect - 矩形

```python
top_left = GvVisionAssembly.sc2Vector(10, 10)
bottom_right = GvVisionAssembly.sc2Vector(200, 150)
rect = GvVisionAssembly.scRect(top_left, bottom_right)
```

---

## 4. GUI显示

### 4.1 GUI显示流程

```
1. 创建样式 (GsScriptGuiStyle)
2. 创建图形 (GsScriptGuiXxx)
3. 关联样式和图形
4. 添加到数组 (GcScriptGuiArray)
5. 显示到视图 (GvGuiDataAgent.SetGraphicDisplay)
```

### 4.2 GsScriptGuiStyle - 样式属性

```python
style = GvVisionAssembly.GsScriptGuiStyle()

# 常用属性
style.clrLineColor = [255, 0, 0]    # 线条颜色 [R, G, B]
style.nLineWidth = 2                 # 线宽
style.nLineStyle = 0                 # 线型 (0=实线, 1=虚线, ...)
style.lFontSize = 20                # 字体大小
style.bVisible = True               # 是否可见
style.bLabelVisible = True          # 是否显示标签
```

### 4.3 显示圆形

```python
# 创建样式
gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.clrLineColor = [0, 255, 0]  # 绿色
gui_style.nLineWidth = 2

# 创建圆形
guiCircle = GvVisionAssembly.GsScriptGuiCircle()
center = GvVisionAssembly.sc2Vector(100, 200)
guiCircle.circle = GvVisionAssembly.scCircle(center, 50)
guiCircle.sScriptGuiStyle = gui_style

# 添加到数组并显示
gui_array = GvVisionAssembly.GcScriptGuiArray()
gui_array.Add(guiCircle)
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

### 4.4 显示文本

```python
gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.clrLineColor = [255, 0, 0]
gui_style.lFontSize = 30

gui_text = GvVisionAssembly.GsScriptGuiText()
gui_text.strText = "第1个圆"      # 文本内容
gui_text.posX = 100                # X坐标
gui_text.posY = 200                # Y坐标
gui_text.sScriptGuiStyle = gui_style

gui_array = GvVisionAssembly.GcScriptGuiArray()
gui_array.Add(gui_text)
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

### 4.5 显示矩形

```python
gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.clrLineColor = [0, 0, 255]

gui_rect = GvVisionAssembly.GsScriptGuiRect()
gui_rect.rect = GvVisionAssembly.scRect(
    GvVisionAssembly.sc2Vector(10, 10),
    GvVisionAssembly.sc2Vector(200, 150)
)
gui_rect.sScriptGuiStyle = gui_style

gui_array = GvVisionAssembly.GcScriptGuiArray()
gui_array.Add(gui_rect)
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

### 4.6 显示直线

```python
gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.clrLineColor = [255, 255, 0]

gui_line = GvVisionAssembly.GsScriptGuiLine()
gui_line.line = GvVisionAssembly.scLine(
    GvVisionAssembly.sc2Vector(0, 0),
    GvVisionAssembly.sc2Vector(100, 100)
)
gui_line.sScriptGuiStyle = gui_style

gui_array = GvVisionAssembly.GcScriptGuiArray()
gui_array.Add(gui_line)
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

### 4.7 多个图形同时显示

```python
gui_array = GvVisionAssembly.GcScriptGuiArray()

# 添加圆形
guiCircle = GvVisionAssembly.GsScriptGuiCircle()
guiCircle.circle = GvVisionAssembly.scCircle(
    GvVisionAssembly.sc2Vector(100, 100), 50
)
guiCircle.sScriptGuiStyle = gui_style
gui_array.Add(guiCircle)

# 添加文本
gui_text = GvVisionAssembly.GsScriptGuiText()
gui_text.strText = "圆1"
gui_text.posX = 100
gui_text.posY = 80
gui_text.sScriptGuiStyle = gui_style
gui_array.Add(gui_text)

# 一次性显示
GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

---

## 5. 消息报告

### 5.1 消息类型

```python
# 导入消息类型
msg_type = GvVisionAssembly.GeMsgReportType

# 可用类型
msg_type.eMRTFatalErr   # 致命错误
msg_type.eMRTErr        # 错误
msg_type.eMRTWarn       # 警告
msg_type.eMRTInfo       # 信息
msg_type.eMRTDebug      # 调试
```

### 5.2 使用示例

```python
# 报告错误消息
msg_type = GvVisionAssembly.GeMsgReportType
GvVisionAssembly.ReportMessage("未找到圆", msg_type.eMRTErr, False)

# 报告警告
GvVisionAssembly.ReportMessage("半径过小", msg_type.eMRTWarn, False)

# 报告信息
GvVisionAssembly.ReportMessage("处理完成", msg_type.eMRTInfo, False)
```

---

## 6. 常用示例

### 6.1 获取找圆工具结果并显示

```python
from ScImageShow import ScImageShow

# 获取变量
pos_i = int(GvVar.GetVar("#i"))

# 获取找圆结果
center = GvTool.GetToolData("找圆工具_001.圆心")
r = GvTool.GetToolData("找圆工具_001.半径")

# 创建GUI
gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.clrLineColor = [0, 255, 0]
gui_style.lFontSize = 30

# 圆形
guiCircle = GvVisionAssembly.GsScriptGuiCircle()
guiCircle.circle = GvVisionAssembly.scCircle(center, r)
guiCircle.sScriptGuiStyle = gui_style

# 文本
gui_text = GvVisionAssembly.GsScriptGuiText()
gui_text.strText = f"第 {pos_i + 1} 个圆"
gui_text.posX = center.GetX() - 50
gui_text.posY = center.GetY()
gui_text.sScriptGuiStyle = gui_style

# 显示
gui_array = GvVisionAssembly.GcScriptGuiArray()
gui_array.Add(gui_text)
gui_array.Add(guiCircle)

GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

### 6.2 获取数组解析工具结果

```python
# 数组解析工具输出的是数组，需要用 GetTrans() 方法获取
result = GvTool.GetToolData("数组解析工具_001.输出数据")
data = result.GetTrans()

x = data[0]  # X坐标
y = data[1]  # Y坐标
r = data[2]  # 半径（如果是圆的话）
```

### 6.3 获取多条直线结果

```python
# 获取找线工具的结果数组
lines = GvTool.GetToolData("找线工具_001.执行结果")
count = len(lines)

for i in range(count):
    line = lines[i]
    start = line.GetStart()
    end = line.GetEnd()
    print(f"直线{i+1}: ({start.GetX()}, {start.GetY()}) -> ({end.GetX()}, {end.GetY()})")
```

### 6.4 完整：显示多个圆

```python
# 假设使用多圆多线查找工具
circles = GvTool.GetToolData("多圆多线查找工具_001.圆结果数组")
count = len(circles)

gui_style = GvVisionAssembly.GsScriptGuiStyle()
gui_style.clrLineColor = [0, 255, 0]
gui_style.lFontSize = 25

gui_array = GvVisionAssembly.GcScriptGuiArray()

for i in range(count):
    circle = circles[i]
    center = circle.GetCenter()
    r = circle.GetRadius()
    
    # 添加圆形
    guiCircle = GvVisionAssembly.GsScriptGuiCircle()
    guiCircle.circle = GvVisionAssembly.scCircle(center, r)
    guiCircle.sScriptGuiStyle = gui_style
    gui_array.Add(guiCircle)
    
    # 添加文本标签
    gui_text = GvVisionAssembly.GsScriptGuiText()
    gui_text.strText = f"圆{i+1}"
    gui_text.posX = center.GetX()
    gui_text.posY = center.GetY() - 30
    gui_text.sScriptGuiStyle = gui_style
    gui_array.Add(gui_text)

GvGuiDataAgent.SetGraphicDisplay("View-1", gui_array)
```

---

## 附录：颜色参考

| 颜色 | RGB值 |
|------|-------|
| 红色 | [255, 0, 0] |
| 绿色 | [0, 255, 0] |
| 蓝色 | [0, 0, 255] |
| 黄色 | [255, 255, 0] |
| 紫色 | [255, 0, 255] |
| 青色 | [0, 255, 255] |
| 白色 | [255, 255, 255] |
| 黑色 | [0, 0, 0] |

---

*整理自 VISIONAssembly 官方帮助文档*