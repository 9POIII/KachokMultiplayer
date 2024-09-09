using System;
using System.Collections.Generic;
using Interfaces;

namespace Objects.Enemies.StateMachine
{
    public class StateMachine
    {
        private IState currentState;
        public IState CurrentState => currentState;

        private Dictionary<Type, IState> states = new Dictionary<Type, IState>();

        public void AddState(IState state)
        {
            var type = state.GetType();
            if (!states.ContainsKey(type))
            {
                states[type] = state;
            }
        }

        public void ChangeState<T>() where T : IState
        {
            var type = typeof(T);
            if (states.ContainsKey(type))
            {
                currentState?.Exit();
                currentState = states[type];
                currentState.Enter();
            }
        }

        public void Update()
        {
            currentState?.Execute();
        }
    }
}