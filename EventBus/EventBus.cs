using DrakeToolbox.Services;
using DrakeTools.Pool;

namespace DrakeTools.Events
{
    public sealed class EventBus : IService
    {
        public delegate void EventCallback<EventType>(in EventType callback) where EventType : struct, IEvent;

        public bool IsPersistent => false;

        private readonly ConcurrentPool eventPool = new ConcurrentPool();
        private readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

        public void AddListener<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (!subscribers.ContainsKey(eventType))
                subscribers.Add(eventType, new List<Delegate>());

            subscribers[eventType].Add(callback);
        }

        public void RemoveListener<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (!subscribers.ContainsKey(eventType))
                throw new KeyNotFoundException();

            subscribers[eventType].Remove(callback);
        }

        public void Raise<EventType>(params object[] parameters) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            EventType raisingEvent = eventPool.Get<EventType>(parameters);
            if (subscribers.TryGetValue(eventType, out List<Delegate> listeners))
            {
                foreach (Delegate callback in listeners)
                {
                    ((EventCallback<EventType>)callback)?.Invoke(raisingEvent);
                }
            }
            eventPool.Release<EventType>(raisingEvent);
        }

        public void Clear()
        {
            subscribers.Clear();
        }
    }
}