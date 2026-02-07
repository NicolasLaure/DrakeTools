using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DrakeToolbox.Pool;

namespace DrakeToolbox.StateMachine
{
    public sealed class BehaviourActions : IResettable
    {
        private Dictionary<int, List<Action>> mainThreadBehaviours;
        private ConcurrentDictionary<int, ConcurrentBag<Action>> multiThreadBehaviours;
        private Action transitionBehaviour;

        public Dictionary<int, List<Action>> MainThreadBehaviours => mainThreadBehaviours;
        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadBehaviours => multiThreadBehaviours;
        public Action TransitionBehaviour => transitionBehaviour;

        public void AddMainThreadBehaviour(int executionOrder, Action behaviour)
        {
            if (mainThreadBehaviours == null)
                mainThreadBehaviours = new Dictionary<int, List<Action>>();

            if (!mainThreadBehaviours.ContainsKey(executionOrder))
                mainThreadBehaviours.Add(executionOrder, new List<Action>());

            mainThreadBehaviours[executionOrder].Add(behaviour);
        }

        public void AddMultiThreadBehaviour(int executionOrder, Action behaviour)
        {
            if (multiThreadBehaviours == null)
                multiThreadBehaviours = new ConcurrentDictionary<int, ConcurrentBag<Action>>();

            if (!multiThreadBehaviours.ContainsKey(executionOrder))
                multiThreadBehaviours.TryAdd(executionOrder, new ConcurrentBag<Action>());

            multiThreadBehaviours[executionOrder].Add(behaviour);
        }

        public void SetTransitionBehaviour(Action behaviour)
        {
            this.transitionBehaviour = behaviour;
        }

        public void Assign(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            if (mainThreadBehaviours != null)
            {
                foreach (KeyValuePair<int, List<Action>> behaviour in mainThreadBehaviours)
                {
                    behaviour.Value.Clear();
                }

                mainThreadBehaviours.Clear();
            }

            if (multiThreadBehaviours != null)
            {
                foreach (KeyValuePair<int, ConcurrentBag<Action>> behaviour in multiThreadBehaviours)
                {
                    behaviour.Value.Clear();
                }

                multiThreadBehaviours.Clear();
            }

            transitionBehaviour = null;
        }
    }
}