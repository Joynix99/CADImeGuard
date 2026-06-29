using System;
using System.Collections.Generic;

namespace CadIME.Engine
{
    /// <summary>
    /// 高性能不区分大小写的 AutoCAD 例外命令高速检索器
    /// </summary>
    public class CommandMatcher
    {
        private readonly HashSet<string> _textCommands = new(StringComparer.Ordinal);
        private readonly object _lockObject = new();

        /// <summary>
        /// 全量动态刷新内存中的特殊命令集合
        /// </summary>
        /// <param name="commands">从配置文件中读取到的原始命令序列</param>
        public void UpdateCommands(IEnumerable<string>? commands)
        {
            if (commands == null) return;

            lock (_lockObject)
            {
                _textCommands.Clear();
                foreach (var cmd in commands)
                {
                    string normalized = NormalizeInternal(cmd);
                    if (!string.IsNullOrWhiteSpace(normalized))
                    {
                        _textCommands.Add(normalized);
                    }
                }
            }
        }

        /// <summary>
        /// 判定当前输入的 AutoCAD 原始命令是否属于预设的中文例外命令列表
        /// </summary>
        public bool IsTextCommand(string rawCommand)
        {
            if (string.IsNullOrWhiteSpace(rawCommand))
            {
                return false;
            }

            string normalized = NormalizeInternal(rawCommand);

            lock (_lockObject)
            {
                return _textCommands.Contains(normalized);
            }
        }

        /// <summary>
        /// Engine 内部私有命令规范器：移除国际化前缀(_)、原始命令前缀(.)和命令行模式前缀(-)，并强转大写
        /// 隔离平台依赖，确保 Engine 层的绝对绝对内聚。
        /// </summary>
        private static string NormalizeInternal(string rawCommand)
        {
            if (string.IsNullOrWhiteSpace(rawCommand))
            {
                return string.Empty;
            }

            string cleanCommand = rawCommand.Trim().ToUpperInvariant();

            bool hasPrefix = true;
            while (hasPrefix && cleanCommand.Length > 0)
            {
                char firstChar = cleanCommand[0];
                if (firstChar == '_' || firstChar == '.' || firstChar == '-')
                {
                    cleanCommand = cleanCommand.Substring(1);
                }
                else
                {
                    hasPrefix = false;
                }
            }

            return cleanCommand;
        }
    }
}
