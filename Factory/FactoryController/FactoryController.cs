using System;
using System.Collections.Generic;
using DrakeToolbox.Events;
using DrakeToolbox.Factory.Events;
using DrakeToolbox.Logging;
using DrakeToolbox.Services;

namespace DrakeToolbox.Factory
{
    public class FactoryController : IDisposable
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private FactoryMapping FactoryMapping => ServiceProvider.Instance.GetService<FactoryMapping>();
        private Logger Logger => ServiceProvider.Instance.GetService<Logger>();

        private uint nextInstanceId;
        private uint defaultInstanceId;

        public uint NextInstanceId => nextInstanceId;

        private SortedDictionary<uint, InstanceData> heldInstances;

        public FactoryController(uint defaultInstanceId)
        {
            EventBus.AddListener<LocalInstantiateRequest>(HandleLocalInstantiateRequest);
            EventBus.AddListener<LocalDeInstantiateRequest>(HandleLocalDeInstantiateRequest);
            EventBus.AddListener<ResetFactoryEvent>(HandleResetFactoryRequest);
            this.defaultInstanceId = defaultInstanceId;
            nextInstanceId = defaultInstanceId;

            heldInstances = new SortedDictionary<uint, InstanceData>();
        }

        public void Dispose()
        {
            EventBus.RemoveListener<LocalInstantiateRequest>(HandleLocalInstantiateRequest);
            EventBus.RemoveListener<LocalDeInstantiateRequest>(HandleLocalDeInstantiateRequest);
            EventBus.RemoveListener<ResetFactoryEvent>(HandleResetFactoryRequest);
        }

        public void Sync(uint instanceId)
        {
            nextInstanceId = instanceId;
        }

        public object GetInstance(string type, uint instanceId)
        {
            Type instanceType = FactoryMapping[type];
            if (instanceType == null)
            {
                Logger.Log($"No Factory available for {type}");
                return null;
            }

            return FactoryMapping[instanceType].GetInstance(instanceId);
        }

        public bool TryGetInstanceCount(string type, out int instanceCount)
        {
            Type instanceType = FactoryMapping[type];
            instanceCount = 0;
            if (instanceType == null)
                return false;

            instanceCount = FactoryMapping[instanceType].GetInstancesCount();
            return true;
        }

        private void HandleLocalInstantiateRequest(in LocalInstantiateRequest callbackContext)
        {
            TryInstantiate(callbackContext.instanceData, callbackContext.clientId);
        }

        private void TryInstantiate(InstanceData instanceData, uint clientId)
        {
            Type instanceType = FactoryMapping[instanceData.instanceType];
            if (instanceType == null)
            {
                Logger.Log($"No Factory available for {instanceData.instanceType}");
                EventBus.Raise<LocalInstantiateRequestRejected>($"No Factory available for {instanceData.instanceType}");
                return;
            }

            if (FactoryMapping[instanceType].ContainsInstance(nextInstanceId))
            {
                Logger.Log($"InstanceId {nextInstanceId} was already present");
                EventBus.Raise<LocalInstantiateRequestRejected>($"InstanceId {nextInstanceId} was already present");
                return;
            }

            if (instanceData.instanceID > nextInstanceId)
            {
                heldInstances.Add(instanceData.instanceID, instanceData);
                return;
            }

            uint result = FactoryMapping[instanceType].CreateInstance(nextInstanceId, instanceType, instanceData.blueprintId, instanceData.originalClientID, clientId, instanceData.constructorParameters);
            if (nextInstanceId != 0 && result == 0)
            {
                Logger.Log($"Instantiation Failed");
                EventBus.Raise<LocalInstantiateRequestRejected>($"Instantiation Failed");
                return;
            }

            EventBus.Raise<LocalInstantiateRequestAccepted>(nextInstanceId, instanceData.instanceType);
            nextInstanceId++;
            EventBus.Raise<NonGenericInstanceCreatedEvent>(instanceData);
        }

        public object[] GetParameters(string typeName, byte[] parameterBytes)
        {
            Type instanceType = FactoryMapping[typeName];
            if (instanceType == null)
                return Array.Empty<object>();

            return FactoryMapping[instanceType].GetParameters(instanceType, parameterBytes);
        }

        private void HandleLocalDeInstantiateRequest(in LocalDeInstantiateRequest callbackContext)
        {
            Type instanceType = FactoryMapping[callbackContext.instanceTypeName];
            if (instanceType == null)
            {
                Logger.Log($"No Factory available for {callbackContext.instanceTypeName}");
                EventBus.Raise<LocalDeInstantiateRequestRejected>($"No Factory available for {callbackContext.instanceTypeName}");
                return;
            }

            FactoryMapping[instanceType].Deinstantiate(callbackContext.instanceId);
            EventBus.Raise<LocalDeInstantiateRequestAccepted>();
        }

        private void HandleResetFactoryRequest(in ResetFactoryEvent callbackContext)
        {
            nextInstanceId = defaultInstanceId;
            Logger.Log("Reset Factories");

            foreach (Type type in FactoryMapping.FactoryInstanceTypes)
            {
                FactoryMapping[type].Reset();
            }

            Logger.Log("Factories Resetted");
            EventBus.Raise<FactoryResetSuccessEvent>();
        }
    }
}