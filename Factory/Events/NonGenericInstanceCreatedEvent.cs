using DrakeToolbox.Events;

namespace DrakeToolbox.Factory.Events
{
    public struct NonGenericInstanceCreatedEvent : IEvent
    {
        public InstanceData instanceData;

        public void Assign(params object[] parameters)
        {
            instanceData = (InstanceData)parameters[0];
        }

        public void Reset()
        {
            instanceData = default(InstanceData);
        }
    }
}