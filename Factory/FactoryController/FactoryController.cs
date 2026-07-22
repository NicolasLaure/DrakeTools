using System;
using System.Collections.Generic;
using DrakeToolbox.Console;
using DrakeToolbox.Events;
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
            Type instanceType = FactoryMapping[callbackContext.instanceData.instanceType];
            if (instanceType == null)
            {
                Logger.Log($"No Factory available for {callbackContext.instanceData.instanceType}");
                EventBus.Raise<LocalInstantiateRequestRejected>($"No Factory available for {callbackContext.instanceData.instanceType}");
                return;
            }

            if (FactoryMapping[instanceType].ContainsInstance(nextInstanceId))
            {
                Logger.Log($"InstanceId {nextInstanceId} was already present");
                EventBus.Raise<LocalInstantiateRequestRejected>($"InstanceId {nextInstanceId} was already present");
                return;
            }

            if (callbackContext.instanceData.instanceID > nextInstanceId)
            {
                heldInstances.Add(callbackContext.instanceData.instanceID, callbackContext.instanceData);
                return;
            }

            FactoryMapping[instanceType].CreateInstance(nextInstanceId, instanceType, callbackContext.instanceData.blueprintId, callbackContext.instanceData.originalClientID, callbackContext.clientId, callbackContext.instanceData.constructorParameters);
            EventBus.Raise<LocalInstantiateRequestAccepted>(nextInstanceId, callbackContext.instanceData.instanceType);
            nextInstanceId++;

            foreach (uint instanceId in heldInstances.Keys)
            {
                if (instanceId > nextInstanceId) return;

                FactoryMapping[instanceType].CreateInstance(nextInstanceId, instanceType, callbackContext.instanceData.blueprintId, callbackContext.instanceData.originalClientID, callbackContext.clientId, callbackContext.instanceData.constructorParameters);
                EventBus.Raise<LocalInstantiateRequestAccepted>(nextInstanceId, callbackContext.instanceData.instanceType);
            }
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