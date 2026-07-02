using System;
using System.Collections.Generic;
using System.Reflection;
using DrakeToolbox.Blueprints;
using DrakeToolbox.Console;
using DrakeToolbox.Events;
using DrakeToolbox.Services;

namespace DrakeToolbox.Factory
{
    public abstract class Factory
    {
        protected EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        protected BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();
        private Logger Logger => ServiceProvider.Instance.GetService<Logger>();

        protected uint lastAssignedId;
        protected Dictionary<Type, ConstructorInfo> constructors;
        protected Dictionary<string, Type> instanceClassNameToType;

        protected MethodInfo registerMethod;
        protected MethodInfo raiseCreatedMethod;

        protected Dictionary<Type, object> creationSubscriptions;
        protected MethodInfo subscribeToCreationMethod;
        protected MethodInfo unsubscribeMethod;
        protected MethodInfo raiseEntityRequestAcceptedMethod;

        private Type instanceType;

        protected Factory(uint defaultLastAssignedId, Type instanceType)
        {
            lastAssignedId = defaultLastAssignedId;
            this.instanceType = instanceType;

            constructors = new Dictionary<Type, ConstructorInfo>();
            creationSubscriptions = new Dictionary<Type, object>();
            instanceClassNameToType = new Dictionary<string, Type>();
        }

        public abstract uint CreateInstance(Type instanceType, params object[] parameters);

        protected void RegisterInstanceMethods(params Type[] constructorParameters)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                {
                    if (instanceType.IsAssignableFrom(type))
                    {
                        RegisterInstance(type, constructorParameters);
                        subscribeToCreationMethod.MakeGenericMethod(type).Invoke(this, new object[0]);
                        instanceClassNameToType.Add(type.Name, type);
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
            if (!requestedType.IsAssignableFrom(instanceType) || !constructors.TryGetValue(requestedType, out ConstructorInfo constructor))
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
    }
}