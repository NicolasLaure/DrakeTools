using System;
using System.Collections.Generic;
using System.Reflection;
using DrakeToolbox.Services;

namespace DrakeToolbox.Factory
{
    public sealed class FactoryMapping : IService
    {
        public bool IsPersistent => false;

        private Dictionary<Type, Type> instanceToFactoryType;

        public Factory this[Type instanceType]
        {
            get
            {
                if (!instanceToFactoryType.ContainsKey(instanceType))
                    return null;

                return ServiceProvider.Instance.GetService(instanceToFactoryType[instanceType]) as Factory;
            }
        }

        public FactoryMapping()
        {
            instanceToFactoryType = new Dictionary<Type, Type>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                List<FactoryOf> attributes = new List<FactoryOf>(type.GetCustomAttributes<FactoryOf>());
                if (attributes.Count > 0)
                {
                    Type instanceType = attributes[0].instanceType;
                    instanceToFactoryType.Add(instanceType, type);
                }
            }
        }
    }
}