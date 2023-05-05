using UnityEngine;
using System.Collections;
using Game.Skill;
using System.Collections.Generic;
using LitJson;
using UnityEditor;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using BattleSystem.Core;
using Event = Game.Skill.Event;
using Game.Skill;

public class GameTriggerConfig {
    public Dictionary<int, Skill> _dicSkillBehaviorlData = new Dictionary<int, Skill>();
    public Dictionary<int, Event> _dicEventsBehaviorlData = new Dictionary<int, Event>();
    public Dictionary<int, BufferParam> _dicBufferBehaviorlData = new Dictionary<int, BufferParam>();

    public void OnBattleCreate() {
        LoadData();

        LoadEventData();

        LoadBufferData();
    }


    public Event GetEventData(int index) {
        Event tmp = null;
        if (_dicEventsBehaviorlData.TryGetValue(index, out tmp)) {
            return tmp;
        }

        return null;
    }

    public void LoadEventData() {
        _dicEventsBehaviorlData.Clear();
        //TextAsset t = Resources.Load("GameData/Event", typeof(TextAsset)) as TextAsset;
        var t = CrossPlatform.LoadTextAsset("GameData/Event");
        if (t == null) {
            Debug.LogError("GameData/Event.json : resource not found");
            return;
        }

        var jdata = JsonConvert.DeserializeObject(t.ToString()) as JObject;
        Event[] items = JsonConvert.DeserializeObject<Event[]>(jdata["Event"].ToString());
        _dicEventsBehaviorlData = new Dictionary<int, Event>((int)items.Length);
        foreach (Event item in items) {
            _dicEventsBehaviorlData[item.ID] = item;
        }

        EventManager.Instance.AddListener<int>("PathEvent", (msg) => {
            Debug.Log("msg:" + msg);
            if (msg == 1) {
                var cmd = CommandPool.Instance.GetByName("Progress Jump");
                if (cmd == null) {
                    StartPlan(18);
                }
                else {
                    CommandPool.Instance.RemoveId(cmd.ID);
                }
            }
            else if (msg == 3) {
                SceneObjectManager.Instance.LocalPlayerCamera.IsFollow = true;
            }
        });

        //var sceneEvent = Battle.Instance.battleCore.FindStuff<SceneEventManager>();
        /*var sceneEvent = this.SceneEventManager;
        if (sceneEvent != null) {
            sceneEvent.RegisterListener("TriggerStep", StartPlan);
        }*/
    }

    public void StartPlan(int param = 0) {
        if (_dicEventsBehaviorlData.ContainsKey(param)) {
            var e = this.GetEventData(param);
            if (e != null) {
                //var triggerFactor = this.TriggerFactory;
                /*var controller = triggerFactor.CreateController(e, 0, 0, Group.Event);
                if (controller != null) {
                    //Debug.LogError("触发了事件" + param);
                    var gameTriggerManager = Battle.Instance.GetBattleComponent<GameTriggerManager>();
                    gameTriggerManager.PlayTrigger(controller, null);
                }*/
            }
        }
    }

    public void LoadBufferData() {
        _dicBufferBehaviorlData.Clear();
        var t = CrossPlatform.LoadTextAsset("GameData/Buffer");
        if (t == null) {
            Debug.LogError("GameData/Buffer.json : resource not found");
            return;
        }

        //    //SkillParam[] items = CustomLitJson.JsonMapper.Instance.ToObject<SkillParam[]>(data);
        var jdata = JsonConvert.DeserializeObject(t.ToString()) as JObject;
        BufferParam[] items = JsonConvert.DeserializeObject<BufferParam[]>(jdata["Buffer"].ToString());
        _dicBufferBehaviorlData = new Dictionary<int, BufferParam>((int)items.Length);
        foreach (BufferParam item in items) {
            _dicBufferBehaviorlData[item.ID] = item;
        }
    }

    public string skillDataJson;

    public void LoadData() {
        _dicSkillBehaviorlData.Clear();
        var t = CrossPlatform.LoadTextAsset("GameData/Skill");

        if (t == null) {
            Debug.LogError("GameData/Skill.json : resource not found");
            return;
        }

        var json = t.text;
        skillDataJson = json;


        var jdata = JsonConvert.DeserializeObject(json) as JObject;
        Skill[] items = JsonConvert.DeserializeObject<Skill[]>(jdata["Skill"].ToString());
        _dicSkillBehaviorlData = new Dictionary<int, Skill>((int)items.Length);
        foreach (Skill item in items) {
            _dicSkillBehaviorlData[item.ID] = item;
        }


        Debug.Log("Skill Config Loades success!");
    }
    
    public Skill GetSkillParam(int skillID) {
        Skill tmp_skill = null;
        if (_dicSkillBehaviorlData.TryGetValue(skillID, out tmp_skill)) { }

        if (tmp_skill == null) {
            Debug.LogError("------------------skillid = " + skillID);
        }

        return tmp_skill;
    }

    public BufferParam GetBufferParam(int skillID) {
        BufferParam tmp_skill = null;
        if (_dicBufferBehaviorlData.TryGetValue(skillID, out tmp_skill)) { }

        return tmp_skill;
    }
    
}