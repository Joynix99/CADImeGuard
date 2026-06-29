namespace CadIME.Models
{
    /// <summary>
    /// 核心决策引擎输出的执行器指令枚举
    /// </summary>
    public enum Action
    {
        /// <summary>
        /// 无操作。状态未发生变化。
        /// </summary>
        None,

        /// <summary>
        /// 强制切换至英文输入状态（US 英文键盘布局）
        /// </summary>
        SwitchToEnglish,

        /// <summary>
        /// 强制切换至中文输入状态（操作系统默认中文布局）
        /// </summary>
        SwitchToChinese,

        /// <summary>
        /// 状态一致性同步。当检测到外部篡改时，强制拧回期望的输入法状态。
        /// </summary>
        Synchronize,

        /// <summary>
        /// 暂停输入法自动切换。当 AutoCAD 切换至后台时触发。
        /// </summary>
        Suspend,

        /// <summary>
        /// 恢复输入法自动切换。当 AutoCAD 重新回到前台时触发。
        /// </summary>
        Resume
    }
}
