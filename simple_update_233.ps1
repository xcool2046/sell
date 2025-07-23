# 简单更新233用户密码
Write-Host "=== 更新233用户密码 ===" -ForegroundColor Green

# 准备更新数据
$updateData = @{
    Name = "erqe"
    LoginUsername = "233"
    Phone = "erq"
    BranchAccount = $null
    GroupId = 2
    RoleId = 5
    Password = "123456"
}

$updateJson = $updateData | ConvertTo-Json

Write-Host "发送更新请求..." -ForegroundColor Yellow
Write-Host "数据: $updateJson" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5078/api/employees/23" -Method PUT -Body $updateJson -ContentType "application/json" -ErrorAction Stop
    
    Write-Host "更新成功!" -ForegroundColor Green
    Write-Host "响应: $($response | ConvertTo-Json)" -ForegroundColor Cyan
    
    # 测试登录
    Write-Host "测试登录..." -ForegroundColor Yellow
    $loginData = @{
        Username = "233"
        Password = "123456"
    }
    $loginJson = $loginData | ConvertTo-Json
    
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5078/api/auth/login" -Method POST -Body $loginJson -ContentType "application/json"
    
    Write-Host "登录成功!" -ForegroundColor Green
    Write-Host "用户信息:" -ForegroundColor Cyan
    Write-Host "  ID: $($loginResponse.data.id)"
    Write-Host "  用户名: $($loginResponse.data.username)"
    Write-Host "  显示名: $($loginResponse.data.displayName)"
    Write-Host "  角色: $($loginResponse.data.roleName)"
    Write-Host "  权限模块: $($loginResponse.data.accessibleModules)"
    
} catch {
    Write-Host "操作失败: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "状态码: $statusCode" -ForegroundColor Red
    }
}

Write-Host "=== 完成 ===" -ForegroundColor Green
