using System;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    public static class ObjectPool
    {
        private static Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();
        private static Object lockObject = new Object();

        public static T Get<T>()
        {
            T newObject;
            lock (lockObject) {
                if (poolDictionary.ContainsKey(typeof(T))) {
                    var pooledObjects = poolDictionary[typeof(T)] as Stack<T>;
                    if (pooledObjects.Count > 0) {
                        return pooledObjects.Pop();
                    }
                }

                newObject = (T) TaskUtility.CreateInstance(typeof(T));
            }

            return newObject;
        }

        public static void Return<T>(T obj)
        {
            if (obj == null) {
                return;
            }

            lock (lockObject) {
                object value;
                if (poolDictionary.TryGetValue(typeof(T), out value)) {
                    var pooledObjects = value as Stack<T>;
                    pooledObjects.Push(obj);
                } else {
                    var pooledObjects = new Stack<T>();
                    pooledObjects.Push(obj);
                    poolDictionary.Add(typeof(T), pooledObjects);
                }
            }
        }

        public static void Clear()
        {
            lock (lockObject) {
                poolDictionary.Clear();
            }
        }
    }
}