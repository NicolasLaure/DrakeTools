using DrakeToolbox.Events;

namespace DrakeToolbox.Factory
{
    public struct LocalInstantiateRequestAccepted : IEvent
    {
        public uint instanceId;
        public string type;

        public void Assign(params object[] parameters)
        {
            instanceId = (uint)parameters[0];
            type = (string)parameters[1];
        }

        public void Reset()
        {
            instanceId = default(uint);
            type = string.Empty;
        }
    }
}