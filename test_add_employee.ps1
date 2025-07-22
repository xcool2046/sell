# 测试添加员工API的PowerShell脚本

# API基础URL
$baseUrl = "http://localhost:5078/api"

# 测试数据
$employeeData = @{
    Name = "测试员工"
    LoginUsername = "testuser123"
    Phone = "13800138000"
    BranchAccount = ""
    GroupId = 1
    RoleId = 1
    Password = "abc1234"
} | ConvertTo-Json

Write-Host "测试添加员工API..." -ForegroundColor Green
Write-Host "发送的数据:" -ForegroundColor Yellow
Write-Host $employeeData

try {
    # 发送POST请求
    $response = Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeData -ContentType "application/json" -ErrorAction Stop
    
    Write-Host "请求成功!" -ForegroundColor Green
    Write-Host "响应数据:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 3
}
catch {
    Write-Host "请求失败!" -ForegroundColor Red
    Write-Host "错误信息:" -ForegroundColor Yellow
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "状态码: $statusCode" -ForegroundColor Red
        
        # 尝试读取错误响应内容
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorStream)
            $errorContent = $reader.ReadToEnd()
            Write-Host "错误详情:" -ForegroundColor Yellow
            Write-Host $errorContent
        }
        catch {
            Write-Host "无法读取错误详情" -ForegroundColor Red
        }
    }
    else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

Write-Host "`n测试完成" -ForegroundColor Green
