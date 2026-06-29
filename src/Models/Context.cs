using System;

namespace CadIME.Models
{
    public enum CommandStage
    {
        WillStart,     // 命令开始
        CommandChanged // 命令结束(Ended/Cancelled/Failed)
    }

    public class Context
    {
        public string CurrentCommand { get; init; } = string.Empty;
        public CommandStage Stage { get; init; } = CommandStage.CommandChanged;
        public IntPtr CurrentKeyboardLayout { get; init; } = IntPtr.Zero;
        public IntPtr DefaultChineseKeyboardLayout { get; init; } = IntPtr.Zero;
        public bool IsCadProcessActive { get; init; } = true;
        public bool IsCadWindowActive { get; init; } = true;
        public bool IsTextEditorActive { get; init; } = false;
        public bool IsModalDialogActive { get; init; } = false;
        public DateTime Timestamp { get; init; } = DateTime.Now;
    }
}
