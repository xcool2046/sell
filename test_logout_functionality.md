# 退出登录功能测试指南

## 修改内容总结

### 1. MainViewModel.cs 中的 Logout 方法
- **修改前**: 先关闭主窗口，再显示登录窗口
- **修改后**: 先创建并显示登录窗口，设置为主窗口，再关闭原主窗口

### 2. LoginWindow.xaml.cs 中的窗口关闭逻辑
- **添加**: `_isLoginSuccessful` 标志来区分登录成功和直接关闭
- **修改**: 只有在登录不成功时才退出应用程序
- **添加**: 监听 LoginViewModel 的登录成功事件

### 3. LoginViewModel.cs 中的登录成功处理
- **添加**: `LoginSuccessful` 事件
- **修改**: 在登录成功时触发事件，通知 LoginWindow

## 测试步骤

### 测试1: 正常登录流程
1. 启动应用程序
2. 使用 admin/admin 或 233/123456 登录
3. 验证主窗口正常显示
4. 验证用户权限正确

### 测试2: 退出登录功能
1. 在主窗口中点击"退出登录"按钮
2. 确认退出对话框，点击"是"
3. **预期结果**: 
   - 主窗口关闭
   - 登录窗口重新显示
   - 应用程序继续运行（不退出）

### 测试3: 登录窗口关闭行为
1. 在登录窗口点击右上角的"×"关闭按钮
2. **预期结果**: 应用程序完全退出

### 测试4: 重新登录
1. 完成测试2后，在重新显示的登录窗口中
2. 使用不同的用户账号登录
3. **预期结果**: 
   - 登录成功后显示主窗口
   - 用户信息和权限正确更新

## 关键修改点

### MainViewModel.cs - Logout方法
```csharp
private void Logout()
{
    try
    {
        var result = MessageBox.Show("确定要退出登录吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            // 清除当前用户
            CurrentUser.ClearUser();

            // 先创建并显示登录窗口
            var loginWindow = new Views.LoginWindow();
            
            // 设置登录窗口为应用程序主窗口
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            
            // 激活登录窗口
            loginWindow.Activate();
            loginWindow.Focus();

            // 然后关闭主窗口
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.Close();
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"登出过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

### LoginWindow.xaml.cs - 关闭逻辑
```csharp
public partial class LoginWindow : Window
{
    private bool _isLoginSuccessful = false;

    public LoginWindow()
    {
        InitializeComponent();
        var viewModel = new LoginViewModel();
        DataContext = viewModel;

        // 监听登录成功事件
        viewModel.LoginSuccessful += OnLoginSuccessful;
        
        // ... 其他初始化代码
    }

    private void OnLoginSuccessful()
    {
        _isLoginSuccessful = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        // 只有在登录不成功的情况下才退出应用程序
        if (!_isLoginSuccessful)
        {
            Application.Current.Shutdown();
        }
    }
}
```

## 预期行为

1. **用户登录成功**: 登录窗口关闭，主窗口显示，应用程序继续运行
2. **用户点击退出登录**: 主窗口关闭，登录窗口重新显示，应用程序继续运行
3. **用户关闭登录窗口**: 应用程序完全退出
4. **用户在重新显示的登录窗口中登录**: 正常进入主窗口

这样的设计确保了用户体验的连续性，用户可以在不重启应用程序的情况下切换不同的账号登录。
