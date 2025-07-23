# 测试233用户登录的脚本
Write-Host "=== 测试233用户登录和权限 ===" -ForegroundColor Green

# 检查数据库中233用户的信息
Write-Host "`n1. 检查233用户的基本信息:" -ForegroundColor Yellow
sqlite3 src\Sellsys.WebApi\sellsys.db "SELECT Id, Name, LoginUsername, RoleId, GroupId FROM Employees WHERE LoginUsername = '233';"

# 检查233用户的角色权限
Write-Host "`n2. 检查233用户的角色权限:" -ForegroundColor Yellow
sqlite3 src\Sellsys.WebApi\sellsys.db "SELECT e.Id, e.Name, e.LoginUsername, r.Name as RoleName, r.AccessibleModules FROM Employees e LEFT JOIN Roles r ON e.RoleId = r.Id WHERE e.LoginUsername = '233';"

# 测试登录API
Write-Host "`n3. 测试登录API:" -ForegroundColor Yellow

# 尝试不同的密码
$passwords = @("233", "123456", "password", "123", "erqe")

foreach ($password in $passwords) {
    Write-Host "`n尝试密码: $password" -ForegroundColor Cyan
    $loginData = @{
        Username = "233"
        Password = $password
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5078/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
        Write-Host "登录成功!" -ForegroundColor Green
        Write-Host "用户信息:" -ForegroundColor Cyan
        Write-Host "  ID: $($response.data.id)"
        Write-Host "  用户名: $($response.data.username)"
        Write-Host "  显示名: $($response.data.displayName)"
        Write-Host "  角色: $($response.data.roleName)"
        Write-Host "  权限模块: $($response.data.accessibleModules)"
        Write-Host "  是否管理员: $($response.data.isAdmin)"
        break
    } catch {
        Write-Host "登录失败: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== 测试完成 ===" -ForegroundColor Green
