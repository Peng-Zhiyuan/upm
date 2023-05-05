using System;
using System.Collections.Generic;
using System.Reflection;

namespace BattleEngine.Logic
{
    public class BattleEventManager : Singleton<BattleEventManager>
    {
        private Dictionary<string, MethodInfo> mDelegatesMethod = new Dictionary<string, MethodInfo>();
        private Dictionary<string, object> mDelegatesObject = new Dictionary<string, object>();
        private Dictionary<int, List<object>> BattleInputEventDic = new Dictionary<int, List<object>>();
        private Dictionary<int, List<object>> BattleAIEventDic = new Dictionary<int, List<object>>();
        private Dictionary<int, List<object>> BattleProcessEventDic = new Dictionary<int, List<object>>();

        public override void Init()
        {
            mDelegatesObject.Clear();
            mDelegatesMethod.Clear();
            BattleProcessEventDic.Clear();
            BattleInputEventDic.Clear();
            BattleAIEventDic.Clear();
            InitInterfaces();
        }

        public void ClearEvent()
        {
            BattleProcessEventDic.Clear();
            BattleInputEventDic.Clear();
            BattleAIEventDic.Clear();
        }

        /// <summary>
        /// 初始化IBattleEvent实现接口
        /// </summary>
        private void InitInterfaces()
        {
            List<Type> list = ReflectUtil.FromAssemblyGetInterfaceTypes(typeof(IBattleEvent), "BattleEngine.Logic");
            for (int i = 0; i < list.Count; i++)
            {
                object obj = Activator.CreateInstance(list[i]);
                MethodInfo miGetName = list[i].GetMethod("GetEventName");
                MethodInfo miExcute = list[i].GetMethod("Excute");
                string eventName = miGetName.Invoke(obj, null) as string;
                mDelegatesObject[eventName] = obj;
                mDelegatesMethod[eventName] = miExcute;
            }
        }

        public void SendProcessEvent(int frameIndex, object obj)
        {
            if (BattleProcessEventDic.ContainsKey(frameIndex))
            {
                BattleProcessEventDic[frameIndex].Add(obj);
            }
            else
            {
                BattleProcessEventDic.Add(frameIndex, new List<object>() { obj });
            }
        }

        public void SendInputEvent(int frameIndex, object obj)
        {
            if (BattleInputEventDic.ContainsKey(frameIndex))
            {
                BattleInputEventDic[frameIndex].Add(obj);
            }
            else
            {
                BattleInputEventDic.Add(frameIndex, new List<object>() { obj });
            }
        }

        public void SendAIEvent(int frameIndex, object obj)
        {
            if (BattleAIEventDic.ContainsKey(frameIndex))
            {
                BattleAIEventDic[frameIndex].Add(obj);
            }
            else
            {
                BattleAIEventDic.Add(frameIndex, new List<object>() { obj });
            }
        }

#region Excute
        private List<object> eventLst = new List<object>();
        private string eventName = "";

        public void ExcuteProcessEvent(int frameIndex)
        {
            if (!BattleProcessEventDic.ContainsKey(frameIndex))
            {
                return;
            }
            eventLst = BattleProcessEventDic[frameIndex];
            for (int i = 0; i < eventLst.Count; i++)
            {
                eventName = (eventLst[i] as IBattleEventData).GetEventName();
                if (!ExcuteEvent(eventName, eventLst[i]))
                {
                    BattleLog.LogError("Battle Event Error " + eventName);
                }
            }
        }

        public void ExcuteInputEvent(int frameIndex)
        {
            if (!BattleInputEventDic.ContainsKey(frameIndex))
            {
                return;
            }
            eventLst = BattleInputEventDic[frameIndex];
            for (int i = 0; i < eventLst.Count; i++)
            {
                eventName = (eventLst[i] as IBattleEventData).GetEventName();
                if (!ExcuteEvent(eventName, eventLst[i]))
                {
                    BattleLog.LogError("Battle Event Error " + eventName);
                }
            }
        }

        public void ExcuteAIEvent(int frameIndex)
        {
            if (!BattleAIEventDic.ContainsKey(frameIndex))
            {
                return;
            }
            eventLst = BattleAIEventDic[frameIndex];
            for (int i = 0; i < eventLst.Count; i++)
            {
                eventName = (eventLst[i] as IBattleEventData).GetEventName();
                if (!ExcuteEvent(eventName, eventLst[i]))
                {
                    BattleLog.LogError("Battle Event Error " + eventName);
                }
            }
        }

        private object[] sendData = new object[2];

        private bool ExcuteEvent(string eventName, object param)
        {
            if (!mDelegatesObject.ContainsKey(eventName))
            {
                return false;
            }
            Array.Clear(sendData, 0, sendData.Length);
            sendData[0] = BattleLogicManager.Instance.BattleData;
            sendData[1] = param;
            mDelegatesMethod[eventName].Invoke(mDelegatesObject[eventName], sendData);
            return true;
        }
#endregion
    }
}