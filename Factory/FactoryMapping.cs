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
        private Dictionary<string, Type> instanceTypeNameToInstanceType;

        private Dictionary<Type, (MethodInfo instantiateMethod, MethodInfo deinstantiateMethod, MethodInfo clearMethod)> fabricTypeToMethods;

        internal List<Type> FactoryInstanceTypes => new List<Type>(instanceToFactoryType.Keys);

        internal Factory? this[Type instanceType]
        {
            get
            {
                if (!instanceToFactoryType.ContainsKey(instanceType))
                    return null;

                return ServiceProvider.Instance.GetService(instanceToFactoryType[instanceType]) as Factory;
            }
        }

        public Type? this[string instanceTypeName]
        {
            get
            {
                if (!instanceTypeNameToInstanceType.ContainsKey(instanceTypeName))
                    return null;

                return instanceTypeNameToInstanceType[instanceTypeName];
            }
        }

        public FactoryMapping()
        {
            instanceToFactoryType = new Dictionary<Type, Type>();
            instanceTypeNameToInstanceType = new Dictionary<string, Type>();

            foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
            {
                List<FactoryOf> attributes = new List<FactoryOf>(type.GetCustomAttributes<FactoryOf>());
                if (attributes.Count > 0)
                {
                    foreach (Type instanceType in attributes[0].instanceTypes)
                    {
                        instanceToFactoryType.Add(instanceType, type);
                        instanceTypeNameToInstanceType.Add(instanceType.Name, instanceType);
                    }
                }
            }
        }
    }
}