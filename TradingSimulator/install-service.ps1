# 模拟炒股 - Windows服务安装脚本
# 以管理员身份运行

param(
    [string]$Action = "install"  # install, uninstall, start, stop
)

$serviceName = "TradingSimulator"
$displayName = "模拟炒股自动交易服务"
$exePath = "$PSScriptRoot\bin\Debug\net10.0-windows\TradingSimulator.exe"
$arguments = "--web"

function Install-Service {
    Write-Host "安装服务: $serviceName" -ForegroundColor Cyan

    # 检查exe是否存在
    if (-not (Test-Path $exePath)) {
        Write-Host "错误: 找不到 $exePath" -ForegroundColor Red
        Write-Host "请先编译项目: dotnet build" -ForegroundColor Yellow
        exit 1
    }

    # 创建服务
    $existing = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Host "服务已存在，正在删除..." -ForegroundColor Yellow
        sc.exe delete $serviceName
        Start-Sleep 2
    }

    # 使用 nssm 或直接创建
    New-Service -Name $serviceName -DisplayName $displayName -StartupType Automatic -BinaryPathName "$exePath $arguments" -ErrorAction SilentlyContinue

    if ($?) {
        Write-Host "服务安装成功!" -ForegroundColor Green
        Write-Host "启动服务: Start-Service $serviceName" -ForegroundColor Cyan
    } else {
        Write-Host "使用备用方法创建服务..." -ForegroundColor Yellow
        sc.exe create $serviceName binPath= "$exePath $arguments" start= auto
        sc.exe description $serviceName "模拟A股自动交易系统"
    }

    # 自动启动
    Start-Service $serviceName
    Write-Host "服务已启动" -ForegroundColor Green
}

function Uninstall-Service {
    Write-Host "卸载服务: $serviceName" -ForegroundColor Cyan
    Stop-Service $serviceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $serviceName
    Write-Host "服务已删除" -ForegroundColor Green
}

function Start-TradingService {
    Start-Service $serviceName
    Write-Host "服务已启动" -ForegroundColor Green
}

function Stop-TradingService {
    Stop-Service $serviceName -Force
    Write-Host "服务已停止" -ForegroundColor Green
}

switch ($Action) {
    "install" { Install-Service }
    "uninstall" { Uninstall-Service }
    "start" { Start-TradingService }
    "stop" { Stop-TradingService }
    default {
        Write-Host "用法: .\install-service.ps1 [-Action install|uninstall|start|stop]" -ForegroundColor Yellow
    }
}
