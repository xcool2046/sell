# Quick test script for department deletion functionality
param(
    [string]$BaseUrl = "http://localhost:5078/api"
)

Write-Host "Testing department deletion functionality..." -ForegroundColor Green

# 1. Check API health
Write-Host "Checking API status..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method Get -TimeoutSec 5
    Write-Host "API OK: $($health.data.status)" -ForegroundColor Green
} catch {
    Write-Host "API unavailable: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please ensure backend service is running at $BaseUrl" -ForegroundColor Yellow
    exit 1
}

# 2. 获取当前数据状态
Write-Host "获取当前数据..." -ForegroundColor Yellow
$depts = (Invoke-RestMethod -Uri "$BaseUrl/departments" -Method Get).data
$groups = (Invoke-RestMethod -Uri "$BaseUrl/departmentgroups" -Method Get).data

Write-Host "当前部门: $($depts.Count)个" -ForegroundColor Cyan
Write-Host "当前分组: $($groups.Count)个" -ForegroundColor Cyan

# 3. 创建测试数据
Write-Host "创建测试数据..." -ForegroundColor Yellow
$testDeptName = "API测试部门"

# 创建部门
$deptBody = "`"$testDeptName`""
$newDept = (Invoke-RestMethod -Uri "$BaseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "✓ 创建部门: ID=$($newDept.id)" -ForegroundColor Green

# 创建分组
$groupBody = @{ DepartmentId = $newDept.id; Name = "测试分组1" } | ConvertTo-Json
$newGroup1 = (Invoke-RestMethod -Uri "$BaseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data

$groupBody = @{ DepartmentId = $newDept.id; Name = "测试分组2" } | ConvertTo-Json  
$newGroup2 = (Invoke-RestMethod -Uri "$BaseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data

Write-Host "✓ 创建分组: $($newGroup1.id), $($newGroup2.id)" -ForegroundColor Green

# 4. 验证创建成功
$groupsAfter = (Invoke-RestMethod -Uri "$BaseUrl/departmentgroups" -Method Get).data
$testGroups = $groupsAfter | Where-Object { $_.departmentId -eq $newDept.id }
Write-Host "✓ 验证: 部门下有 $($testGroups.Count) 个分组" -ForegroundColor Green

# 5. 执行删除测试
Write-Host "执行删除测试..." -ForegroundColor Yellow
$deleteResult = Invoke-RestMethod -Uri "$BaseUrl/departments/$($newDept.id)" -Method Delete

Write-Host "删除结果:" -ForegroundColor Cyan
Write-Host "  成功: $($deleteResult.isSuccess)" -ForegroundColor Cyan
Write-Host "  消息: $($deleteResult.message)" -ForegroundColor Cyan

if ($deleteResult.data) {
    Write-Host "  删除的分组数: $($deleteResult.data.deletedGroupCount)" -ForegroundColor Cyan
    Write-Host "  分组名称: $($deleteResult.data.deletedGroupNames -join ', ')" -ForegroundColor Cyan
}

# 6. 验证删除结果
Write-Host "验证删除结果..." -ForegroundColor Yellow

# 检查部门是否被删除
$deptsAfter = (Invoke-RestMethod -Uri "$BaseUrl/departments" -Method Get).data
$deptExists = $deptsAfter | Where-Object { $_.id -eq $newDept.id }

if ($deptExists) {
    Write-Host "✗ 部门未被删除" -ForegroundColor Red
} else {
    Write-Host "✓ 部门已删除" -ForegroundColor Green
}

# 检查分组是否被删除
$groupsAfterDelete = (Invoke-RestMethod -Uri "$BaseUrl/departmentgroups" -Method Get).data
$remainingGroups = $groupsAfterDelete | Where-Object { $_.departmentId -eq $newDept.id }

if ($remainingGroups.Count -gt 0) {
    Write-Host "✗ 还有 $($remainingGroups.Count) 个分组未被删除" -ForegroundColor Red
} else {
    Write-Host "✓ 所有相关分组已删除" -ForegroundColor Green
}

Write-Host "`n测试完成！" -ForegroundColor Green
Write-Host "现在可以在前端界面验证数据同步是否正常。" -ForegroundColor Yellow
