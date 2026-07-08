using System;
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

        public FactoryController()
        {
            EventBus.AddListener<LocalInstantiateRequest>(HandleLocalInstantiateRequest);
        }

        public void Dispose()
        {
            EventBus.RemoveListener<LocalInstantiateRequest>(HandleLocalInstantiateRequest);
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

            FactoryMapping[instanceType].CreateInstance(instanceType, callbackContext.instanceData.blueprintId, callbackContext.instanceData.originalClientID, callbackContext.instanceData.constructorParameters);
            EventBus.Raise<LocalInstantiateRequestAccepted>(callbackContext.instanceData.blueprintId);
        }
    }
}