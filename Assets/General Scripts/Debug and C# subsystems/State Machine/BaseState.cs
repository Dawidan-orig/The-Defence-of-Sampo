using System;

//https://www.youtube.com/watch?v=qsIiFsddGV4

namespace Sampo.AI
{
    public abstract class BaseState<EState> where EState : Enum
    {
        protected BaseState(EState key)
        {
            StateKey = key;
        }

        public EState StateKey { get; private set; }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
        public abstract EState GetNextState();
    }
}