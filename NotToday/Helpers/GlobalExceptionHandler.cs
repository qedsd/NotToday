using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.IO;

namespace NotToday.Helpers
{
    public static class GlobalExceptionHandler
    {
        public static void Initialize()
        {
            // UI线程异常
            Application.Current.DispatcherUnhandledException +=
                OnDispatcherUnhandledException;

            // 非UI线程异常
            AppDomain.CurrentDomain.UnhandledException +=
                OnUnhandledException;

            // Task异常
            TaskScheduler.UnobservedTaskException +=
                OnUnobservedTaskException;
        }

        private static void OnDispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, "UI线程异常", out bool isHandled);
            e.Handled = isHandled;
        }

        private static void OnUnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception,
                "应用程序异常", out _);
        }

        private static void OnUnobservedTaskException(object sender,
            UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception, "Task异常", out _);
            e.SetObserved();
        }

        private static void HandleException(Exception ex,
            string source, out bool handled)
        {
            handled = false;

            if (ex == null) return;

            // 记录异常
            LogException(ex, source);

            // 根据异常类型处理
            switch (ex)
            {
                case BusinessException businessEx:
                    // 业务异常：显示友好提示
                    ShowUserFriendlyMessage(businessEx.Message);
                    handled = true;
                    break;

                case WarningException warningEx:
                    // 警告异常：不记录完整堆栈
                    ShowWarningMessage(warningEx.Message);
                    handled = true;
                    break;

                case CriticalException criticalEx:
                    // 严重异常：记录并尝试保存数据
                    HandleCriticalException(criticalEx);
                    handled = false; // 让应用程序崩溃
                    break;

                default:
                    // 未知异常：记录并显示通用错误
                    ShowErrorMessage(ex.Message);
                    handled = true;
                    break;
            }

            // 触发异常处理事件
            OnExceptionHandled?.Invoke(ex, source, handled);
        }

        // 自定义事件，允许其他地方订阅
        public static event Action<Exception, string, bool> OnExceptionHandled;

        private static void LogException(Exception ex, string source)
        {
            // 实现日志记录
            string logEntry = $@"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}来源：{source}异常：{ex}";

            // 写入文件或数据库
            File.AppendAllText("GlobalError.log", logEntry);
        }

        private static void ShowUserFriendlyMessage(string message)
        {
            MessageBox.Show(message, "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "警告",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void HandleCriticalException(CriticalException ex)
        {
            // 尝试保存用户数据
            SaveUserData();

            // 记录详细信息
            LogException(ex, "严重异常");

            // 显示错误并准备关闭
            MessageBox.Show($"系统遇到严重错误，即将关闭。\n{ex.Message}",
                "致命错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void SaveUserData()
        {
            // 实现数据保存逻辑
        }
    }

    // 自定义异常类型
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }

    public class WarningException : Exception
    {
        public WarningException(string message) : base(message) { }
    }

    public class CriticalException : Exception
    {
        public CriticalException(string message) : base(message) { }
    }
}
