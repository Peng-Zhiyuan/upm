using System;
using System.Collections.Generic;

namespace ListPool
{
    internal static class ListPoolN<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>();

        public static List<T> Get()
        {
            List<T> list = s_ListPool.Get();
            if (list == null)
                list = new List<T>();
            return list;
        }

        public static void Release(List<T> toRelease)
        {
            toRelease.Clear();
            s_ListPool.Release(toRelease);
        }
    }

    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> stack = new Stack<T>();

        public T Get()
        {
            if (stack.Count == 0)
                return null;
            var element = stack.Pop();
            return element;
        }

        public void Release(T element)
        {
            if (stack.Count > 0
                && ReferenceEquals(stack.Peek(), element))
                UnityEngine.Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            stack.Push(element);
        }
    }
}