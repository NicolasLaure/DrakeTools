using System;
using DrakeToolbox.Events;

namespace DrakeToolbox.Factory
{
    public struct LocalDeInstantiateRequest : IEvent
    {
        public string instanceTypeName;
        public uint instanceId;

        public void Assign(params object[] parameters)
        {
            instanceTypeName = (string)parameters[0];
            instanceId = (uint)parameters[1];
        }

        public void Reset()
        {
            instanceTypeName = string.Empty;
            instanceId = default(uint);
        }
    }
}