# 测试admin用户登录
Write-Host "=== 测试admin用户登录 ===" -ForegroundColor Green

# 测试admin登录
Write-Host "测试admin登录:" -ForegroundColor Yellow
$loginData = @{
    Username = "admin"
    Password = "admin"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5078/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "admin登录成功!" -ForegroundColor Green
    Write-Host "用户信息:" -ForegroundColor Cyan
    Write-Host "  ID: $($response.data.id)"
    Write-Host "  用户名: $($response.data.username)"
    Write-Host "  显示名: $($response.data.displayName)"
    Write-Host "  角色: $($response.data.roleName)"
    Write-Host "  权限模块: $($response.data.accessibleModules)"
    Write-Host "  是否管理员: $($response.data.isAdmin)"
} catch {
    Write-Host "admin登录失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== 完成 ===" -ForegroundColor Green
