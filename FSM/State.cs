using System;

namespace StateMachine
{
    public abstract class State
    {
        public Action<Enum> OnFlag;

        public virtual Type[] OnEnterParametersTypes => Array.Empty<Type>();
        public virtual Type[] OnTickParametersTypes => Array.Empty<Type>();
        public virtual Type[] OnExitParametersTypes => Array.Empty<Type>();

        public virtual BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            return null;
        }

        public virtual BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            return null;
        }

        public virtual BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            return null;
        }
    }
}