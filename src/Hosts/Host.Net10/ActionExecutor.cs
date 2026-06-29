using System;
using CadIME.Abstractions;
using Action = CadIME.Models.Action;

namespace CadIME.Hosts
{
    public class ActionExecutor
    {
        private readonly IKeyboardLayoutService _keyboardService;
        private readonly ILogger _logger;
        private readonly IntPtr _englishHkl = new IntPtr(0x04090409); // 经前置自检卡死，100% 存在

        public ActionExecutor(IKeyboardLayoutService keyboardService, IntPtr defaultChineseHkl, ILogger logger)
        {
            _keyboardService = keyboardService;
            _logger = logger;
        }

        public void Execute(Action action)
        {
            try
            {
                IntPtr currentActualHkl = _keyboardService.GetCurrent();
                bool isCurrentlyChinese = (((uint)currentActualHkl & 0x0000FFFF) == 0x0804);

                switch (action)
                {
                    case Action.None:
                        break;

                    case Action.SwitchToEnglish:
                        // 彻底取消一切英文动作拦截，100% 强行灌入消息强刷
                        _keyboardService.SwitchTo(_englishHkl);
                        break;

                    case Action.SwitchToChinese:
                        // 低 16 位为 0x0804 通通全承认！顺应用户手动切换的搜狗/拼音/五笔，绝不重复强刷
                        if (isCurrentlyChinese) return;
                        _keyboardService.SwitchTo(_keyboardService.GetDefaultChinese());
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Executor] 动作 {action} 遭遇执行灾难", ex);
            }
        }
    }
}
