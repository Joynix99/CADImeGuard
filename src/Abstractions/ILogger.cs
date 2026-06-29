using System;

namespace CadIME.Abstractions
{
    /// <summary>
    /// 全局日志契约抽象（极致精简版）
    /// </summary>
    public interface ILogger
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? exception = null);
        void LogFatal(string message, Exception? exception = null);
    }
}
