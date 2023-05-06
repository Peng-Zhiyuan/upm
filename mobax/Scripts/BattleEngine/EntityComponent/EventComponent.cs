namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;

    public sealed class EventSubscribeCollection<T> where T : class
    {
        public readonly List<EventSubscribe<T>> Subscribes = new List<EventSubscribe<T>>();
        public readonly Dictionary<Action<T>, EventSubscribe<T>> Action2Subscribes = new Dictionary<Action<T>, EventSubscribe<T>>();

        public EventSubscribe<T> Add(Action<T> action)
        {
            var eventSubscribe = new EventSubscribe<T>();
            eventSubscribe.EventAction = action;
            Subscribes.Add(eventSubscribe);
            Action2Subscribes.Add(action, eventSubscribe);
            return eventSubscribe;
        }

        public void Remove(Action<T> action)
        {
            Subscribes.Remove(Action2Subscribes[action]);
            Action2Subscribes.Remove(action);
        }
    }

    public sealed class EventSubscribe<T>
    {
        public Action<T> EventAction;
        public bool Coroutine;

        public void AsCoroutine()
        {
            Coroutine = true;
        }
    }

    public sealed class EventComponent : Component
    {
        public override bool Enable { get; set; } = true;
        private Dictionary<Type, object> EventSubscribeCollections = new Dictionary<Type, object>();
        private Dictionary<object, object> CoroutineEventSubscribeQueue = new Dictionary<object, object>();
        public static bool DebugLog { get; set; } = false;

        public override void Update(float deltaTime)
        {
            if (CoroutineEventSubscribeQueue.Count > 0)
            {
                var data = CoroutineEventSubscribeQueue.GetEnumerator();
                while (data.MoveNext())
                {
                    var evnt = data.Current.Key;
                    var eventSubscribe = data.Current.Value;
                    var field = eventSubscribe.GetType().GetField("EventAction");
                    var value = field.GetValue(eventSubscribe);
                    value.GetType().GetMethod("Invoke").Invoke(value, new object[] { evnt });
                }
                CoroutineEventSubscribeQueue.Clear();
            }
        }

        public new T Publish<T>(T TEvent) where T : class
        {
            if (EventSubscribeCollections.TryGetValue(typeof(T), out var collection))
            {
                var eventSubscribeCollection = collection as EventSubscribeCollection<T>;
                if (eventSubscribeCollection.Subscribes.Count == 0)
                {
                    return TEvent;
                }
                var arr = eventSubscribeCollection.Subscribes.ToArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Coroutine == false)
                    {
                        arr[i].EventAction.Invoke(TEvent);
                    }
                    else
                    {
                        CoroutineEventSubscribeQueue.Add(TEvent, arr[i]);
                    }
                }
            }
            return TEvent;
        }

        public new EventSubscribe<T> Subscribe<T>(Action<T> action) where T : class
        {
            EventSubscribeCollection<T> eventSubscribeCollection;
            if (EventSubscribeCollections.TryGetValue(typeof(T), out var collection))
            {
                eventSubscribeCollection = collection as EventSubscribeCollection<T>;
            }
            else
            {
                eventSubscribeCollection = new EventSubscribeCollection<T>();
                EventSubscribeCollections.Add(typeof(T), eventSubscribeCollection);
            }
            return eventSubscribeCollection.Add(action);
        }

        public new void UnSubscribe<T>(Action<T> action) where T : class
        {
            if (EventSubscribeCollections.TryGetValue(typeof(T), out var collection))
            {
                var eventSubscribeCollection = collection as EventSubscribeCollection<T>;
                eventSubscribeCollection.Remove(action);
            }
        }
    }
}