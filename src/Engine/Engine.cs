using System;
using CadIME.Abstractions;
using CadIME.Models;
using Action = CadIME.Models.Action;

namespace CadIME.Engine
{
    /// <summary>
    /// CadIME 核心控制引擎（实现 IEngine 契约的主控制中枢）
    /// </summary>
    public class Engine : IEngine<Context, Action, Configuration>
    {
        private readonly StateMachine _stateMachine = new();
        private readonly CommandMatcher _commandMatcher = new();
        private ILogger? _logger;
        private bool _isDisposed;

        /// <summary>
        /// 全局初始化配置与日志装配行为
        /// </summary>
        public void Initialize(Configuration configuration, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // 将例外命令刷入匹配池
            _commandMatcher.UpdateCommands(configuration.Commands);

            // 将状态机设定为初始英文预备态
            _stateMachine.SetState(State.Initializing);
            
            _logger.LogInfo("[Engine] 核心策略状态机引擎被动防护模式初始化成功。");
        }

        /// <summary>
        /// 驱动业务大脑：接收 Host 数据快照，根据内在策略返回 Action 执行命令
        /// </summary>
        public Action Process(Context context)
        {
            if (_isDisposed) return Action.None;

            try
            {
                if (context == null)
                {
                    _logger?.LogWarning("[Engine Process] 接收到了空的 Context 上下文快照，拒绝进行决策。");
                    return Action.None;
                }

                // 1. 判定当前命令是否属于中文白名单命令
                bool isTextCommand = _commandMatcher.IsTextCommand(context.CurrentCommand);

                // 2. 驱动有限状态机进行被动状态流转决策
                Action decisionAction = _stateMachine.Transition(isTextCommand, context);

                return decisionAction;
            }
            catch (Exception ex)
            {
                _logger?.LogError("[Engine Process] 业务大脑状态机运转中发生了不可预知的捕获异常", ex);
                return Action.None;
            }
        }

        public void Shutdown()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _logger?.LogInfo("[Engine] 业务大脑核心引擎成功关闭。");
        }

        public void Dispose()
        {
            Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
