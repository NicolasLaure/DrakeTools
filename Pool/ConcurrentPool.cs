using System.Collections.Concurrent;

namespace DrakeTools.Pool
{
    public class ConcurrentPool
    {
        private ConcurrentDictionary<Type, ConcurrentStack<IResettable>> concurrentPool =
            new ConcurrentDictionary<Type, ConcurrentStack<IResettable>>();

        public ResettableType Get<ResettableType>(params object[] parameters) where ResettableType : IResettable
        {
            Type resettableType = typeof(ResettableType);

            if (!concurrentPool.ContainsKey(resettableType))
                concurrentPool.TryAdd(resettableType, new ConcurrentStack<IResettable>());

            ResettableType value;
            if (concurrentPool[resettableType].Count > 0)
            {
                concurrentPool[resettableType].TryPop(out IResettable poppedElement);
                value = (ResettableType)poppedElement;
            }
            else
            {
                value = (ResettableType)Activator.CreateInstance(resettableType);
            }

            value.Assign(parameters);
            return value;
        }

        public void Release<ResettableType>(IResettable instance) where ResettableType : IResettable
        {
            instance.Reset();
            if (!concurrentPool.ContainsKey(typeof(ResettableType)))
                concurrentPool.TryAdd(typeof(ResettableType), new ConcurrentStack<IResettable>());

            concurrentPool[typeof(ResettableType)].Push(instance);
        }
    }
}