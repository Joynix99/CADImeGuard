using System;

namespace CadIME.Abstractions
{
    /// <summary>
    /// 操作系统键盘布局与输入法切换契约
    /// </summary>
    public interface IKeyboardLayoutService
    {
        /// <summary>
        /// 获取当前线程/前台窗口正在激活使用的键盘布局句柄 (HKL)
        /// </summary>
        IntPtr GetCurrent();

        /// <summary>
        /// 扫描操作系统已安装的语言列表，获取首个可用的中文输入法布局句柄
        /// </summary>
        /// <returns>若无可用中文布局，必须返回 IntPtr.Zero 以便外层执行安全降级</returns>
        IntPtr GetDefaultChinese();

        /// <summary>
        /// 激活并切换至指定的键盘布局
        /// </summary>
        /// <param name="keyboardLayoutHandle">目标键盘布局的原生 HKL 句柄</param>
        /// <returns>切换是否成功</returns>
        bool SwitchTo(IntPtr keyboardLayoutHandle);
    }
}
