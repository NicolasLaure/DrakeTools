using System;
using System.Collections.Generic;

namespace DrakeToolbox.Services
{
    public sealed class ServiceProvider
    {
        private static ServiceProvider instance = null;

        public static ServiceProvider Instance
        {
            get
            {
                if (instance == null)
                    instance = new ServiceProvider();

                return instance;
            }
        }

        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();

        public void AddService<ServiceType>(IService service) where ServiceType : class, IService
        {
            if (!services.ContainsKey(typeof(ServiceType)))
                services.Add(typeof(ServiceType), service);
        }

        public void RemoveService<ServiceType>(Type serviceType) where ServiceType : class, IService
        {
            if (!services.ContainsKey(typeof(ServiceType)))
                throw new KeyNotFoundException();

            services.Remove(typeof(ServiceType));
        }

        public ServiceType GetService<ServiceType>() where ServiceType : class, IService
        {
            if (!services.ContainsKey(typeof(ServiceType)))
                throw new KeyNotFoundException();

            return services[typeof(ServiceType)] as ServiceType;
        }

        public bool ContainsService<ServiceType>() where ServiceType : class, IService
        {
            return services.ContainsKey(typeof(ServiceType));
        }

        public void ClearServices()
        {
            services.Clear();
        }

        public void ClearNonPersistentServices()
        {
            List<Type> servicesToRemove = new List<Type>();
            foreach (Type serviceType in services.Keys)
            {
                if (!services[serviceType].IsPersistent)
                    servicesToRemove.Add(serviceType);
            }

            foreach (Type serviceType in servicesToRemove)
            {
                services.Remove(serviceType);
            }
        }
    }
}