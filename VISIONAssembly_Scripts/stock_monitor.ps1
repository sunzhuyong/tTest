# Stock Monitor Script
# A股板块/基金实时监控

$WatchList = @{
    "BK0866" = "存储芯片"
    "BK0021" = "半导体"
    "SH603986" = "兆易创新"
    "SZ000021" = "深科技"
    "SH513310" = "中韩半导体ETF"
}

$AlertThreshold = 3.0
$CheckInterval = 300
$LogFile = "$env:USERPROFILE\stock_monitor.log"

function Get-StockPrice {
    param([string]$Symbol)

    try {
        $output = wsl opencli xueqiu stock $Symbol 2>&1

        foreach ($line in $output) {
            if ($line -match $Symbol) {
                $parts = $line -split '\|'
                if ($parts.Count -ge 5) {
                    $price = $parts[3].Trim()
                    $changeStr = $parts[4].Trim() -replace '%','' -replace '\+',''

                    try {
                        return @{
                            Price = [double]$price
                            Change = [double]$changeStr
                        }
                    } catch {
                    }
                }
            }
        }
    } catch {
    }

    return $null
}

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMsg = "[$timestamp] $Message"
    Write-Host $logMsg
}

function Test-AndNotify {
    Write-Log "=================================================="
    Write-Log "Start checking..."

    $alerts = @()

    foreach ($symbol in $WatchList.Keys) {
        $name = $WatchList[$symbol]
        $data = Get-StockPrice -Symbol $symbol

        if ($data) {
            $change = $data.Change
            $price = $data.Price
            $changeStr = if ($change -gt 0) { "+" + $change.ToString("0.00") } else { $change.ToString("0.00") }

            Write-Log "$name ($symbol): $price ($changeStr%)"

            if ([Math]::Abs($change) -ge $AlertThreshold) {
                $direction = if ($change -gt 0) { "UP" } else { "DOWN" }
                $alerts += "$name`: $changeStr% ($direction)"
            }
        }
    }

    if ($alerts.Count -gt 0) {
        Write-Log "ALERT: $($alerts -join ', ')"
    } else {
        Write-Log "No alerts"
    }

    Write-Log "=================================================="
}

Write-Log "Stock Monitor Started"
Write-Log "Watching: $($WatchList.Values -join ', ')"
Write-Log "Threshold: $AlertThreshold%"

while ($true) {
    try {
        Test-AndNotify
    } catch {
        Write-Log "Error: $_"
    }

    Start-Sleep -Seconds $CheckInterval
}