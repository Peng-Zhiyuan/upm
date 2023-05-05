using System;

namespace BattleEngine.Logic
{
    public class Component : IDisposable
    {
        public Entity Entity { get; set; }
        public bool IsDisposed { get; set; }
        public virtual bool Enable { get; set; }
        public bool Disable => Enable == false;

        public T GetEntity<T>() where T : Entity
        {
            return Entity as T;
        }

        public virtual void Setup() { }
        public virtual void Setup(object initData) { }
        public virtual void Update(float deltaTime) { }
        public virtual void OnDestroy() { }

        public virtual void Dispose()
        {
            //BattleLog.Log($"{GetType().Name}->Dispose");
            IsDisposed = true;
        }

        public T Publish<T>(T TEvent) where T : class
        {
            Entity.Publish(TEvent);
            return TEvent;
        }

        public void Subscribe<T>(Action<T> action) where T : class
        {
            Entity.Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            Entity.UnSubscribe(action);
        }
    }
}