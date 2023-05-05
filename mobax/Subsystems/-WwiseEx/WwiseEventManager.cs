using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WwiseEventManager
{
    /// <summary>
    /// 发送事件，事件会先查表兑换成对应的内部事件再发送，如何转换由音频团队定义
    /// </summary>
    /// <param name="table">转换表</param>
    /// <param name="gameEvent">事件</param>
    /// <param name="sourceObj">音源物体</param>
    public static void SendEvent(TransformTable table, string gameEvent, GameObject sourceObj = null)
    {
        var isUseWwise = DeveloperLocalSettings.IsUseWwise;
        if (!isUseWwise || string.IsNullOrEmpty(gameEvent))
        {
            return;
        }
        //Debug.Log($"[Wwise] game event : {gameEvent} (table: {table})");
        if ((Application.isPlaying && ClientConfigManager.OnLoadTableTextAsset == null) || (!Application.isPlaying && ClientConfigManager.OnEditorModeLoadTableTextAsset == null))
        {
            Debug.LogWarning("[Wwise] game event : " + table + "  skiped due to table not loaded");
            return;
        }
        if (table == TransformTable.BeginSkillAbility)
        {
            OnBeginSkillAbility(gameEvent, sourceObj);
        }
        else if (table == TransformTable.EndSkillTimeline)
        {
            OnEndSkillTimelinet(gameEvent, sourceObj);
        }
        else if (table == TransformTable.EndSkillAbility)
        {
            OnEndSkillAbility(gameEvent, sourceObj);
        }
        else if (table == TransformTable.SkillHit)
        {
            OnSkillHit(gameEvent, sourceObj);
        }
        else if (table == TransformTable.HeroVoice_Hit)
        {
            OnHeroBeHit(gameEvent, sourceObj);
        }
        else if (table == TransformTable.HeroVoice_Dead)
        {
            OnHeroDead(gameEvent, sourceObj);
        }
        else if (table == TransformTable.TimelineStart)
        {
            OnTimelineStart(gameEvent, sourceObj);
        }
        else if (table == TransformTable.TimelineEnd)
        {
            OnTimelineEnd(gameEvent, sourceObj);
        }
        else if (table == TransformTable.MovieStart)
        {
            OnMovieStart(gameEvent, sourceObj);
        }
        else if (table == TransformTable.SceneLoad)
        {
            OnSceneLoad(gameEvent);
        }
        else if (table == TransformTable.UiControls)
        {
            OnUiControls(gameEvent);
        }
        else if (table == TransformTable.UiOpen)
        {
            OnUiOpen(gameEvent);
        }
        else if (table == TransformTable.UiClose)
        {
            OnUiClose(gameEvent);
        }
        else if (table == TransformTable.EffectStart)
        {
            OnEffectStart(gameEvent);
        }
        else if (table == TransformTable.EffectEnd)
        {
            OnEffectEnd(gameEvent);
        }
        /*        else if (gameEvent == WwiseGameEvent.AudioClipSe)
                {
                    OnAudioClipSe(param);
                }*/
        else if (table == TransformTable.Custom)
        {
            OnCustom(gameEvent, sourceObj);
        }
        else if (table == TransformTable.Voice)
        {
            OnAudioVoice(gameEvent);
        }
        else if (table == TransformTable.Comics)
        {
            OnAudioComics(gameEvent);
        }
    }

    static void OnAudioComics(string resName)
    {
        if (ClientConfig.AudioComicsTable.ContainsKey(resName))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioComicsTable[resName].eventName);
        }
    }

    static void OnAudioVoice(string resName)
    {
        if (ClientConfig.AudioVoiceTable.ContainsKey(resName))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioVoiceTable[resName].eventName);
        }
    }

    /*    static void OnAudioClipSe(string resName)
        {
            if (ClientConfig.AudioSeTable.ContainsKey(resName))
            {
                WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioSeTable[resName].eventName);
            }
        }*/

    static void OnCustom(string id, GameObject obj = null)
    {
        if (ClientConfig.AudioCustomTable.ContainsKey(id))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioCustomTable[id].eventName, obj);
        }
    }

    static void OnBeginSkillAbility(string id, GameObject obj)
    {
        if (ClientConfig.AudioSkillTable.ContainsKey(id))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioSkillTable[id].startEvent, obj);
        }
    }

    static void OnEndSkillTimelinet(string id, GameObject obj)
    {
        if (ClientConfig.AudioSkillTable.ContainsKey(id))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioSkillTable[id].timelineEndEvent, obj);
        }
    }

    static void OnEndSkillAbility(string id, GameObject obj)
    {
        if (ClientConfig.AudioSkillTable.ContainsKey(id))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioSkillTable[id].stopEvent, obj);
        }
    }

    static void OnSkillHit(string id, GameObject obj)
    {
        if (ClientConfig.AudioSkillTable.ContainsKey(id))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioSkillTable[id].hitEvent, obj);
        }
    }

    static void OnHeroBeHit(string id, GameObject obj)
    {
        if (ClientConfig.AudioHeroVoiceTable.ContainsKey(id))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioHeroVoiceTable[id].hitEvent, obj);
        }
    }

    static void OnHeroDead(string id, GameObject obj)
    {
        //Debug.LogError("OnHeroDead:" + id);
        if (ClientConfig.AudioHeroVoiceTable.ContainsKey(id))
        {
            //Debug.LogError("Wwise OnHeroDead:" + id);
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioHeroVoiceTable[id].deadEvent, obj);
        }
    }

    static void OnTimelineStart(string name, GameObject obj)
    {
        if (ClientConfig.AudioTimelineTable.ContainsKey(name))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioTimelineTable[name].openEventName, obj);
        }
    }

    static void OnTimelineEnd(string name, GameObject obj)
    {
        if (ClientConfig.AudioTimelineTable.ContainsKey(name))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioTimelineTable[name].closeEventName, obj);
        }
    }

    static void OnMovieStart(string name, GameObject obj)
    {
        //TODO
        if (ClientConfig.AudioCustomTable.ContainsKey(name))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioCustomTable[name].eventName, obj);
        }
    }

    static void OnSceneLoad(string name)
    {
        if (ClientConfig.AudioSceneTable.ContainsKey(name))
        {
            WwiseManagerEx.GetInstance().TryPostEventList(ClientConfig.AudioSceneTable[name].eventName);
        }
    }

    static void OnUiControls(string name)
    {
        var row = ClientConfig.AudioUiControlsTable.TryGet(name);
        if (row != null)
        {
            WwiseManagerEx.GetInstance().TryPostEventList(row.eventName);
        }
    }

    static void OnUiOpen(string uiName)
    {
        var row = ClientConfig.AudioUiPrefabTable.TryGet(uiName);
        if (row != null)
        {
            var eventList = row.openEventName;
            WwiseManagerEx.GetInstance().TryPostEventList(eventList);
        }
    }

    static void OnUiClose(string uiName)
    {
        var row = ClientConfig.AudioUiPrefabTable.TryGet(uiName);
        if (row != null)
        {
            var eventList = row.closeEventName;
            WwiseManagerEx.GetInstance().TryPostEventList(eventList);
        }
    }

    static void OnEffectStart(string fxName)
    {
        var row = ClientConfig.AudioEffectTable.TryGet(fxName);
        if (row != null)
        {
            var eventList = row.eventName;
            WwiseManagerEx.GetInstance().TryPostEventList(eventList);
        }
    }

    static void OnEffectEnd(string fxName)
    {
        var row = ClientConfig.AudioEffectTable.TryGet(fxName);
        if (row != null)
        {
            var eventList = row.closeName;
            WwiseManagerEx.GetInstance().TryPostEventList(eventList);
        }
    }
}