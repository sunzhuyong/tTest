import os
import shutil
import GvVisionAssembly

genote = GvVisionAssembly.GeMsgReportType
noteType = genote.eMRTNote

# ==============================
# 工具函数：安全拼接路径（杜绝 \\ 硬编码）
# ==============================
def safe_join(parent, name):
    return os.path.join(parent, name)

# ==============================
# 1. 移动单个文件（先复制→校验→删源，失败不删）
# ==============================
def MoveFileSafe(source, dest):
    if not os.path.exists(source):
        msg = f"MoveFileSafe: 源文件不存在 {source}"
        GvVisionAssembly.ReportMessage(msg, noteType, False)
        return False

    if not os.path.isfile(source):
        return False

    if not os.path.exists(dest):
        os.makedirs(dest)

    fname = os.path.basename(source)
    dest_file = safe_join(dest, fname)

    # 先复制
    try:
        shutil.copy2(source, dest_file)
    except Exception as e:
        GvVisionAssembly.ReportMessage(f"MoveFileSafe: 复制失败 {fname}", noteType, False)
        return False

    # 校验大小
    if os.path.getsize(source) != os.path.getsize(dest_file):
        GvVisionAssembly.ReportMessage(f"MoveFileSafe: 文件大小不一致 {fname}", noteType, False)
        return False

    # 确认成功再删源
    try:
        os.remove(source)
    except:
        GvVisionAssembly.ReportMessage(f"MoveFileSafe: 删源失败(已复制成功) {fname}", noteType, False)
        return False

    GvVisionAssembly.ReportMessage(f"MoveFileSafe: 成功 {fname}", noteType, True)
    return True

# ==============================
# 2. 移动整个目录（递归，失败不删目录）
# ==============================
def MoveDirSafe(source_dir, dest_dir):
    if not os.path.exists(source_dir):
        return False
    if source_dir == dest_dir:
        return False

    all_ok = True

    for name in os.listdir(source_dir):
        src = safe_join(source_dir, name)
        dst = safe_join(dest_dir, name)

        if os.path.isfile(src):
            if not MoveFileSafe(src, dest_dir):
                all_ok = False
        else:
            if not MoveDirSafe(src, dst):
                all_ok = False

    # 只有目录空了才删，绝不强制删
    if all_ok and len(os.listdir(source_dir)) == 0:
        try:
            os.rmdir(source_dir)
        except:
            pass

    return all_ok

# ==============================
# 3. 复制单个文件（安全）
# ==============================
def CopyFileSafe(source, dest):
    if not os.path.exists(source) or not os.path.isfile(source):
        return False

    if not os.path.exists(dest):
        os.makedirs(dest)

    fname = os.path.basename(source)
    dest_file = safe_join(dest, fname)

    try:
        shutil.copy2(source, dest_file)
        GvVisionAssembly.ReportMessage(f"CopyFileSafe: 成功 {fname}", noteType, True)
        return True
    except:
        GvVisionAssembly.ReportMessage(f"CopyFileSafe: 失败 {fname}", noteType, False)
        return False

# ==============================
# 4. 按关键字复制目录（递归）
# ==============================
def CopyDirByKey(source_dir, dest_dir, key=""):
    if not os.path.exists(source_dir):
        return False

    for name in os.listdir(source_dir):
        src = safe_join(source_dir, name)

        if os.path.isfile(src):
            if key in name or key == "":
                CopyFileSafe(src, dest_dir)
        else:
            CopyDirByKey(src, dest_dir, key)

    return True

# ==============================
# 5. 重命名目录（安全，存在则合并）
# ==============================
def RenameDirSafe(old_dir, new_dir):
    if not os.path.exists(old_dir):
        return False
    if old_dir == new_dir:
        return False

    if os.path.exists(new_dir):
        return MoveDirSafe(old_dir, new_dir)

    try:
        os.rename(old_dir, new_dir)
        return True
    except:
        GvVisionAssembly.ReportMessage("RenameDirSafe: 失败", noteType, False)
        return False

# ==============================
# 6. 合并目录（安全别名）
# ==============================
def MergeDirSafe(dir1, dir2):
    return RenameDirSafe(dir1, dir2)
    
    
    

# 移动文件
MoveFileSafe(r"D:\test\1.jpg", r"D:\test\ok")

# 移动目录
MoveDirSafe(r"D:\test\raw", r"D:\test\raw_bak")

# 复制文件
CopyFileSafe(r"D:\test\a.png", r"D:\test\copy")

# 按关键字复制目录
CopyDirByKey(r"D:\test\img", r"D:\test\img_result", "captured")

# 重命名
RenameDirSafe(r"D:\test\old", r"D:\test\new")

# 合并目录
MergeDirSafe(r"D:\test\A", r"D:\test\B")