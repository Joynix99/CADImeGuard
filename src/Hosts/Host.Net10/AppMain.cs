using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CadIME.Abstractions;
using CadIME.Models;
using CadIME.Windows;
using Action = CadIME.Models.Action;

namespace CadIME.Hosts
{
    public class AppMain
    {
        private static SimpleLogger? _logger;
        private static CadIME.Engine.Engine? _engine;
        private static KeyboardLayoutService? _keyboardService;
        private static WindowService? _windowService;
        private static ActionExecutor? _executor;
        private static bool _isSelfFused = false; // 软件级自我熔断（禁用）标志位

        // 强行导入 Win32 弹窗 API，脱离对复杂 WPF 窗体组件的元数据依赖，100% 保证启动时弹窗成功
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        /// <summary>
        /// 返回当前软件是否已经处于自我熔断禁用状态
        /// </summary>
        public static bool IsDisabled => _isSelfFused;

        public static void OnInitialize()
        {
            try
            {
                // 1. 率先拉起日志系统（最高优先级）
                _logger = new SimpleLogger();
                _logger.LogInfo("[Host] 启动自检：安全日志系统率先唤醒。");

                // ==============================================================================
                // 【核心安全前置自检大闸】：检查操作系统是否存在 English (United States) 布局 (0x04090409)
                // ==============================================================================
                bool hasUsEnglish = false;
                IntPtr[] layouts = new IntPtr[32];
                int count = GetKeyboardLayoutList(layouts.Length, layouts);
                
                for (int i = 0; i < count; i++)
                {
                    if (layouts[i] == new IntPtr(0x04090409))
                    {
                        hasUsEnglish = true;
                        break;
                    }
                }

                if (!hasUsEnglish)
                {
                    // 定义唯一的持久化日志排查永久存根标记
                    string fuseKeyword = "CRITICAL_MISSING_0409_KEYBOARD";

                    // 扫描今天的历史日志，查验是否已经提醒过
                    bool hasWarnedBefore = _logger.HasLoggedMessage(fuseKeyword);

                    if (hasWarnedBefore)
                    {
                        // 【熔断分支 A】：日志发现已经提醒过 ➔ 绝不骚扰弹窗，直接自我封印注销退出
                        _isSelfFused = true;
                        _logger.LogWarning("[Host 自检熔断] 检测到当前系统缺失 0x04090409 键盘布局，且历史日志表明已完成弹窗提醒。软件启动自我熔断保护，直接静默退出。");
                        return; // 直接中止后续一切 brains 装配，软件彻底瘫痪不工作
                    }
                    else
                    {
                        // 【熔断分支 B】：首次发现缺失 ➔ 弹出标准的 Windows 官方高亮对话框，并记录日志标记
                        _logger.LogFatal($"[Host 自检拦截] [{fuseKeyword}] 无法正常运行！原因：Windows 系统中缺失 English (United States) 键盘布局包。");
                        
                        string caption = "CadIME Modern 环境缺失提示";
                        string noticeText = "检测到您的 Windows 系统中未安装或未启用 [英语(美国) - 美式键盘] (0x04090409) 键盘布局。\n\n" +
                                            "为了保障特定命令（TEXT/T）结束或按 ESC 取消时能够 100% 成功退回英文，本插件必须要依赖此标准布局。\n\n" +
                                            "【解决办法】：请在 Windows 的 [设置 -> 时间和语言 -> 语言和区域 -> 添加语言] 中，添加安装 [英语(美国)] 语言包并确保激活其键盘。\n\n" +
                                            "提示：本插件将不会重复弹窗打扰您，本次确认后软件将对当前环境自动禁用退出。";
                        
                        // 0x00000030: MB_ICONWARNING (感叹号标志) | MB_OK (单确定按钮)
                        MessageBox(IntPtr.Zero, noticeText, caption, 0x00000030 | 0x00000000);
                        
                        _isSelfFused = true;
                        return;
                    }
                }

                // ==============================================================================
                // 环境完全通过自检，开启正常满血运转
                // ==============================================================================
                JsonConfigurationProvider configProvider = new JsonConfigurationProvider(_logger);
                Configuration config = configProvider.Load();

                _logger.SetLevel(config.LogLevel);

                _keyboardService = new KeyboardLayoutService();
                _windowService = new WindowService();
                
                IntPtr chineseHkl = _keyboardService.GetDefaultChinese();
                if (chineseHkl == IntPtr.Zero)
                {
                    chineseHkl = _keyboardService.GetCurrent();
                }

                _engine = new CadIME.Engine.Engine();
                _engine.Initialize(config, _logger);

                _executor = new ActionExecutor(_keyboardService, chineseHkl, _logger);
                _executor.Execute(Action.SwitchToEnglish);
                
                _logger.LogInfo("[Host] 自检 100% 通过！全套生命周期矩阵平稳进入被动运行态。");
            }
            catch (Exception ex)
            {
                _logger?.LogFatal("[Host Initialize Critical] 宿主总调度装配线故障", ex);
            }
        }

        public static void DriveCoreLogic(string currentCommand, CommandStage stage)
        {
            // 如果软件已经处于自我熔断禁用状态，直接一票否决原路退出，彻底拒绝处理任何事件
            if (_isSelfFused) return;
            if (_engine == null || _logger == null || _windowService == null || _executor == null || _keyboardService == null) return;
            
            try
            {
                Context context = new Context
                {
                    CurrentCommand = currentCommand,
                    Stage = stage,
                    CurrentKeyboardLayout = _keyboardService.GetCurrent(),
                    DefaultChineseKeyboardLayout = _keyboardService.GetDefaultChinese(),
                    IsCadProcessActive = true,
                    IsCadWindowActive = _windowService.IsAutoCADForeground(),
                    IsTextEditorActive = _windowService.IsTextEditorActive(),
                    IsModalDialogActive = _windowService.IsModalDialogActive(),
                    Timestamp = DateTime.Now
                };

                Action nextAction = _engine.Process(context);

                if (nextAction != Action.None)
                {
                    _executor.Execute(nextAction);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("[Host Event Bridge] 事件中转链路异常", ex);
            }
        }

        public static void OnShutdown()
        {
            try
            {
                if (_isSelfFused) return;
                _engine?.Shutdown();
                _engine?.Dispose();
            }
            catch { }
        }
    }
}
