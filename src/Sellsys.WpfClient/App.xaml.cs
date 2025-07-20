using System.Configuration;
using System.Data;
using System.Windows;
using Sellsys.WpfClient.Services;

namespace Sellsys.WpfClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("App.OnStartup: Starting application...");

            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            System.Diagnostics.Debug.WriteLine("App.OnStartup: Exception handlers set up");

            base.OnStartup(e);

            System.Diagnostics.Debug.WriteLine("App.OnStartup: Base startup completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App.OnStartup: Exception during startup: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"App.OnStartup: Stack trace: {ex.StackTrace}");
            MessageBox.Show($"应用程序启动失败: {ex.Message}\n\n{ex.StackTrace}", "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnUnhandledException: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"OnUnhandledException Stack: {ex.StackTrace}");
            ErrorHandlingService.HandleError(ex, "Unhandled AppDomain Exception");
            MessageBox.Show($"未处理的异常: {ex.Message}\n\n{ex.StackTrace}", "应用程序错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnDispatcherUnhandledException: {e.Exception.Message}");
        System.Diagnostics.Debug.WriteLine($"OnDispatcherUnhandledException Stack: {e.Exception.StackTrace}");
        ErrorHandlingService.HandleError(e.Exception, "Unhandled Dispatcher Exception");
        MessageBox.Show($"UI线程异常: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "UI错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Prevent the application from crashing
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ErrorHandlingService.HandleError(e.Exception, "Unobserved Task Exception");
        e.SetObserved(); // Prevent the application from crashing
    }
}

