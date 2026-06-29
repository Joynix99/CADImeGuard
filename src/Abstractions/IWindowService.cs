namespace CadIME.Abstractions
{
    /// <summary>
    /// 宿主窗口与操作系统前台焦点侦测契约
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// 检查当前处于操作系统最前台焦点的窗口是否属于当前 AutoCAD 进程
        /// </summary>
        bool IsAutoCADForeground();

        /// <summary>
        /// 检查当前 AutoCAD 内部是否有活动的内嵌或浮动文本编辑器（如单行/多行文字输入框）
        /// </summary>
        bool IsTextEditorActive();

        /// <summary>
        /// 检查当前 AutoCAD 内部是否弹出了阻塞交互的模态对话框（如选项、图层管理器等）
        /// </summary>
        bool IsModalDialogActive();
    }
}
