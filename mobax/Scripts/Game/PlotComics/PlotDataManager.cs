using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Plot.Runtime
{
    public class IPlotEventData
    {
        public int Id;
        public int Type;
        public int Arg1; // 怪物ID
        public int Arg2; // 血量百分比
        public int PauseBattle;
        public List<string> Comics;
        public string Timeline;
        public string Cg;
        public int ChatType;
        public int ChatId; // 对话种类2-立绘对话，1-地图对话，3-战斗内对话
        public string Bgm;
    }

    public class IPlotManagerData
    {
        public int SoleId; // 唯一Id
    }

    public class IPlotChatData
    {
        public int Key;
        public int Pos;
        public string Name;
        public string Chat;
        public string Sound;
        public string Model;
        public int ModelScale;
        public string Emote;
        public int Animation;
        public string Action;
        public string Dub; // 配音
        public int Choice;
        public string ChatBg;
        public string MaskMaterial;
    }

    public class IPlotChatChoiceData
    {
        public int Num;
        public string Des;
        public int ChatId;
    }

    /// <summary>
    /// 管理整个剧情需要的配置表数据
    /// </summary>
    public class PlotDataManager : StuffObject<PlotDataManager>
    {
        [Sirenix.OdinInspector.ShowInInspector]
        private Dictionary<int, IPlotEventData> _plotDataDic = new Dictionary<int, IPlotEventData>();

        private Dictionary<int, List<IPlotManagerData>> _plotManagerDic = new Dictionary<int, List<IPlotManagerData>>();
        private Dictionary<int, List<IPlotChatData>> _plotChatDic = new Dictionary<int, List<IPlotChatData>>();

        private Dictionary<int, List<IPlotChatChoiceData>> _plotChatChoiceDic =
            new Dictionary<int, List<IPlotChatChoiceData>>();

        #region ---Get---

        public IPlotEventData GetPlotEventData(int plotEventId)
        {
            return this._plotDataDic.TryGetValue(plotEventId, default);
        }

        public List<IPlotManagerData> GetPlotManagerData(int managerId)
        {
            return this._plotManagerDic.TryGetValue(managerId, default);
        }

        public List<IPlotChatData> GetPlotChatData(int chatId)
        {
            return this._plotChatDic.TryGetValue(chatId, default);
        }

        public List<IPlotChatChoiceData> GetPlotChatChoiceData(int chatId)
        {
            return this._plotChatChoiceDic.TryGetValue(chatId, default);
        }

        #endregion


        #region ---Add---

        public void Add2CurrentPlotData(IDictionary dic)
        {
            foreach (var kv in dic)
            {
                this.Add((DictionaryEntry) kv);
            }
        }

        public void Add2CurrentPlotManagerData(IDictionary dic)
        {
            foreach (var kv in dic)
            {
                this.AddManagerData((DictionaryEntry) kv);
            }
        }

        public void Add2CurrentPlotChatData(IDictionary dic)
        {
            foreach (var kv in dic)
            {
                this.AddChatData((DictionaryEntry) kv);
            }
        }

        public void Add2CurrentPlotChatChoiceData(IDictionary dic)
        {
            foreach (var kv in dic)
            {
                this.AddChatChoiceData((DictionaryEntry) kv);
            }
        }

        void AddChatChoiceData(DictionaryEntry dic)
        {
            var value = dic.Value;
            var type = value.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var chatDataList = new List<IPlotChatChoiceData>();
            var id = -1;
            foreach (var p in properties)
            {
                if (p.Name == "Id")
                {
                    id = (int) p.GetValue(value);
                }

                if (p.Name == "Colls")
                {
                    var list = p.GetValue(value) as IList;
                    foreach (var l in list)
                    {
                        var chatData = new IPlotChatChoiceData();
                        var tt = l.GetType();
                        var props = tt.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var ppp in props)
                        {
                            if (ppp.Name == "Num")
                            {
                                chatData.Num = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "chatId")
                            {
                                chatData.ChatId = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Des")
                            {
                                chatData.Des = (string) ppp.GetValue(l);
                            }
                        }

                        chatDataList.Add(chatData);
                    }
                }
            }

            this._plotChatChoiceDic[id] = chatDataList;
        }

        void AddChatData(DictionaryEntry dic)
        {
            var value = dic.Value;
            var type = value.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var chatDataList = new List<IPlotChatData>();
            var id = -1;
            foreach (var p in properties)
            {
                if (p.Name == "Id")
                {
                    id = (int) p.GetValue(value);
                }

                if (p.Name == "Colls")
                {
                    var list = p.GetValue(value) as IList;
                    foreach (var l in list)
                    {
                        var chatData = new IPlotChatData();
                        var tt = l.GetType();
                        var props = tt.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var ppp in props)
                        {
                            if (ppp.Name == "Key")
                            {
                                chatData.Key = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Pos")
                            {
                                chatData.Pos = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Name")
                            {
                                chatData.Name = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Chat")
                            {
                                chatData.Chat = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Sound")
                            {
                                chatData.Sound = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "modelScale")
                            {
                                chatData.ModelScale = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Model")
                            {
                                chatData.Model = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Emote")
                            {
                                chatData.Emote = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Animation")
                            {
                                chatData.Animation = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Action")
                            {
                                chatData.Action = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Dub")
                            {
                                chatData.Dub = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "Choice")
                            {
                                chatData.Choice = (int) ppp.GetValue(l);
                            }

                            if (ppp.Name == "chatBg")
                            {
                                chatData.ChatBg = (string) ppp.GetValue(l);
                            }

                            if (ppp.Name == "maskMateria")
                            {
                                chatData.MaskMaterial = (string) ppp.GetValue(l);
                            }
                        }

                        chatDataList.Add(chatData);
                    }
                }
            }

            this._plotChatDic[id] = chatDataList;
        }

        private void AddManagerData(DictionaryEntry dic)
        {
            var value = dic.Value;
            var type = value.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var managerDataList = new List<IPlotManagerData>();
            var id = -1;
            foreach (var p in properties)
            {
                if (p.Name == "Id")
                {
                    id = (int) p.GetValue(value);
                }

                if (p.Name == "Colls")
                {
                    var list = p.GetValue(value) as IList;
                    foreach (var l in list)
                    {
                        var managerData = new IPlotManagerData();
                        var tt = l.GetType();
                        var props = tt.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var ppp in props)
                        {
                            if (ppp.Name == "Soleid")
                            {
                                managerData.SoleId = (int) ppp.GetValue(l);
                            }
                        }

                        managerDataList.Add(managerData);
                    }
                }
            }

            this._plotManagerDic[id] = managerDataList;
        }

        private void Add(DictionaryEntry dic)
        {
            var value = dic.Value;
            var type = value.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var eventData = new IPlotEventData();

            foreach (var p in properties)
            {
                if (p.Name == "Id")
                {
                    eventData.Id = (int) p.GetValue(value);
                }
                else if (p.Name == "Type")
                {
                    eventData.Type = (int) p.GetValue(value);
                }
                else if (p.Name == "Arg1")
                {
                    eventData.Arg1 = (int) p.GetValue(value);
                }
                else if (p.Name == "Arg2")
                {
                    eventData.Arg2 = (int) p.GetValue(value);
                }
                else if (p.Name == "pauseBattle")
                {
                    eventData.PauseBattle = (int) p.GetValue(value);
                }
                else if (p.Name == "Comics")
                {
                    eventData.Comics = (List<string>) p.GetValue(value);
                }
                else if (p.Name == "Timeline")
                {
                    eventData.Timeline = (string) p.GetValue(value);
                }
                else if (p.Name == "Cg")
                {
                    eventData.Cg = (string) p.GetValue(value);
                }
                else if (p.Name == "Bgm")
                {
                    eventData.Bgm = (string) p.GetValue(value);
                }
                else if (p.Name == "chatType")
                {
                    eventData.ChatType = (int) p.GetValue(value);
                }
                else if (p.Name == "Chatid")
                {
                    eventData.ChatId = (int) p.GetValue(value);
                }
            }

            this._plotDataDic[(int) dic.Key] = eventData;
        }

        #endregion
    }
}