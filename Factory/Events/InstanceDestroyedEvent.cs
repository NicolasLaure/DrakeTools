using DrakeToolbox.Events;

namespace DrakeToolbox.Factory.Events
{
    public struct InstanceDestroyedEvent<InstanceType> : IEvent where InstanceType : Instance
    {
        public uint instanceId;

        public void Assign(params object[] parameters)
        {
            instanceId = (uint)parameters[0];
        }

        public void Reset()
        {
            instanceId = default(uint);
        }
    }
}