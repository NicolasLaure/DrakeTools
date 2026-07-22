using System;
using DrakeToolbox.Events;

namespace DrakeToolbox.Factory
{
    public struct LocalInstantiateRequest : IEvent
    {
        public InstanceData instanceData;
        public uint clientId;

        public void Assign(params object[] parameters)
        {
            instanceData = (InstanceData)parameters[0];
            clientId = (uint)parameters[1];
        }

        public void Reset()
        {
            instanceData = default(InstanceData);
            clientId = default(uint);
        }
    }
}