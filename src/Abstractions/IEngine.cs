using System;

namespace CadIME.Abstractions
{
    /// <summary>
    /// 状态机与核心策略引擎驱动契约
    /// </summary>
    /// <typeparam name="TContext">只读上下文快照类型</typeparam>
    /// <typeparam name="TAction">输出决策动作枚举类型</typeparam>
    /// <typeparam name="TConfig">注入的配置类型</typeparam>
    public interface IEngine<TContext, TAction, TConfig> : IDisposable
    {
        /// <summary>
        /// 初始化引擎。加载命令列表并锁定初始状态。
        /// </summary>
        void Initialize(TConfig configuration, ILogger logger);

        /// <summary>
        /// 驱动状态机核心。根据 Host 拍入的最新快照，决策并输出执行指令。
        /// </summary>
        /// <param name="context">当前的只读环境状态快照</param>
        /// <returns>返回具体的动作指令，内部发生任何异常必须捕获并安全返回 None</returns>
        TAction Process(TContext context);

        /// <summary>
        /// 释放引擎内部持有的策略资源
        /// </summary>
        void Shutdown();
    }
}
