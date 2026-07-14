using System;
using System.Collections.Generic;
using System.Reflection;
using DrakeToolbox.Services;

namespace DrakeToolbox.Factory
{
    public class GenericFactory<InstanceType> : Factory where InstanceType : Instance
    {
        private Registry<InstanceType> InstanceRegistry => ServiceProvider.Instance.GetService<Registry<InstanceType>>();

        public GenericFactory(string blueprintTableName) : base(typeof(InstanceType), blueprintTableName)
        {
            registerMethod = InstanceRegistry.GetType().GetMethod(InstanceRegistry.RegisterMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            disposeMethod = InstanceRegistry.GetType().GetMethod(InstanceRegistry.DisposeInstanceMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            clearMethod = InstanceRegistry.GetType().GetMethod(InstanceRegistry.ClearInstancesMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal override uint CreateInstance(uint newEntityId, Type requestedType, string blueprintId, uint ownerId, params object[] parameters)
        {
            if (!IsCreationValid(requestedType, parameters))
                return 0;


            object newEntity = constructors[requestedType].Invoke(parameters);
            typeToSetIdMethod[requestedType].Invoke(newEntity, new object[] { newEntityId, ownerId });

            if (registerMethod == null)
                throw new MissingMethodException($"Missing EntityRegistry register method");
            registerMethod.Invoke(InstanceRegistry, new object[] { newEntity });

            BlueprintBinder.Apply(ref newEntity, blueprintTable, blueprintId);

            ((InstanceType)newEntity).Init();

            List<Type> entityTypes = new List<Type>();
            Type currentType = null;

            do
            {
                currentType = currentType == null ? newEntity.GetType() : currentType.BaseType;
                entityTypes.Add(currentType);
            } while (currentType != typeof(InstanceType));

            for (int i = entityTypes.Count - 1; i >= 0; i--)
            {
                raiseCreatedMethod.MakeGenericMethod(entityTypes[i]).Invoke(this, new object[] { blueprintId, newEntityId, ownerId, parameters });
            }

            ((InstanceType)newEntity).LateInit();
            return newEntityId;
        }

        internal override void Deinstantiate(uint instanceId)
        {
            disposeMethod.Invoke(InstanceRegistry, new object[] { instanceId });
        }

        internal override void Reset()
        {
            clearMethod.Invoke(InstanceRegistry, new object[] { });
        }
    }
}