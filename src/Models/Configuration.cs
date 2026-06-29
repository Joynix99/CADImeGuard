using System.Collections.Generic;

namespace CadIME.Models
{
    /// <summary>
    /// 全局配置数据契约实体（包含损坏回退防御机制）
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// 配置方案版本号
        /// </summary>
        public string Version { get; init; } = "1.0";

        /// <summary>
        /// 全局日志输出拦截阈值等级
        /// </summary>
        public string LogLevel { get; init; } = "Information";

        /// <summary>
        /// 需要强制切换为中文输入法的 AutoCAD 特殊命令列表
        /// </summary>
        public List<string> Commands { get; init; }

        /// <summary>
        /// 构造函数：初始化实例并提供硬件级硬编码回退保障
        /// </summary>
        public Configuration()
        {
            // 防御性设计：确保该列表在任何极端情况下都不会为 null
            Commands = new List<string>
            {
                "TEXT",
                "MTEXT",
                "DTEXT",
                "DDEDIT",
                "ATTEDIT",
                "MTEDIT"
            };
        }
    }
}
