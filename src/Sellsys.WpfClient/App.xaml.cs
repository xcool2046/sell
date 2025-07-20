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
        // Set up global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        base.OnStartup(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            ErrorHandlingService.HandleError(ex, "Unhandled AppDomain Exception");
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ErrorHandlingService.HandleError(e.Exception, "Unhandled Dispatcher Exception");
        e.Handled = true; // Prevent the application from crashing
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ErrorHandlingService.HandleError(e.Exception, "Unobserved Task Exception");
        e.SetObserved(); // Prevent the application from crashing
    }
}

