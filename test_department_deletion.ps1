# 部门删除功能测试脚本
# 测试部门删除后，相关分组是否被正确删除，以及前端是否同步更新

Write-Host "=== 部门删除功能测试 ===" -ForegroundColor Green

$baseUrl = "http://localhost:5078/api"

# 检查API服务是否运行
Write-Host "1. 检查API服务状态..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    Write-Host "✓ API服务正常运行" -ForegroundColor Green
    Write-Host "  状态: $($healthResponse.data.status)" -ForegroundColor Cyan
    Write-Host "  数据库: $($healthResponse.data.database)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ API服务未运行或无法访问: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 获取当前部门列表
Write-Host "`n2. 获取当前部门列表..." -ForegroundColor Yellow
try {
    $departmentsResponse = Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get
    $departments = $departmentsResponse.data
    Write-Host "✓ 当前部门数量: $($departments.Count)" -ForegroundColor Green
    foreach ($dept in $departments) {
        Write-Host "  - ID: $($dept.id), 名称: $($dept.name), 分组数: $($dept.groups.Count)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "✗ 获取部门列表失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 获取当前部门分组列表
Write-Host "`n3. 获取当前部门分组列表..." -ForegroundColor Yellow
try {
    $groupsResponse = Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get
    $groups = $groupsResponse.data
    Write-Host "✓ 当前分组数量: $($groups.Count)" -ForegroundColor Green
    foreach ($group in $groups) {
        Write-Host "  - ID: $($group.id), 名称: $($group.name), 部门ID: $($group.departmentId), 部门名称: $($group.departmentName)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "✗ 获取分组列表失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 创建测试部门
Write-Host "`n4. 创建测试部门..." -ForegroundColor Yellow
$testDeptName = "测试部门_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
try {
    $createDeptResponse = Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body "`"$testDeptName`"" -ContentType "application/json"
    $testDeptId = $createDeptResponse.data.id
    Write-Host "✓ 创建测试部门成功: ID=$testDeptId, 名称=$testDeptName" -ForegroundColor Green
} catch {
    Write-Host "✗ 创建测试部门失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 为测试部门创建分组
Write-Host "`n5. 为测试部门创建分组..." -ForegroundColor Yellow
$testGroupNames = @("测试分组1", "测试分组2")
$testGroupIds = @()

foreach ($groupName in $testGroupNames) {
    try {
        $createGroupBody = @{
            DepartmentId = $testDeptId
            Name = $groupName
        } | ConvertTo-Json
        
        $createGroupResponse = Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $createGroupBody -ContentType "application/json"
        $groupId = $createGroupResponse.data.id
        $testGroupIds += $groupId
        Write-Host "✓ 创建分组成功: ID=$groupId, 名称=$groupName" -ForegroundColor Green
    } catch {
        Write-Host "✗ 创建分组失败: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 验证分组创建成功
Write-Host "`n6. 验证分组创建成功..." -ForegroundColor Yellow
try {
    $groupsAfterCreate = Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get
    $testGroups = $groupsAfterCreate.data | Where-Object { $_.departmentId -eq $testDeptId }
    Write-Host "✓ 测试部门下的分组数量: $($testGroups.Count)" -ForegroundColor Green
    foreach ($group in $testGroups) {
        Write-Host "  - ID: $($group.id), 名称: $($group.name)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "✗ 验证分组创建失败: $($_.Exception.Message)" -ForegroundColor Red
}

# 删除测试部门（这是关键测试）
Write-Host "`n7. 删除测试部门（关键测试）..." -ForegroundColor Yellow
try {
    $deleteResponse = Invoke-RestMethod -Uri "$baseUrl/departments/$testDeptId" -Method Delete
    Write-Host "✓ 删除部门请求成功" -ForegroundColor Green
    
    if ($deleteResponse.data) {
        Write-Host "  删除结果详情:" -ForegroundColor Cyan
        Write-Host "    - 部门ID: $($deleteResponse.data.departmentId)" -ForegroundColor Cyan
        Write-Host "    - 部门名称: $($deleteResponse.data.departmentName)" -ForegroundColor Cyan
        Write-Host "    - 删除的分组数量: $($deleteResponse.data.deletedGroupCount)" -ForegroundColor Cyan
        Write-Host "    - 删除的分组名称: $($deleteResponse.data.deletedGroupNames -join ', ')" -ForegroundColor Cyan
    }
    
    Write-Host "  响应消息: $($deleteResponse.message)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ 删除部门失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 验证部门已被删除
Write-Host "`n8. 验证部门已被删除..." -ForegroundColor Yellow
try {
    $departmentsAfterDelete = Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get
    $remainingDept = $departmentsAfterDelete.data | Where-Object { $_.id -eq $testDeptId }
    
    if ($remainingDept) {
        Write-Host "✗ 部门未被删除！" -ForegroundColor Red
    } else {
        Write-Host "✓ 部门已成功删除" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ 验证部门删除失败: $($_.Exception.Message)" -ForegroundColor Red
}

# 验证相关分组已被删除（关键验证）
Write-Host "`n9. 验证相关分组已被删除（关键验证）..." -ForegroundColor Yellow
try {
    $groupsAfterDelete = Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get
    $remainingGroups = $groupsAfterDelete.data | Where-Object { $_.departmentId -eq $testDeptId }
    
    if ($remainingGroups.Count -gt 0) {
        Write-Host "✗ 发现未删除的分组！数量: $($remainingGroups.Count)" -ForegroundColor Red
        foreach ($group in $remainingGroups) {
            Write-Host "  - ID: $($group.id), 名称: $($group.name)" -ForegroundColor Red
        }
    } else {
        Write-Host "✓ 所有相关分组已成功删除" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ 验证分组删除失败: $($_.Exception.Message)" -ForegroundColor Red
}

# 测试总结
Write-Host "`n=== 测试总结 ===" -ForegroundColor Green
Write-Host "测试完成！请检查以上结果确认功能是否正常工作。" -ForegroundColor Yellow
Write-Host "关键验证点：" -ForegroundColor Yellow
Write-Host "1. 部门删除后，相关分组是否被级联删除" -ForegroundColor Yellow
Write-Host "2. API返回的删除结果是否包含详细信息" -ForegroundColor Yellow
Write-Host "3. 数据库中的数据一致性是否正确" -ForegroundColor Yellow

Write-Host "`n请在前端界面验证：" -ForegroundColor Magenta
Write-Host "1. 部门管理页面是否正确显示剩余部门" -ForegroundColor Magenta
Write-Host "2. 部门分组页面是否已同步更新，不显示已删除部门的分组" -ForegroundColor Magenta
Write-Host "3. 删除确认对话框是否显示将要删除的分组信息" -ForegroundColor Magenta
