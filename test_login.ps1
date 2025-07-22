#!/usr/bin/env pwsh

# 测试登录流程的PowerShell脚本
Write-Host "=== 销售管理系统登录测试脚本 ===" -ForegroundColor Green

# 设置工作目录
$projectPath = "e:\download-\sell-master\sell-master"
Set-Location $projectPath

Write-Host "当前工作目录: $(Get-Location)" -ForegroundColor Yellow

# 1. 清理之前的进程
Write-Host "`n1. 清理之前的进程..." -ForegroundColor Cyan
Get-Process | Where-Object { $_.ProcessName -like "*Sellsys*" } | ForEach-Object {
    Write-Host "终止进程: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Red
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

# 等待进程完全退出
Start-Sleep -Seconds 2

# 2. 编译项目
Write-Host "`n2. 编译项目..." -ForegroundColor Cyan
$buildResult = dotnet build src/Sellsys.WpfClient --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败!" -ForegroundColor Red
    exit 1
}
Write-Host "编译成功!" -ForegroundColor Green

# 3. 启动应用程序
Write-Host "`n3. 启动应用程序..." -ForegroundColor Cyan
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Sellsys.WpfClient" -PassThru -WindowStyle Normal

Write-Host "应用程序已启动，进程ID: $($process.Id)" -ForegroundColor Green

# 4. 等待应用程序启动
Write-Host "`n4. 等待应用程序启动..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

# 5. 检查进程是否还在运行
Write-Host "`n5. 检查应用程序状态..." -ForegroundColor Cyan
$runningProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
if ($runningProcess) {
    Write-Host "✓ 应用程序正在运行" -ForegroundColor Green
    Write-Host "进程名称: $($runningProcess.ProcessName)" -ForegroundColor Yellow
    Write-Host "进程ID: $($runningProcess.Id)" -ForegroundColor Yellow
    Write-Host "内存使用: $([math]::Round($runningProcess.WorkingSet64/1MB, 2)) MB" -ForegroundColor Yellow
} else {
    Write-Host "✗ 应用程序已退出或崩溃" -ForegroundColor Red
}

# 6. 检查日志文件
Write-Host "`n6. 检查调试日志..." -ForegroundColor Cyan
$logPath = "$env:USERPROFILE\Desktop\wpf_debug.log"
if (Test-Path $logPath) {
    Write-Host "找到日志文件: $logPath" -ForegroundColor Green
    Write-Host "最新日志内容:" -ForegroundColor Yellow
    Get-Content $logPath -Tail 10 | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
} else {
    Write-Host "未找到日志文件: $logPath" -ForegroundColor Yellow
}

# 7. 检查窗口
Write-Host "`n7. 检查窗口状态..." -ForegroundColor Cyan
try {
    Add-Type -AssemblyName System.Windows.Forms
    $windows = [System.Windows.Forms.Application]::OpenForms
    if ($windows.Count -gt 0) {
        Write-Host "找到 $($windows.Count) 个窗口:" -ForegroundColor Green
        foreach ($window in $windows) {
            Write-Host "  - $($window.Text)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "未找到任何窗口" -ForegroundColor Yellow
    }
} catch {
    Write-Host "无法检查窗口状态: $($_.Exception.Message)" -ForegroundColor Red
}

# 8. 等待用户操作
Write-Host "`n8. 测试说明:" -ForegroundColor Cyan
Write-Host "  - 请在登录窗口中输入用户名: admin" -ForegroundColor White
Write-Host "  - 请在登录窗口中输入密码: admin" -ForegroundColor White
Write-Host "  - 点击登录按钮" -ForegroundColor White
Write-Host "  - 观察是否出现主窗口" -ForegroundColor White

Write-Host "`n按任意键继续监控..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# 9. 持续监控
Write-Host "`n9. 开始监控应用程序状态..." -ForegroundColor Cyan
for ($i = 1; $i -le 30; $i++) {
    $currentProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
    if ($currentProcess) {
        Write-Host "[$i/30] 应用程序运行中... (内存: $([math]::Round($currentProcess.WorkingSet64/1MB, 2)) MB)" -ForegroundColor Green
    } else {
        Write-Host "[$i/30] 应用程序已退出" -ForegroundColor Red
        break
    }
    Start-Sleep -Seconds 2
}

# 10. 清理
Write-Host "`n10. 清理进程..." -ForegroundColor Cyan
if (Get-Process -Id $process.Id -ErrorAction SilentlyContinue) {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    Write-Host "进程已终止" -ForegroundColor Yellow
}

Write-Host "`n=== 测试完成 ===" -ForegroundColor Green
