using System;
using DrakeToolbox.Events;

namespace DrakeToolbox.Factory.Events
{
    public struct InstanceCreatedEvent<InstanceType> : IEvent where InstanceType : Instance
    {
        public string blueprintId;
        public uint instanceId;
        public uint ownerId;
        public object[] constructorParameters;

        public void Assign(params object[] parameters)
        {
            blueprintId = (string)parameters[0];
            instanceId = (uint)parameters[1];
            ownerId = (uint)parameters[2];
            constructorParameters = (object[])parameters[3];
        }

        public void Reset()
        {
            blueprintId = string.Empty;
            instanceId = default(uint);
            ownerId = default(uint);
            constructorParameters = Array.Empty<object>();
        }
    }
}