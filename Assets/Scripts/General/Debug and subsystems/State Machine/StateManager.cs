using System;
using System.Collections.Generic;
using UnityEngine;
//https://www.youtube.com/watch?v=qsIiFsddGV4

namespace Sampo.AI
{
    public class StateManager<EState> : MonoBehaviour where EState : Enum
    {
        protected Dictionary<EState, BaseState<EState>> states = new();

        protected BaseState<EState> currentState;

        protected bool IsTransitioningState = false;

        void Start()
        {
            currentState.EnterState();
        }
        void Update()
        {
            EState nextStateKey = currentState.GetNextState();
            if (!IsTransitioningState && nextStateKey.Equals(currentState))
            {
                currentState.UpdateState();
            }
            else if(!IsTransitioningState)
                TransitionToState(nextStateKey);

            currentState.UpdateState();
        }

        private void FixedUpdate()
        {
            currentState.FixedUpdateState();
        }

        void TransitionToState(EState key) 
        {
            IsTransitioningState = true;
            currentState.ExitState();
            currentState = states[key];
            currentState.EnterState();
            IsTransitioningState = false;
        }
    }
}