using System;
using System.Collections.Generic;
using System.Reflection;
using DrakeToolbox.Blueprints;
using DrakeToolbox.Console;
using DrakeToolbox.Events;
using DrakeToolbox.Factory.Events;
using DrakeToolbox.Formatting;
using DrakeToolbox.Services;

namespace DrakeToolbox.Factory
{
    public abstract class Factory
    {
        protected EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        protected BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();
        private Logger Logger => ServiceProvider.Instance.GetService<Logger>();

        protected Dictionary<Type, ConstructorInfo> constructors;
        protected Dictionary<Type, MethodInfo> typeToSetIdMethod;
        protected Dictionary<string, Type> instanceClassNameToType;

        protected MethodInfo? registerMethod;
        protected MethodInfo? disposeMethod;
        protected MethodInfo? clearMethod;
        protected MethodInfo raiseCreatedMethod;

        protected string blueprintTable;

        private Type instanceType;

        protected Factory(Type instanceType, string blueprintTable)
        {
            this.blueprintTable = blueprintTable;
            this.instanceType = instanceType;

            constructors = new Dictionary<Type, ConstructorInfo>();
            instanceClassNameToType = new Dictionary<string, Type>();
            typeToSetIdMethod = new Dictionary<Type, MethodInfo>();

            raiseCreatedMethod = typeof(Factory).GetMethod(nameof(RaiseInstanceCreated), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal uint CreateInstance(uint newEntityId, Type requestedType, string blueprintId, uint ownerId, uint clientId, params byte[] parameters)
        {
            if (!constructors.ContainsKey(requestedType))
                throw new KeyNotFoundException($"Cannot create instance of {requestedType.Name}");

            ParameterInfo[] parameterInfos = constructors[requestedType].GetParameters();
            Type[] parameterTypes = new Type[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
                parameterTypes[i] = parameterInfos[i].ParameterType;

            return CreateInstance(newEntityId, requestedType, blueprintId, ownerId, clientId, ByteFormat.ToObjectArray(parameters, 0, parameterTypes));
        }

        internal abstract uint CreateInstance(uint newEntityId, Type requestedType, string blueprintId, uint ownerId, uint clientId, params object[] parameters);
        internal abstract void Deinstantiate(uint instanceId);
        internal abstract void Reset();

        protected void RegisterInstanceMethods(params Type[] constructorParameters)
        {
            foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                {
                    if (typeof(Instance).IsAssignableFrom(type))
                        if (instanceType.IsAssignableFrom(type))
                        {
                            RegisterInstance(type, constructorParameters);
                            instanceClassNameToType.Add(type.Name, type);
                            typeToSetIdMethod.Add(type, typeof(Instance).GetMethod(Instance.SetIdsMethodName, BindingFlags.NonPublic | BindingFlags.Instance));
                        }
                }
            }

            void RegisterInstance(Type type, Type[] instanceConstructorParameters)
            {
                foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length != instanceConstructorParameters.Length)
                        continue;

                    bool isValid = true;
                    for (int i = 0; i < instanceConstructorParameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != instanceConstructorParameters[i])
                            isValid = false;
                    }

                    if (isValid)
                    {
                        constructors.Add(type, constructor);
                        break;
                    }
                }
            }
        }

        protected bool IsCreationValid(Type requestedType, params object[] parameters)
        {
            if (!instanceType.IsAssignableFrom(requestedType) || !constructors.TryGetValue(requestedType, out ConstructorInfo constructor))
            {
                Logger.LogError($"Can't create instance of {requestedType.Name}");
                return false;
            }

            ParameterInfo[] parameterInfos = constructor.GetParameters();
            if (parameters.Length != parameterInfos.Length)
            {
                Logger.LogError($"Parameters Length doesn't match {requestedType.Name} constructor parameters");
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].GetType() != parameterInfos[i].ParameterType)
                {
                    Logger.LogError($"Parameters Order doesn't match {requestedType.Name} constructor parameters");
                    return false;
                }
            }

            return true;
        }

        private void RaiseInstanceCreated<InstanceType>(string blueprintId, uint instanceId, uint ownerId, object[] parameters) where InstanceType : Instance
        {
            EventBus.Raise<InstanceCreatedEvent<InstanceType>>(blueprintId, instanceId, ownerId, parameters);
        }

        public abstract object GetInstance(uint id);
        public abstract int GetInstancesCount();
        public abstract bool ContainsInstance(uint id);
    }
}