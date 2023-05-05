using UnityEngine;
using System.Collections;
using System;

namespace BehaviorDesigner.Runtime
{
    // ScriptableObjects do not have coroutines like monobehaviours do. Therefore we must add the functionality ourselves by using the parent behavior component which is a monobehaviour.
    public class TaskCoroutine
    {
        private IEnumerator mCoroutineEnumerator;
        private Coroutine mCoroutine;
        public Coroutine Coroutine { get { return mCoroutine; } }
        private Behavior mParent;
        private string mCoroutineName;
        private bool mStop = false;
        public void Stop() { mStop = true; }

        public TaskCoroutine(Behavior parent, IEnumerator coroutine, string coroutineName)
        {
            throw new Exception("[TaskCoroutine] not support yet");
            //mParent = parent;
            //mCoroutineEnumerator = coroutine;
            //mCoroutineName = coroutineName;
            //var e = RunCoroutine();
            //mCoroutine = parent.StartCoroutine(e);
        }

        public IEnumerator RunCoroutine()
        {
            while (!mStop) {
                if (mCoroutineEnumerator != null && mCoroutineEnumerator.MoveNext()) {
                    yield return mCoroutineEnumerator.Current;
                } else {
                    break;
                }
            }
            mParent.TaskCoroutineEnded(this, mCoroutineName);
        }
    }
}