# 测试记住账号和密码功能
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Write-Host "开始测试记住账号和密码功能..." -ForegroundColor Green

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
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit))
    
    if ($usernameBox) {
        Write-Host "找到用户名输入框，输入测试账号..." -ForegroundColor Green
        $valuePattern = $usernameBox.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $valuePattern.SetValue("testuser")
        Start-Sleep -Seconds 1
    } else {
        Write-Host "未找到用户名输入框" -ForegroundColor Red
        return
    }
    
    # 查找密码输入框
    Write-Host "查找密码输入框..." -ForegroundColor Yellow
    $passwordBox = $loginWindow.FindFirst([System.Windows.Automation.TreeScope]::Descendants, 
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Document))
    
    if ($passwordBox) {
        Write-Host "找到密码输入框，输入测试密码..." -ForegroundColor Green
        $valuePattern = $passwordBox.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $valuePattern.SetValue("testpass123")
        Start-Sleep -Seconds 1
    } else {
        Write-Host "未找到密码输入框" -ForegroundColor Red
        return
    }
    
    # 查找"记住账号和密码"复选框
    Write-Host "查找记住账号和密码复选框..." -ForegroundColor Yellow
    $checkboxes = $loginWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, 
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::CheckBox))
    
    $rememberCheckbox = $null
    foreach ($checkbox in $checkboxes) {
        if ($checkbox.Current.Name -like "*记住*") {
            $rememberCheckbox = $checkbox
            break
        }
    }
    
    if ($rememberCheckbox) {
        Write-Host "找到记住账号和密码复选框: $($rememberCheckbox.Current.Name)" -ForegroundColor Green
        
        # 检查复选框是否已选中
        $togglePattern = $rememberCheckbox.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        $currentState = $togglePattern.ToggleState
        
        Write-Host "当前复选框状态: $currentState" -ForegroundColor Cyan
        
        # 如果未选中，则选中它
        if ($currentState -eq [System.Windows.Automation.ToggleState]::Off) {
            Write-Host "选中记住账号和密码复选框..." -ForegroundColor Green
            $togglePattern.Toggle()
            Start-Sleep -Seconds 1
        }
    } else {
        Write-Host "未找到记住账号和密码复选框" -ForegroundColor Red
        return
    }

    Write-Host "测试完成！请手动验证以下功能：" -ForegroundColor Green
    Write-Host "1. 复选框文本是否显示为'记住账号和密码'" -ForegroundColor Yellow
    Write-Host "2. 选中复选框后，下次启动应用时是否自动填充账号和密码" -ForegroundColor Yellow
    Write-Host "3. 取消选中复选框后，下次启动应用时是否不填充账号和密码" -ForegroundColor Yellow

} catch {
    Write-Host "测试过程中发生错误: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "堆栈跟踪: $($_.ScriptStackTrace)" -ForegroundColor Red
}

Write-Host "测试脚本执行完成" -ForegroundColor Green
