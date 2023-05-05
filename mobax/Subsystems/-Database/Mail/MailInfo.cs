using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using Sirenix.OdinInspector;

public class MailInfo 
{
    public string _id;

    /// <summary>
    /// 0 是个人邮件 -2 是 sdk 邮件，其他是模板邮件
    /// </summary>
    public int id;
    public string uid;

    public AttrItem[] attr;
    public string title;
    public string content;
    public long time;
    public int type;
    public string from;
    public MailState status;

    public Dictionary<string, int> args;

    public enum MailType
    {
        Normal,
        Template,
        Sdk,
    }

    public MailType LogicType
    {
        get
        {
            if(this.id == 0)
            {
                return MailType.Normal;
            }
            else if(this.id == -2)
            {
                return MailType.Sdk;
            }
            else if(this.id == -1)
            {
                // 在自己后台手动发送，但是模拟 sdk 的格式
                return MailType.Sdk;
            }
            else
            {
                return MailType.Template;
            }
        }
    }

    public class AttrItem
    {
        public int id;
        public int num;
    }

    public bool HasAttach
    {
        get
        {
            if (this.attr == null || this.attr.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// 是否已处理：
    /// 1. 如果有附件，领取后叫做已处理
    /// 2. 如果没有附件，阅读后叫做已处理
    /// </summary>
    public bool IsProcessed
    {
        get
        {
            var hasAttach = this.HasAttach;
            if (!hasAttach)
            {
                if(this.status == MailState.Read || this.status == MailState.Recyclable)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if(this.status == MailState.Recyclable)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    MailRow Row
    {
        get
        {
            if(this.id == 0)
            {
                return null;
            }
            else
            {
                var row = StaticData.MailTable[this.id];
                return row;
            }
        }
    }

    public string LogicFrom
    {
        get
        {
            var type = this.LogicType;
            if(type == MailType.Normal)
            {
                return this.from;
            }
            else if(type == MailType.Sdk)
            {
                return "m1_mail_system".Localize();
            }
            else if(type == MailType.Template)
            {
                var row = this.Row;
                var ret = row.From.Localize();
                return ret;
            }
            throw new System.Exception("[MailInfo] unsupport type: " + type);
        }
    }

    public string LogicTitle
    {
        get
        {
            var type = this.LogicType;
            if (type == MailType.Normal)
            {
                return this.title;
            }
            else if(type == MailType.Sdk)
            {
                var language = LocalizationManager.Stuff.Language;
                var ret = this.SdkMailInfo.GetTitle(language);
                return ret;
            }
            else if(type == MailType.Template)
            {
                var row = this.Row;
                var ret = row.Title.Localize();
                return ret;
            }
            throw new System.Exception("[MailInfo] unsupport type: " + type);
        }
    }

    string _resolvedContgent;
    public string LogicContent
    {
        get
        {
            var type = this.LogicType;
            if(type == MailType.Normal)
            {
                return this.content;
            }
            else if (type == MailType.Sdk)
            {
                var language = LocalizationManager.Stuff.Language;
                var ret = this.SdkMailInfo.GetContent(language);
                return ret;
            }
            else if(type == MailType.Template)
            {
                if(_resolvedContgent == null)
                {
                    var row = this.Row;
                    var content = row.Content.Localize();
                    this._resolvedContgent = ResolveContentVariables(content, this.args);
                }
                return _resolvedContgent;
            }
            throw new System.Exception("[MailInfo] unsupport type: " + type);
        }
    }

    static string ResolveContentVariables(string orgin, Dictionary<string, int> args)
    {
        var text = orgin;
        if(text.Contains("{arenatype}"))
        {
            var arenaLevel = DictionaryUtil.TryGet(args, "ArenaLevel", -1);
            if(arenaLevel != -1)
            {
                var name = GetArenaRankName(arenaLevel);
                name = name.Localize();
                text = text.Replace("{arenatype}", name);
            }
            else
            {
                text = text.Replace("{arenatype}", "Unknown");
            }
        }
        return text;

    }

    static string GetArenaRankName(int id)
    {
        var row = StaticData.ArenaTable.TryGet(id);
        if(row == null)
        {
            return "Unknown";
        }
        else
        {
            return row.Rank;
        }
    }

    [ShowInInspector]
    SdkMailInfo _sdkMailInfo;
    bool _sdkMailInfoParsed;
    public SdkMailInfo SdkMailInfo
    {
        get
        {
            if(!_sdkMailInfoParsed)
            {
                try
                {
                    this._sdkMailInfo = JsonMapper.Instance.ToObject<SdkMailInfo>(this.content);
                }
                catch
                {

                }
                if(this._sdkMailInfo == null)
                {
                    this._sdkMailInfo = new SdkMailInfo();
                }
                _sdkMailInfoParsed = true;
            }
            return _sdkMailInfo;
        }
    }
}

public enum MailState
{
    New = 0,    // 新邮件
    Read = 1,   // 已读
    Recyclable = 2, // 已领取附件
    Deleted = 9,    // 逻辑删除
}