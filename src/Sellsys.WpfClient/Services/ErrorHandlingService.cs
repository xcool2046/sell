using System;
using System.Windows;

namespace Sellsys.WpfClient.Services
{
    public static class ErrorHandlingService
    {
        public static void HandleError(Exception ex, string context = "")
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error in {context}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            // For now, just log to debug output
            // In a production app, you might want to:
            // - Log to a file
            // - Send to a logging service
            // - Show a non-blocking notification
            
            // Avoid showing MessageBox during navigation as it can cause crashes
        }

        public static void HandleApiError(Exception ex, string operation = "")
        {
            string message = ex.Message;
            
            // Categorize common API errors
            if (ex.Message.Contains("网络请求失败") || ex.Message.Contains("HttpRequestException"))
            {
                message = "无法连接到服务器，请检查网络连接或确认服务器已启动";
            }
            else if (ex.Message.Contains("数据解析失败") || ex.Message.Contains("JsonException"))
            {
                message = "服务器返回的数据格式错误";
            }

            System.Diagnostics.Debug.WriteLine($"API Error during {operation}: {message}");
            
            // In a real application, you might want to show a non-blocking notification
            // or update a status bar instead of using MessageBox
        }

        public static void ShowUserFriendlyError(string message, string title = "错误")
        {
            // Only show MessageBox when it's safe to do so (not during navigation)
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use BeginInvoke to avoid blocking the UI thread
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
            else
            {
                // If we're not on the UI thread, just log the error
                System.Diagnostics.Debug.WriteLine($"{title}: {message}");
            }
        }
    }
}
