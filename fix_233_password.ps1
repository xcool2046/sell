# 通过API更新233用户密码
Write-Host "=== 通过API更新233用户密码 ===" -ForegroundColor Green

# 首先获取233用户的信息
Write-Host "获取233用户信息..." -ForegroundColor Yellow
try {
    $userResponse = Invoke-RestMethod -Uri "http://localhost:5078/api/employees/23" -Method GET
    if ($userResponse.isSuccess) {
        Write-Host "用户信息获取成功:" -ForegroundColor Green
        Write-Host "  ID: $($userResponse.data.id)"
        Write-Host "  姓名: $($userResponse.data.name)"
        Write-Host "  用户名: $($userResponse.data.loginUsername)"
        Write-Host "  角色ID: $($userResponse.data.roleId)"
        Write-Host "  分组ID: $($userResponse.data.groupId)"
        
        # 准备更新数据
        $updateData = @{
            Name = $userResponse.data.name
            LoginUsername = $userResponse.data.loginUsername
            Phone = $userResponse.data.phone
            BranchAccount = $userResponse.data.branchAccount
            GroupId = $userResponse.data.groupId
            RoleId = $userResponse.data.roleId
            Password = "123456"
        }
        
        $updateJson = $updateData | ConvertTo-Json
        
        Write-Host "更新用户密码..." -ForegroundColor Yellow
        Write-Host "发送的数据:" -ForegroundColor Cyan
        Write-Host $updateJson
        
        # 发送更新请求
        try {
            $updateResponse = Invoke-RestMethod -Uri "http://localhost:5078/api/employees/23" -Method PUT -Body $updateJson -ContentType "application/json"
            Write-Host "更新响应:" -ForegroundColor Cyan
            $updateResponse | ConvertTo-Json -Depth 3
        } catch {
            Write-Host "更新请求失败: $($_.Exception.Message)" -ForegroundColor Red
            if ($_.Exception.Response) {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                Write-Host "响应内容: $responseBody" -ForegroundColor Red
            }
            return
        }

        if ($updateResponse.isSuccess) {
            Write-Host "密码更新成功!" -ForegroundColor Green
            
            # 测试新密码登录
            Write-Host "测试新密码登录..." -ForegroundColor Yellow
            $loginData = @{
                Username = "233"
                Password = "123456"
            }
            
            $loginJson = $loginData | ConvertTo-Json
            
            $loginResponse = Invoke-RestMethod -Uri "http://localhost:5078/api/auth/login" -Method POST -Body $loginJson -ContentType "application/json"
            
            Write-Host "登录测试成功!" -ForegroundColor Green
            Write-Host "用户信息:" -ForegroundColor Cyan
            Write-Host "  ID: $($loginResponse.data.id)"
            Write-Host "  用户名: $($loginResponse.data.username)"
            Write-Host "  显示名: $($loginResponse.data.displayName)"
            Write-Host "  角色: $($loginResponse.data.roleName)"
            Write-Host "  权限模块: $($loginResponse.data.accessibleModules)"
            Write-Host "  是否管理员: $($loginResponse.data.isAdmin)"
        } else {
            Write-Host "密码更新失败: $($updateResponse.message)" -ForegroundColor Red
        }
    } else {
        Write-Host "获取用户信息失败: $($userResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "操作失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "=== 完成 ===" -ForegroundColor Green
