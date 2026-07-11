using System;
using System.Collections.Generic;
using DrakeToolbox.Console;
using DrakeToolbox.Events;
using DrakeToolbox.Factory.Events;
using DrakeToolbox.Services;

namespace DrakeToolbox.Factory
{
    public sealed class Registry<InstanceType> : IService where InstanceType : Instance
    {
        public bool IsPersistent => false;

        private Dictionary<uint, InstanceType> instances;
        private Dictionary<Type, List<uint>> instanceIdsPerType;

        public InstanceType this[uint id] => instances[id];
        public Dictionary<Type, List<uint>> InstanceIdsPerType => instanceIdsPerType;

        public string RegisterMethodName => nameof(Register);
        public string DisposeInstanceMethodName => nameof(DisposeInstance);
        public string ClearInstancesMethodName => nameof(ClearInstances);

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public Registry()
        {
            instances = new Dictionary<uint, InstanceType>();
            instanceIdsPerType = new Dictionary<Type, List<uint>>();
        }

        private void Register(InstanceType entity)
        {
            instances.Add(entity.Id, entity);
            Type currentEntityType = entity.GetType();
            do
            {
                if (!instanceIdsPerType.ContainsKey(currentEntityType))
                    instanceIdsPerType.Add(currentEntityType, new List<uint>());

                instanceIdsPerType[currentEntityType].Add(entity.Id);
                currentEntityType = currentEntityType.BaseType;
            } while (currentEntityType != typeof(InstanceType));
        }

        private void DisposeInstance(uint id)
        {
            if (!instances.ContainsKey(id))
                return;

            EventBus.Raise<InstanceDestroyedEvent<InstanceType>>(id);
            InstanceType instance = instances[id];
            instances.Remove(id);

            Type currentEntityType = instance.GetType();
            do
            {
                if (!instanceIdsPerType.ContainsKey(currentEntityType))
                    instanceIdsPerType.Add(currentEntityType, new List<uint>());
                instanceIdsPerType[currentEntityType].Remove(id);

                currentEntityType = currentEntityType.BaseType;
            } while (currentEntityType != typeof(InstanceType));
        }

        private void ClearInstances()
        {
            instances.Clear();

            foreach (List<uint> ids in instanceIdsPerType.Values)
                ids.Clear();

            instanceIdsPerType.Clear();
        }

        public EntityType GetEntity<EntityType>(uint id) where EntityType : InstanceType
        {
            return (EntityType)instances[id];
        }
    }
}