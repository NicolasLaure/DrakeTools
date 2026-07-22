using DrakeToolbox.Events;

namespace DrakeToolbox.Factory
{
    public struct FactoryResetSuccessEvent : IEvent
    {
        public void Assign(params object[] parameters)
        {
        }

        public void Reset()
        {
        }
    }
}