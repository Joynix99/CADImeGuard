using System;
using System.IO;
using System.Collections.Generic;
using CadIME.Abstractions;
using CadIME.Models;
using CadIME.Common;

namespace CadIME.Hosts
{
    public class JsonConfigurationProvider : IConfigurationProvider<Configuration>
    {
        private readonly ILogger _logger;
        private readonly string _configPath;

        public JsonConfigurationProvider(ILogger logger)
        {
            _logger = logger;
            _configPath = PathHelper.GetConfigPath();
        }

        public Configuration Load()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    _logger.LogWarning($"配置文件不存在，将自动创建默认配置: {_configPath}");
                    Configuration defaultConfig = new Configuration();
                    Save(defaultConfig);
                    return defaultConfig;
                }

                string content = File.ReadAllText(_configPath);
                List<string> commands = new List<string>();
                if (content.Contains("[") && content.Contains("]"))
                {
                    int start = content.IndexOf("[") + 1;
                    int end = content.IndexOf("]");
                    string arrayContent = content.Substring(start, end - start);
                    string[] items = arrayContent.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in items)
                    {
                        string cleanItem = item.Replace("\"", "").Replace("'", "").Trim();
                        if (!string.IsNullOrWhiteSpace(cleanItem))
                        {
                            commands.Add(cleanItem);
                        }
                    }
                }

                if (commands.Count == 0) return new Configuration();
                return new Configuration { Commands = commands };
            }
            catch (Exception ex)
            {
                _logger.LogError("读取或解析配置文件时崩溃，启用内存硬编码降级方案", ex);
                return new Configuration();
            }
        }

        public void Save(Configuration config)
        {
            try
            {
                string json = "{" + Environment.NewLine +
                              $"  \"Version\": \"{config.Version}\"," + Environment.NewLine +
                              $"  \"LogLevel\": \"{config.LogLevel}\"," + Environment.NewLine +
                              "  \"ChineseCommands\": [" + Environment.NewLine;
                
                for (int i = 0; i < config.Commands.Count; i++)
                {
                    json += $"    \"{config.Commands[i]}\"{(i == config.Commands.Count - 1 ? "" : ",")}{Environment.NewLine}";
                }
                json += "  ]" + Environment.NewLine + "}";

                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError("持久化保存配置文件时发生故障", ex);
            }
        }
    }
}
