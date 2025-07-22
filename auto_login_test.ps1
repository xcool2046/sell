# 自动化登录测试脚本
param(
    [int]$WaitSeconds = 30
)

Write-Host "=== 自动化登录测试脚本 ===" -ForegroundColor Green

# 加载必要的程序集
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

# 设置工作目录
$projectPath = "e:\download-\sell-master\sell-master"
Set-Location $projectPath

# 清理之前的进程
Write-Host "清理之前的进程..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -like "*Sellsys*" -or $_.ProcessName -like "*dotnet*" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# 编译项目
Write-Host "编译项目..." -ForegroundColor Yellow
$buildOutput = dotnet build src/Sellsys.WpfClient 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败!" -ForegroundColor Red
    Write-Host $buildOutput
    exit 1
}
Write-Host "编译成功!" -ForegroundColor Green

# 启动应用程序
Write-Host "启动应用程序..." -ForegroundColor Yellow
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Sellsys.WpfClient" -PassThru -WindowStyle Hidden

Write-Host "应用程序已启动，进程ID: $($process.Id)" -ForegroundColor Green

# 等待应用程序启动
Write-Host "等待应用程序启动..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

# 检查进程是否还在运行
$runningProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
if (-not $runningProcess) {
    Write-Host "应用程序启动失败或已崩溃" -ForegroundColor Red
    exit 1
}

Write-Host "应用程序正在运行，开始UI自动化测试..." -ForegroundColor Green

try {
    # 查找登录窗口
    Write-Host "查找登录窗口..." -ForegroundColor Yellow
    $automation = [System.Windows.Automation.AutomationElement]::RootElement
    
    # 等待登录窗口出现
    $loginWindow = $null
    for ($i = 0; $i -lt 10; $i++) {
        $windows = $automation.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($window in $windows) {
            if ($window.Current.Name -like "*登录*" -or $window.Current.Name -like "*销售管理系统*") {
                $loginWindow = $window
                break
            }
        }
        if ($loginWindow) { break }
        Start-Sleep -Seconds 1
    }
    
    if (-not $loginWindow) {
        Write-Host "未找到登录窗口" -ForegroundColor Red
        return
    }
    
    Write-Host "找到登录窗口: $($loginWindow.Current.Name)" -ForegroundColor Green
    
    # 查找用户名输入框
    Write-Host "查找用户名输入框..." -ForegroundColor Yellow
    $usernameBox = $loginWindow.FindFirst([System.Windows.Automation.TreeScope]::Descendants, 
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, 
        [System.Windows.Automation.ControlType]::Edit))
    
    if ($usernameBox) {
        Write-Host "找到用户名输入框，输入 'admin'..." -ForegroundColor Green
        $usernameBox.SetFocus()
        [System.Windows.Forms.SendKeys]::SendWait("admin")
        Start-Sleep -Seconds 1
    } else {
        Write-Host "未找到用户名输入框" -ForegroundColor Red
    }
    
    # 查找密码输入框
    Write-Host "查找密码输入框..." -ForegroundColor Yellow
    $passwordBoxes = $loginWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, 
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, 
        [System.Windows.Automation.ControlType]::Edit))
    
    if ($passwordBoxes.Count -gt 1) {
        $passwordBox = $passwordBoxes[1]  # 第二个输入框应该是密码框
        Write-Host "找到密码输入框，输入 'admin'..." -ForegroundColor Green
        $passwordBox.SetFocus()
        [System.Windows.Forms.SendKeys]::SendWait("admin")
        Start-Sleep -Seconds 1
    } else {
        Write-Host "未找到密码输入框" -ForegroundColor Red
    }
    
    # 查找登录按钮
    Write-Host "查找登录按钮..." -ForegroundColor Yellow
    $loginButton = $loginWindow.FindFirst([System.Windows.Automation.TreeScope]::Descendants, 
        [System.Windows.Automation.AndCondition]::new(
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button),
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, "登 录")
        ))
    
    if ($loginButton) {
        Write-Host "找到登录按钮，点击..." -ForegroundColor Green
        $invokePattern = $loginButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        $invokePattern.Invoke()
        Start-Sleep -Seconds 3
    } else {
        Write-Host "未找到登录按钮" -ForegroundColor Red
    }
    
    # 检查是否出现主窗口
    Write-Host "检查主窗口是否出现..." -ForegroundColor Yellow
    $mainWindow = $null
    for ($i = 0; $i -lt 10; $i++) {
        $windows = $automation.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($window in $windows) {
            if ($window.Current.Name -like "*客户管理*" -or $window.Current.Name -like "*巨炜科技*") {
                $mainWindow = $window
                break
            }
        }
        if ($mainWindow) { break }
        Start-Sleep -Seconds 1
    }
    
    if ($mainWindow) {
        Write-Host "✓ 登录成功！主窗口已出现: $($mainWindow.Current.Name)" -ForegroundColor Green
        Write-Host "测试通过！" -ForegroundColor Green
    } else {
        Write-Host "✗ 登录失败！未找到主窗口" -ForegroundColor Red
        
        # 检查是否还有登录窗口
        $stillLoginWindow = $automation.FindFirst([System.Windows.Automation.TreeScope]::Children, 
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, "*登录*"))
        
        if ($stillLoginWindow) {
            Write-Host "登录窗口仍然存在，可能登录失败" -ForegroundColor Red
        } else {
            Write-Host "登录窗口已消失，但主窗口未出现" -ForegroundColor Red
        }
    }
    
} catch {
    Write-Host "UI自动化测试出错: $($_.Exception.Message)" -ForegroundColor Red
}

# 等待一段时间观察
Write-Host "等待 $WaitSeconds 秒观察结果..." -ForegroundColor Yellow
Start-Sleep -Seconds $WaitSeconds

# 清理进程
Write-Host "清理进程..." -ForegroundColor Yellow
if (Get-Process -Id $process.Id -ErrorAction SilentlyContinue) {
    Stop-Process -Id $process.Id -Force
    Write-Host "进程已终止" -ForegroundColor Yellow
}

Write-Host "测试完成" -ForegroundColor Green
