from ScImageShow import ScImageShow

index = GvVar.GetVar("#index")
affinerect = GvTool.GetToolData("几何定位_002.搜索位置数组")[index]
Center = affinerect.GetCenter()
x = affinerect.GetCenter().GetX()-200
y = affinerect.GetCenter().GetY()

##Gui风格
guistyle = GvVisionAssembly.GsScriptGuiStyle()
guistyle.nLineWidth = 3
guistyle.clrLineColor = [0, 255, 0]

##仿射矩形Gui对象
affinerect_gui = GvVisionAssembly.GsScriptGuiAffineRect()
affinerect_gui.sScriptGuiStyle = guistyle
affinerect_gui.affineRect = affinerect

##Gui数组
guiarray = GvVisionAssembly.GcScriptGuiArray()
guiarray.Add(affinerect_gui)   # 将Gui对象添加到Gui数组

##将Gui数组添加到视图
ScImageShow.ImageShowTextXY(ScImageShow,guiarray,x,y,"第{}个".format(index+1),\
[100, 0, 0],200,0.0)#显示文字
ScImageShow.ImagechowCrossVec(ScImageShow,guiarray,Center, [0, 255, 0], 5)#显示十字交点
GvGuiDataAgent.SetGraphicDisplay("View-1", guiarray)

