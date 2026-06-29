using System;
using System.IO;
using CadIME.Abstractions;
using CadIME.Common;

namespace CadIME.Hosts
{
    /// <summary>
    /// 系统标准化被动日志服务（100% 深度消杀版）
    /// </summary>
    public class SimpleLogger : ILogger
    {
        private readonly string _logFilePath;
        private string _currentLevel = "Information";
        private readonly object _logLock = new();

        public SimpleLogger()
        {
            // 【终极消杀】：直接动态获取系统的 TEMP 安全盘区路径，彻底剥离对 PathHelper.GetLogDirectory 的过时调用
            string logDir = Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath();
            
            // 确保全局 TEMP 临时隔离区常驻可用
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            
            _logFilePath = PathHelper.GetLogFilePath();
        }

        /// <summary>
        /// 高速雷达日志扫描器。用于在启动时检查历史日志中是否已经记录过特定提醒。
        /// </summary>
        public bool HasLoggedMessage(string keyword)
        {
            lock (_logLock)
            {
                try
                {
                    if (!File.Exists(_logFilePath)) return false;
                    
                    using (var reader = new StreamReader(_logFilePath))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains(keyword)) return true;
                        }
                    }
                }
                catch
                {
                    // 静默保护
                }
                return false;
            }
        }

        public void SetLevel(string level)
        {
            if (!string.IsNullOrWhiteSpace(level)) _currentLevel = level.Trim();
        }

        public void Log(string level, string message)
        {
            lock (_logLock)
            {
                try
                {
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logLine);
                }
                catch { }
            }
        }

        public void LogDebug(string message) { if (_currentLevel == "Debug") Log("DEBUG", message); }
        public void LogInfo(string message) { Log("INFO", message); }
        public void LogWarning(string message) { Log("WARN", message); }
        public void LogError(string message, Exception? exception = null) {
            string exMsg = exception != null ? $"{message} | 异常: {exception.Message}{Environment.NewLine}{exception.StackTrace}" : message;
            Log("ERROR", exMsg);
        }
        public void LogFatal(string message, Exception? exception = null) {
            string exMsg = exception != null ? $"{message} | 致命: {exception.Message}{Environment.NewLine}{exception.StackTrace}" : message;
            Log("FATAL", exMsg);
        }
    }
}
