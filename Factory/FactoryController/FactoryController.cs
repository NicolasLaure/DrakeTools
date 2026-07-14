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

        private uint lastInstanceId;
        private const uint DEFAULT_INSTANCE_ID = 0;

        public FactoryController()
        {
            EventBus.AddListener<LocalInstantiateRequest>(HandleLocalInstantiateRequest);
            EventBus.AddListener<LocalDeInstantiateRequest>(HandleLocalDeInstantiateRequest);
            EventBus.AddListener<ResetFactoryEvent>(HandleResetFactoryRequest);
            lastInstanceId = DEFAULT_INSTANCE_ID;
        }

        public void Dispose()
        {
            EventBus.RemoveListener<LocalInstantiateRequest>(HandleLocalInstantiateRequest);
            EventBus.RemoveListener<LocalDeInstantiateRequest>(HandleLocalDeInstantiateRequest);
            EventBus.RemoveListener<ResetFactoryEvent>(HandleResetFactoryRequest);
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

            lastInstanceId++;
            FactoryMapping[instanceType].CreateInstance(lastInstanceId, instanceType, callbackContext.instanceData.blueprintId, callbackContext.instanceData.originalClientID, callbackContext.instanceData.constructorParameters);
            EventBus.Raise<LocalInstantiateRequestAccepted>(callbackContext.instanceData.blueprintId);
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
            lastInstanceId = DEFAULT_INSTANCE_ID;
            List<Type> instanceTypes = FactoryMapping.FactoryInstanceTypes;
            foreach (Type type in instanceTypes)
            {
                FactoryMapping[type].Reset();
            }
        }
    }
}