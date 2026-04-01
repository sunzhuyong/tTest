"""
A股板块/基金实时监控脚本
功能：定时获取行情，监控涨跌幅，超过阈值时通过飞书/钉钉通知

支持飞书和钉钉 webhook 推送

使用前配置:
    1. 获取 webhook URL（见下方教程）
    2. 修改 WEBHOOK_URL 为你的 URL
    3. 修改 NOTIFY_TYPE 为 "feishu" 或 "dingtalk"

使用方法:
    python stock_monitor.py           # 运行一次
    python stock_monitor.py --loop   # 持续监控
    python stock_monitor.py --test    # 测试通知
"""

import time
import sys
import random
import json
import urllib.request
import urllib.error
from datetime import datetime
from pathlib import Path

# ==================== 配置区域 ====================

# 监控标的（可以添加更多）
WATCH_LIST = {
    "BK0866": "存储芯片",
    "BK0021": "半导体",
    "SH603986": "兆易创新",
    "SZ000021": "深科技",
    "SH513310": "中韩半导体ETF",
}

# 涨跌幅阈值（超过此值通知）
ALERT_THRESHOLD = 3.0  #%

# 检查间隔（秒）
CHECK_INTERVAL = 300  # 5分钟

# ========== 通知配置 ==========
# 通知类型: "feishu" 或 "dingtalk"
NOTIFY_TYPE = "feishu"

# 飞书 webhook
WEBHOOK_URL = "https://open.feishu.cn/open-apis/bot/v2/hook/0260cbcc-f091-4a82-8ad3-27bd664af86c"

# 钉钉 webhook（获取方法见下方）
# 格式: https://oapi.dingtalk.com/robot/send?access_token=xxxxx
DINGTALK_WEBHOOK = "YOUR_DINGTALK_WEBHOOK_URL"

# 开启通知
ENABLE_NOTIFY = True

# 日志文件
LOG_FILE = Path(__file__).parent / "stock_monitor.log"

# ==================== 价格获取 ====================

def get_stock_price(symbol):
    """获取股票/板块行情（模拟数据）"""
    base_prices = {
        "BK0866": 892.26,
        "BK0021": 17043.29,
        "SH603986": 255.00,
        "SZ000021": 26.67,
        "SH513310": 3.619,
    }

    base = base_prices.get(symbol, 100)
    change = random.uniform(-3, 3)
    price = base * (1 + change / 100)

    return {
        "name": WATCH_LIST.get(symbol, symbol),
        "price": round(price, 2),
        "change": round(change, 2)
    }

# ==================== 通知功能 ====================

def log(msg):
    """日志记录"""
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    log_msg = f"[{timestamp}] {msg}"
    print(log_msg)

    try:
        with open(LOG_FILE, "a", encoding="utf-8") as f:
            f.write(log_msg + "\n")
    except:
        pass

def send_feishu(message):
    """发送飞书通知"""
    try:
        data = {
            "msg_type": "text",
            "content": {"text": message}
        }

        req = urllib.request.Request(
            WEBHOOK_URL,
            data=json.dumps(data).encode('utf-8'),
            headers={'Content-Type': 'application/json'}
        )

        with urllib.request.urlopen(req, timeout=10) as response:
            result = json.loads(response.read().decode('utf-8'))
            if result.get('code') == 0:
                log("飞书通知发送成功")
            else:
                log(f"飞书通知失败: {result}")
    except Exception as e:
        log(f"飞书通知失败: {e}")

def send_dingtalk(message):
    """发送钉钉通知"""
    try:
        data = {
            "msgtype": "text",
            "text": {"content": message}
        }

        req = urllib.request.Request(
            DINGTALK_WEBHOOK,
            data=json.dumps(data).encode('utf-8'),
            headers={'Content-Type': 'application/json'}
        )

        with urllib.request.urlopen(req, timeout=10) as response:
            result = json.loads(response.read().decode('utf-8'))
            if result.get('errcode') == 0:
                log("钉钉通知发送成功")
            else:
                log(f"钉钉通知失败: {result}")
    except Exception as e:
        log(f"钉钉通知失败: {e}")

def send_notification(title, message):
    """发送通知"""
    if not ENABLE_NOTIFY:
        return

    full_message = f"{title}\n{message}"

    if NOTIFY_TYPE == "feishu":
        send_feishu(full_message)
    elif NOTIFY_TYPE == "dingtalk":
        send_dingtalk(full_message)
    else:
        # Windows 弹窗
        try:
            import subprocess
            subprocess.run([
                "powershell", "-Command",
                f'Add-Type -AssemblyName System.Windows.Forms; '
                f'[System.Windows.Forms.MessageBox]::Show("{message}", "{title}", 0, 48)'
            ], capture_output=True, timeout=5)
        except:
            pass

    print(f"\n{'='*50}")
    print(f"[NOTIFY] {title}")
    print(f"   {message}")
    print(f"{'='*50}\n")

# ==================== 核心功能 ====================

def check_and_notify():
    """检查所有监控标的"""
    log("=" * 50)
    log("开始检查...")

    alerts = []

    for symbol, name in WATCH_LIST.items():
        data = get_stock_price(symbol)

        if data:
            change = data["change"]
            price = data["price"]

            change_str = f"+{change:.2f}%" if change > 0 else f"{change:.2f}%"
            log(f"{name}({symbol}): {price} ({change_str})")

            if abs(change) >= ALERT_THRESHOLD:
                direction = "↑" if change > 0 else "↓"
                alerts.append(f"{name}: {change_str} {direction}")

    if alerts:
        title = "⚠️ A股监控提醒"
        message = "\n".join(alerts)
        send_notification(title, message)
        log(f"触发通知: {alerts}")
    else:
        log("无异常")

    log("=" * 50)

# ==================== 主程序 ====================

def main():
    args = sys.argv[1:] if len(sys.argv) > 1 else []

    if "--test" in args:
        send_notification("测试通知", "A股监控系统正常工作！")
        return

    if "--setup" in args:
        print("="*60)
        print("Webhook 配置教程")
        print("="*60)
        print("""
【飞书 webhook 获取方法】
1. 打开飞书电脑版
2. 点击左上角头像 → 设置 → 机器人
3. 点击"创建机器人"
4. 输入名称（如"A股监控"）
5. 复制 webhook 地址，填入上方配置

【钉钉 webhook 获取方法】
1. 打开钉钉电脑版
2. 点击右上角"智能助手"
3. 点击"添加机器人"
4. 选择"自定义机器人"
5. 设置名称，安全设置（可选）
6. 复制 webhook 地址，填入上方配置

配置好后再运行:
    python stock_monitor.py --test
测试通知是否正常。
""")
        return

    if "--loop" in args:
        log("A股监控系统启动（持续监控模式）")
        log(f"通知类型: {NOTIFY_TYPE}")
        log(f"监控标的: {list(WATCH_LIST.values())}")
        log(f"涨跌幅阈值: {ALERT_THRESHOLD}%")
        log(f"检查间隔: {CHECK_INTERVAL}秒")

        while True:
            try:
                check_and_notify()
            except Exception as e:
                log(f"检查出错: {e}")

            time.sleep(CHECK_INTERVAL)
    else:
        log("运行一次检查...")
        check_and_notify()

if __name__ == "__main__":
    main()