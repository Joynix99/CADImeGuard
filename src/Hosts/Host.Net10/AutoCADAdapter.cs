using System;
using System.IO;
using System.Diagnostics;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using CadIME.Models;
using CadIME.Hosts;
using CadIME.Windows;

[assembly: ExtensionApplication(typeof(CadIME.Host.Net10.AutoCADAdapter))]

namespace CadIME.Host.Net10
{
    /// <summary>
    /// 纯被动事件聚合与硬件级主线程安全封送型入口适配器（完全消除静态引用隐患）
    /// </summary>
    public class AutoCADAdapter : IExtensionApplication
    {
        private static IntPtr _winEventHookHandle = IntPtr.Zero;
        private static NativeMethods.WinEventProc? _winEventProcDelegate;
        private static uint _currentPid;
        private static readonly IntPtr _englishHkl = new IntPtr(0x04090409);
        private static bool _isHookFiredSignal = false; 

        public void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                string systemTemp = Environment.GetEnvironmentVariable("TEMP") ?? "C:\\Temp";
                File.WriteAllText(Path.Combine(systemTemp, "CadIME_Crash.txt"), e.ExceptionObject.ToString());
            };

            try
            {
                AppMain.OnInitialize();
                var docManager = Application.DocumentManager;

                if (AppMain.IsDisabled)
                {
                    if (docManager.MdiActiveDocument != null)
                    {
                        docManager.MdiActiveDocument.Editor.WriteMessage("\n>>> [CadIME 警告] 本机缺失 0409 键盘，插件已自我熔断静默退出。\n");
                    }
                    return;
                }

                // 全面转向 DocumentManager 集合总线托管，100% 根除重复挂载
                docManager.DocumentCreated += OnDocumentCreated;
                docManager.DocumentToBeDestroyed += OnDocumentToBeDestroyed;

                // 遍历当前内存中已经建立出来的所有活体图纸，执行唯一性绑定
                foreach (Document doc in docManager)
                {
                    WeldDocumentEvents(doc);
                }

                // 注册硬件级焦点监听钩子
                _currentPid = (uint)Process.GetCurrentProcess().Id;
                _winEventProcDelegate = new NativeMethods.WinEventProc(OnWindowFocusChangedHook);

                _winEventHookHandle = NativeMethods.SetWinEventHook(
                    NativeMethods.EVENT_OBJECT_FOCUS,
                    NativeMethods.EVENT_OBJECT_FOCUS,
                    IntPtr.Zero,
                    _winEventProcDelegate,
                    _currentPid,
                    0,
                    NativeMethods.WINEVENT_OUTOFCONTEXT
                );

                if (docManager.MdiActiveDocument != null)
                {
                    docManager.MdiActiveDocument.Editor.WriteMessage("\n>>> [CadIME Modern 2027] 全网深度消杀重组！总线安全焊接网已满血就位！\n");
                }
            }
            catch (System.Exception ex)
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                doc?.Editor.WriteMessage($"\n[CadIME 错误提示] 入口大崩溃: {ex.Message}\n");
            }
        }

        public void Terminate()
        {
            try
            {
                if (_winEventHookHandle != IntPtr.Zero)
                {
                    NativeMethods.UnhookWinEvent(_winEventHookHandle);
                    _winEventHookHandle = IntPtr.Zero;
                }

                var docManager = Application.DocumentManager;
                docManager.DocumentCreated -= OnDocumentCreated;
                docManager.DocumentToBeDestroyed -= OnDocumentToBeDestroyed;

                // 遍历内存中所有的打开图纸，全量清洗，彻底剿灭野指针闪退
                foreach (Document doc in docManager)
                {
                    UnWeldDocumentEvents(doc);
                }

                Application.Idle -= OnMainThreadSafeSignalExecutor;
                AppMain.OnShutdown();
            }
            catch { }
        }

        // 【核心修正】：升级为静态处理函数，对齐总线要求
        private static void WeldDocumentEvents(Document doc)
        {
            if (doc == null) return;
            doc.CommandWillStart -= OnCommandWillStart;
            doc.CommandEnded -= OnCommandChangedNotification;
            doc.CommandCancelled -= OnCommandChangedNotification;
            doc.CommandFailed -= OnCommandChangedNotification;

            doc.CommandWillStart += OnCommandWillStart;
            doc.CommandEnded += OnCommandChangedNotification;
            doc.CommandCancelled += OnCommandChangedNotification;
            doc.CommandFailed += OnCommandChangedNotification;
        }

        private static void UnWeldDocumentEvents(Document doc)
        {
            if (doc == null) return;
            doc.CommandWillStart -= OnCommandWillStart;
            doc.CommandEnded -= OnCommandChangedNotification;
            doc.CommandCancelled -= OnCommandChangedNotification;
            doc.CommandFailed -= OnCommandChangedNotification;
        }

        private static void OnDocumentCreated(object? sender, DocumentCollectionEventArgs e) => WeldDocumentEvents(e.Document);
        private static void OnDocumentToBeDestroyed(object? sender, DocumentCollectionEventArgs e) => UnWeldDocumentEvents(e.Document);

        // 【核心修正】：事件回调全部升级为静态方法，一枪打穿 CS0120 报错
        private static void OnCommandWillStart(object? sender, CommandEventArgs e)
        {
            if (AppMain.IsDisabled) return;
            AppMain.DriveCoreLogic(CommandClean(e.GlobalCommandName), CommandStage.WillStart);
        }

        private static void OnCommandChangedNotification(object? sender, CommandEventArgs e)
        {
            if (AppMain.IsDisabled) return;
            AppMain.DriveCoreLogic(CommandClean(e.GlobalCommandName), CommandStage.CommandChanged);
        }

        private static void OnWindowFocusChangedHook(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (AppMain.IsDisabled) return;
            if (_isHookFiredSignal) return; 
            
            _isHookFiredSignal = true;
            Application.Idle -= OnMainThreadSafeSignalExecutor;
            Application.Idle += OnMainThreadSafeSignalExecutor; 
        }

        private static void OnMainThreadSafeSignalExecutor(object? sender, EventArgs e)
        {
            Application.Idle -= OnMainThreadSafeSignalExecutor; 
            _isHookFiredSignal = false;

            if (AppMain.IsDisabled) return;
            try
            {
                object cmdNamesObj = Application.GetSystemVariable("CMDNAMES");
                string cmdNames = cmdNamesObj != null ? cmdNamesObj.ToString()!.Trim() : string.Empty;

                if (string.IsNullOrWhiteSpace(cmdNames))
                {
                    IntPtr targetW = NativeMethods.GetForegroundWindow();
                    if (targetW != IntPtr.Zero)
                    {
                        IntPtr currentHkl = NativeMethods.GetKeyboardLayout(0);
                        if (currentHkl != _englishHkl)
                        {
                            NativeMethods.PostMessage(targetW, NativeMethods.WM_INPUTLANGCHANGEREQUEST, (IntPtr)NativeMethods.KLF_ACTIVATE, _englishHkl);
                        }
                    }
                    AppMain.DriveCoreLogic(string.Empty, CommandStage.CommandChanged);
                }
            }
            catch { }
        }

        private static string CommandClean(string rawCommand)
        {
            if (string.IsNullOrWhiteSpace(rawCommand)) return string.Empty;
            string clean = rawCommand.Trim().ToUpperInvariant();
            bool hasPrefix = true;
            while (hasPrefix && clean.Length > 0)
            {
                if (clean.StartsWith("_") || clean.StartsWith(".") || clean.StartsWith("-"))
                {
                    clean = clean.Substring(1);
                }
                else
                {
                    hasPrefix = false;
                }
            }
            return clean;
        }
    }
}
