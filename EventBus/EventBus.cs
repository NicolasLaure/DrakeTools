using System;
using System.Collections.Generic;
using DrakeToolbox.Pool;
using DrakeToolbox.Services;

namespace DrakeToolbox.Events
{
    public sealed class EventBus : IService
    {
        public delegate void EventCallback<EventType>(in EventType callback) where EventType : struct, IEvent;

        public bool IsPersistent => false;

        private readonly ConcurrentPool eventPool = new ConcurrentPool();
        private readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

        private Dictionary<Type, List<Delegate>> subscribersToDelete = new Dictionary<Type, List<Delegate>>();

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
            if (!subscribersToDelete.ContainsKey(eventType))
                subscribersToDelete.Add(eventType, new List<Delegate>());

            subscribersToDelete[eventType].Add(callback);
        }

        public void Raise<EventType>(params object[] parameters) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (subscribersToDelete.ContainsKey(eventType) && subscribers.ContainsKey(eventType))
            {
                for (int i = 0; i < subscribersToDelete[eventType].Count; i++)
                {
                    if (subscribers[eventType].Contains(subscribersToDelete[eventType][i]))
                        subscribers[eventType].Remove(subscribersToDelete[eventType][i]);
                }

                subscribersToDelete[eventType].Clear();
            }

            EventType raisingEvent = eventPool.Get<EventType>(parameters);
            if (subscribers.TryGetValue(eventType, out List<Delegate> listeners))
            {
                foreach (Delegate callback in listeners)
                {
                    if (subscribersToDelete.ContainsKey(eventType) && subscribersToDelete[eventType].Contains(callback))
                        continue;

                    ((EventCallback<EventType>)callback)?.Invoke(raisingEvent);
                }
            }

            eventPool.Release(raisingEvent);
        }

        public void Clear()
        {
            subscribers.Clear();
        }
    }
}