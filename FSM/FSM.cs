namespace StateMachine
{
    public sealed class FSM<StateType, FlagType>
        where StateType : Enum
        where FlagType : Enum
    {
        private const int UNNASSIGNED_TRANSITION = -1;
        public int currentState;
        private Dictionary<int, State> states;
        private Dictionary<int, Func<object[]>> behaviourEnterParams;
        private Dictionary<int, Func<object[]>> behaviourTickParams;
        private Dictionary<int, Func<object[]>> behaviourExitParams;

        private (int destinationState, Action onTransition)[,] transitions;

        ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

        private BehaviourActions GetCurrentEnterBehaviour =>
            states[currentState].GetOnEnterBehaviours(behaviourEnterParams[currentState]?.Invoke());

        private BehaviourActions GetCurrentTickBehaviour =>
            states[currentState].GetOnTickBehaviours(behaviourTickParams[currentState]?.Invoke());

        private BehaviourActions GetCurrentExitBehaviour =>
            states[currentState].GetOnExitBehaviours(behaviourExitParams[currentState]?.Invoke());

        private StateType defaultState;


        public FSM(StateType defaultState)
        {
            this.defaultState = defaultState;

            states = new Dictionary<int, State>();
            transitions = new (int, Action)[Enum.GetValues(typeof(StateType)).Length,
                Enum.GetValues(typeof(FlagType)).Length];
            for (int i = 0; i < transitions.GetLength(0); i++)
            {
                for (int j = 0; j < transitions.GetLength(1); j++)
                {
                    transitions[i, j] = (UNNASSIGNED_TRANSITION, null);
                }
            }

            behaviourEnterParams = new Dictionary<int, Func<object[]>>();
            behaviourTickParams = new Dictionary<int, Func<object[]>>();
            behaviourExitParams = new Dictionary<int, Func<object[]>>();
        }

        public void Init()
        {
            ForceState(defaultState);
        }

        public void AddState<TState>(StateType stateIndex,
            Func<object[]> onEnterParams = null,
            Func<object[]> onTickParams = null,
            Func<object[]> onExitParams = null)
            where TState : State, new()
        {
            if (!states.ContainsKey(Convert.ToInt32(stateIndex)))
            {
                TState state = new TState();

                ValidateParameters(state.OnEnterParametersTypes, onEnterParams);
                ValidateParameters(state.OnTickParametersTypes, onTickParams);
                ValidateParameters(state.OnExitParametersTypes, onExitParams);

                state.OnFlag += Transition;
                states.Add(Convert.ToInt32(stateIndex), state);
                behaviourEnterParams.Add(Convert.ToInt32(stateIndex), onEnterParams);
                behaviourTickParams.Add(Convert.ToInt32(stateIndex), onTickParams);
                behaviourExitParams.Add(Convert.ToInt32(stateIndex), onExitParams);
            }
        }

        public void ForceState(StateType state)
        {
            currentState = Convert.ToInt32(state);
            ExecuteBehaviour(GetCurrentEnterBehaviour);
        }

        public void SetTransition(StateType originalState, FlagType flag, StateType destinationState,
            Action onTransition = null)
        {
            transitions[Convert.ToInt32(originalState), Convert.ToInt32(flag)] =
                (Convert.ToInt32(destinationState), onTransition);
        }

        public void Transition(Enum flag)
        {
            if (states.ContainsKey(currentState))
                ExecuteBehaviour(GetCurrentExitBehaviour);

            if (transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState !=
                UNNASSIGNED_TRANSITION)
            {
                transitions[currentState, Convert.ToInt32(flag)].onTransition?.Invoke();
                currentState = transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState;
                ExecuteBehaviour(GetCurrentEnterBehaviour);
            }
        }

        public bool TryTransition(Enum flag)
        {
            if (transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState !=
                UNNASSIGNED_TRANSITION)
            {
                if (states.ContainsKey(currentState))
                    ExecuteBehaviour(GetCurrentExitBehaviour);

                transitions[currentState, Convert.ToInt32(flag)].onTransition?.Invoke();
                currentState = transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState;
                ExecuteBehaviour(GetCurrentEnterBehaviour);
                return true;
            }

            return false;
        }

        public void Tick()
        {
            if (states.ContainsKey(currentState))
            {
                ExecuteBehaviour(GetCurrentTickBehaviour);
            }
        }

        private void ExecuteBehaviour(BehaviourActions behaviourActions)
        {
            if (behaviourActions == null)
                return;

            int executionOrder = 0;

            while ((behaviourActions.MainThreadBehaviours != null && behaviourActions.MainThreadBehaviours.Count > 0) ||
                   (behaviourActions.MultiThreadBehaviours != null && behaviourActions.MultiThreadBehaviours.Count > 0))
            {
                Task multiThreadBehaviour = new Task(() =>
                {
                    if (behaviourActions.MultiThreadBehaviours != null)
                    {
                        if (behaviourActions.MultiThreadBehaviours.ContainsKey(executionOrder))
                        {
                            Parallel.ForEach(behaviourActions.MultiThreadBehaviours[executionOrder], parallelOptions,
                                (behaviour) => { behaviour?.Invoke(); });
                            behaviourActions.MultiThreadBehaviours.TryRemove(executionOrder, out _);
                        }
                    }
                });

                multiThreadBehaviour.Start();

                if (behaviourActions.MainThreadBehaviours != null)
                {
                    if (behaviourActions.MainThreadBehaviours.ContainsKey(executionOrder))
                    {
                        foreach (Action behaviourAction in behaviourActions.MainThreadBehaviours[executionOrder])
                        {
                            behaviourAction?.Invoke();
                        }

                        behaviourActions.MainThreadBehaviours.Remove(executionOrder);
                    }
                }

                multiThreadBehaviour?.Wait();
                executionOrder++;

                behaviourActions.TransitionBehaviour?.Invoke();
            }
        }

        public void ValidateParameters(Type[] expectedParams, Func<object[]> receivedParams)
        {
            if (expectedParams.Length == 0 && receivedParams == null)
                return;

            object[] parameters = receivedParams.Invoke();

            List<Type> receivedParametersTypes = new List<Type>();
            foreach (object parameter in parameters)
            {
                receivedParametersTypes.Add(parameter.GetType());
            }

            if (expectedParams.Length != receivedParametersTypes.Count)
            {
                throw new ArgumentException("Number Of parameters received Different from expected");
            }

            for (int i = 0; i < expectedParams.Length; i++)
            {
                if (!expectedParams[i].IsAssignableFrom(receivedParametersTypes[i]))
                    throw new InvalidCastException(
                        $"Type {receivedParametersTypes[i].Name} cannot be assigned to {expectedParams[i].Name}");
            }
        }
    }
}