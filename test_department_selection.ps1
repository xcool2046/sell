# 测试部门选择功能
Write-Host "=== 测试部门选择功能 ===" -ForegroundColor Green

# 设置工作目录
$workDir = "e:\download-\sell-master\sell-feature-sellsys-complete-system\sell-feature-sellsys-complete-system"
Set-Location $workDir

# 终止现有进程
Write-Host "终止现有进程..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -like "*Sellsys*" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# 编译项目
Write-Host "编译项目..." -ForegroundColor Yellow
dotnet build --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "编译成功!" -ForegroundColor Green
} else {
    Write-Host "编译失败!" -ForegroundColor Red
    exit 1
}

# 启动WebAPI
Write-Host "启动WebAPI..." -ForegroundColor Yellow
$webApiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Sellsys.WebApi" -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 5

# 检查WebAPI是否启动成功
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method Get -TimeoutSec 5
    Write-Host "WebAPI启动成功!" -ForegroundColor Green
} catch {
    Write-Host "WebAPI启动失败!" -ForegroundColor Red
    $webApiProcess | Stop-Process -Force -ErrorAction SilentlyContinue
    exit 1
}

# 启动WPF客户端
Write-Host "启动WPF客户端..." -ForegroundColor Yellow
$clientProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Sellsys.WpfClient" -PassThru

Write-Host "`n=== 测试说明 ===" -ForegroundColor Cyan
Write-Host "1. 使用管理员账号登录 (admin/123456)" -ForegroundColor White
Write-Host "2. 进入客户管理模块" -ForegroundColor White
Write-Host "3. 选择一个客户，点击'分配销售'或'分配客服'" -ForegroundColor White
Write-Host "4. 验证部门名称现在是下拉框而不是固定文本" -ForegroundColor White
Write-Host "5. 选择不同的部门，验证分组和人员是否正确更新" -ForegroundColor White
Write-Host "6. 完成分配操作，验证功能是否正常工作" -ForegroundColor White

Write-Host "`n=== 预期结果 ===" -ForegroundColor Cyan
Write-Host "✓ 部门名称显示为可选择的下拉框" -ForegroundColor Green
Write-Host "✓ 默认选择'销售部'或'客服部'(如果存在)" -ForegroundColor Green
Write-Host "✓ 选择部门后自动加载对应的分组" -ForegroundColor Green
Write-Host "✓ 选择分组后自动加载对应的人员" -ForegroundColor Green
Write-Host "✓ 分配功能正常工作" -ForegroundColor Green

Write-Host "`n按任意键停止测试..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# 清理进程
Write-Host "`n清理进程..." -ForegroundColor Yellow
$webApiProcess | Stop-Process -Force -ErrorAction SilentlyContinue
$clientProcess | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process | Where-Object { $_.ProcessName -like "*Sellsys*" } | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host "测试完成!" -ForegroundColor Green
