using System;
using CadIME.Models;
using Action = CadIME.Models.Action;

namespace CadIME.Engine
{
    public class StateMachine
    {
        private State _currentState = State.Initializing;
        private readonly object _stateLock = new();

        public State CurrentState { get { lock (_stateLock) return _currentState; } }
        public void SetState(State state) { lock (_stateLock) _currentState = state; }

        public Action Transition(bool isTextCommand, Context context)
        {
            lock (_stateLock)
            {
                if (!context.IsCadWindowActive)
                {
                    if (_currentState != State.Suspended)
                    {
                        _currentState = State.Suspended;
                        return Action.Suspend;
                    }
                    return Action.None;
                }

                switch (_currentState)
                {
                    case State.Initializing:
                        _currentState = State.English;
                        return Action.SwitchToEnglish;

                    case State.Suspended:
                        if (context.Stage == CommandStage.WillStart && (isTextCommand || context.IsTextEditorActive))
                        {
                            _currentState = State.Chinese;
                            return Action.SwitchToChinese;
                        }
                        _currentState = State.English;
                        return Action.SwitchToEnglish;

                    case State.English:
                        // 卡死中文特定命令一票否决权，普通命令执行期绝对无法切中文
                        if (context.Stage == CommandStage.WillStart && (isTextCommand || context.IsModalDialogActive))
                        {
                            _currentState = State.Chinese;
                            return Action.SwitchToChinese;
                        }
                        return Action.None;

                    case State.Chinese:
                        // 100% 依赖合并后的 CommandChanged 信号执行单次快速退英文
                        if (context.Stage == CommandStage.CommandChanged && !context.IsTextEditorActive)
                        {
                            _currentState = State.English;
                            return Action.SwitchToEnglish;
                        }
                        return Action.None;

                    default:
                        return Action.None;
                }
            }
        }
    }
}
