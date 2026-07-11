using DrakeToolbox.Events;

namespace DrakeToolbox.Factory
{
    public struct ResetFactoryEvent : IEvent
    {
        public void Assign(params object[] parameters)
        {
        }

        public void Reset()
        {
        }
    }
}