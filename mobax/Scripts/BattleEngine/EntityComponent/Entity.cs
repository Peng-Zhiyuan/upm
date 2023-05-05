namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;

    public abstract partial class Entity
    {
        public static MasterEntity Master => MasterEntity.Instance;

        private static Entity NewEntity(Type entityType)
        {
            var entity = Activator.CreateInstance(entityType) as Entity;
            entity.InstanceId = IdFactory.NewInstanceId();
            if (!Master.Entities.ContainsKey(entityType))
            {
                Master.Entities.Add(entityType, new List<Entity>());
            }
            Master.Entities[entityType].Add(entity);
            return entity;
        }

        public static T Create<T>() where T : Entity
        {
            var entity = NewEntity(typeof(T)) as T;
            entity.Id = entity.InstanceId;
            Master.AddChild(entity);
            entity.Awake();
            return entity;
        }

        public static T Create<T>(object initData) where T : Entity
        {
            var entity = NewEntity(typeof(T)) as T;
            entity.Id = entity.InstanceId;
            Master.AddChild(entity);
            entity.Awake(initData);
            return entity;
        }

        public static T CreateWithParent<T>(Entity parent) where T : Entity
        {
            var entity = NewEntity(typeof(T)) as T;
            entity.Id = entity.InstanceId;
            parent.AddChild(entity);
            entity.Awake();
            return entity;
        }

        public static T CreateWithParent<T>(Entity parent, object initData) where T : Entity
        {
            var entity = NewEntity(typeof(T)) as T;
            entity.Id = entity.InstanceId;
            parent.AddChild(entity);
            entity.Awake(initData);
            return entity;
        }

        public static void Destroy(Entity entity)
        {
            if (entity == null)
                return;
            entity.OnDestroy();
            entity.Dispose();
            entity = null;
        }
    }

    public abstract partial class Entity : IDisposable
    {
#if !SERVER
        public UnityEngine.GameObject GameObject { get; set; }
#endif
        public long Id { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set
            {
#if !SERVER
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    GameObject.name = $"{GetType().Name}: {value}";
                }
#endif
                name = value;
            }
        }
        public long InstanceId { get; set; }
        private Entity parent;
        public Entity Parent
        {
            get { return parent; }
            private set
            {
                parent = value;
                OnSetParent(value);
            }
        }
        public bool IsDisposed
        {
            get { return InstanceId == 0; }
        }
        public Dictionary<Type, Component> Components { get; set; } = new Dictionary<Type, Component>();
        private List<Entity> Children { get; set; } = new List<Entity>();
        private Dictionary<Type, List<Entity>> Type2Children { get; set; } = new Dictionary<Type, List<Entity>>();

        public Entity()
        {
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                GameObject = new UnityEngine.GameObject(GetType().Name);
                UnityEngine.GameObject.DontDestroyOnLoad(GameObject);
                var view = GameObject.AddComponent<ComponentView>();
                view.Type = GameObject.name;
                view.Component = this;
            }
#endif
        }

        public virtual void Awake() { }
        public virtual void Awake(object initData) { }

        public virtual void OnDestroy()
        {
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                UnityEngine.GameObject.DestroyImmediate(GameObject);
            }
#endif
        }

        // public virtual void Update() { }
        public virtual void OnUpdate(int currentFrame) { }
        public virtual void LogicUpdate(float deltaTime) { }

        public void Dispose()
        {
            if (Children.Count > 0)
            {
                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    Entity.Destroy(Children[i]);
                }
                Children.Clear();
                Type2Children.Clear();
            }
            var data = this.Components.GetEnumerator();
            while (data.MoveNext())
            {
                data.Current.Value.OnDestroy();
                data.Current.Value.Dispose();
            }
            this.Components.Clear();
            Parent?.RemoveChild(this);
            InstanceId = 0;
            if (Master.Entities.ContainsKey(GetType()))
            {
                Master.Entities[GetType()].Remove(this);
            }
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                UnityEngine.GameObject.DestroyImmediate(GameObject);
            }
#endif
        }

        public virtual void OnSetParent(Entity parent) { }

        public T GetParent<T>() where T : Entity
        {
            return parent as T;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();
            component.Entity = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            Master.AllComponents.Add(component);
            component.Setup();
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                var view = GameObject.AddComponent<ComponentView>();
                view.Type = typeof(T).Name;
                view.Component = component;
            }
#endif
            return component;
        }

        public T AddComponent<T>(object initData) where T : Component, new()
        {
            var component = new T();
            component.Entity = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            Master.AllComponents.Add(component);
            component.Setup(initData);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                var view = GameObject.AddComponent<ComponentView>();
                view.Type = typeof(T).Name;
                view.Component = component;
            }
#endif
            return component;
        }

        public void RemoveComponent<T>() where T : Component
        {
            this.Components[typeof(T)].OnDestroy();
            this.Components[typeof(T)].Dispose();
            this.Components.Remove(typeof(T));
        }

        public T GetComponent<T>() where T : Component
        {
            if (this.Components.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }
            return null;
        }

        public void SetParent(Entity parent)
        {
            Parent?.RemoveChild(this);
            parent?.AddChild(this);
        }

        public void AddChild(Entity child)
        {
            Children.Add(child);
            if (!Type2Children.ContainsKey(child.GetType()))
            {
                Type2Children.Add(child.GetType(), new List<Entity>());
            }
            Type2Children[child.GetType()].Add(child);
            child.Parent = this;
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer
                && child.GameObject != null && GameObject != null)
                child.GameObject.transform.SetParent(GameObject.transform);
#endif
        }

        public void RemoveChild(Entity child)
        {
            Children.Remove(child);
            if (Type2Children.ContainsKey(child.GetType()))
            {
                Type2Children[child.GetType()].Remove(child);
            }
            child.Parent = null;
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer
                && child.GameObject != null)
                child.GameObject.transform.SetParent(null);
#endif
        }

        public Entity[] GetChildren()
        {
            return Children.ToArray();
        }

        public Entity[] GetTypeChildren<T>() where T : Entity
        {
            return Type2Children[typeof(T)].ToArray();
        }

        public T Publish<T>(T TEvent) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                return TEvent;
            }
            eventComponent.Publish(TEvent);
            return TEvent;
        }

        public EventSubscribe<T> Subscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            return eventComponent.Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.UnSubscribe(action);
            }
        }
    }
}