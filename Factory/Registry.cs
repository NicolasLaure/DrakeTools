using System;
using System.Collections.Generic;
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
        public string InstancesFieldName => nameof(instances);

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public Registry()
        {
            instances = new Dictionary<uint, InstanceType>();
            instanceIdsPerType = new Dictionary<Type, List<uint>>();
        }

        private void Register(InstanceType entity)
        {
            instances.Add(entity.Id, entity);
            Type? currentEntityType = null;
            do
            {
                currentEntityType = currentEntityType == null ? entity.GetType() : currentEntityType.BaseType;
                if (currentEntityType != null && !instanceIdsPerType.ContainsKey(currentEntityType))
                    instanceIdsPerType.Add(currentEntityType, new List<uint>());

                instanceIdsPerType[currentEntityType].Add(entity.Id);
            } while (currentEntityType != typeof(InstanceType));
        }

        private void DisposeInstance(uint id)
        {
            if (!instances.ContainsKey(id))
                return;

            EventBus.Raise<InstanceDestroyedEvent<InstanceType>>(id);
            InstanceType instance = instances[id];
            instances.Remove(id);

            Type? currentEntityType = null;
            do
            {
                currentEntityType = currentEntityType == null ? instance.GetType() : currentEntityType.BaseType;

                if (currentEntityType != null && !instanceIdsPerType.ContainsKey(currentEntityType))
                    instanceIdsPerType.Add(currentEntityType, new List<uint>());

                instanceIdsPerType[currentEntityType].Remove(id);
            } while (currentEntityType != typeof(InstanceType));
        }

        private void ClearInstances()
        {
            List<uint> instanceIds = new List<uint>(instances.Keys);
            foreach (uint instanceId in instanceIds)
            {
                DisposeInstance(instanceId);
            }

            instances.Clear();
            instanceIdsPerType.Clear();
        }

        public EntityType GetEntity<EntityType>(uint id) where EntityType : InstanceType
        {
            if (instances[id] as EntityType != null)
                return (EntityType)instances[id];
            else
                return null;
        }

        public int GetCount()
        {
            return instances.Count;
        }

        public bool Contains(uint id)
        {
            return instances.ContainsKey(id);
        }
    }
}