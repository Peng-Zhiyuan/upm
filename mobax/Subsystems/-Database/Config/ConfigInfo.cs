using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using Sirenix.OdinInspector;

public class ConfigInfo 
{
    public ConfigType AType;

    // 生效结束时间，可能是时间戳，也可能是相对于某个时间点的天数
    public long ETime;

     // 生效开始时间，可能是时间戳，也可能是相对于某个时间点的天数
    public long STime;

    // 时间格式
    public int TType;
    public string id;
    public string attach;
    public string content;
    public int group;
    ///public string name;
    public string title;
    
    public int sort;

    //活动状态,0-测试,1-上线,2-下线
    public int status;
    public long update;

    // 公告图资源名
    public string banner;
    public long create;

    public bool HasBanner
    {
        get
        {
            return !string.IsNullOrEmpty(banner);
        }
    }
    public bool HasAttach
    {
        get
        {
            return !string.IsNullOrEmpty(this.attach);
        }
    }

    public bool IsValid
    {
        get
        {
            var startDate = Clock.ToDateTimeAllowNull(this.STime);
            var endDate = Clock.ToDateTimeAllowNull(this.ETime);
            var now = Clock.Now;
            var inTime = DateUtil.IsBetween(now, startDate, endDate);
            if(!inTime)
            {
                return false;
            }

            // 3 类型的超级邮件
            // 只有创建时间比账号注册时间完才有效
            if(this.AType == ConfigType.CreateTimeSuperMail)
            {
                var roleRegisterTime = Database.Stuff.roleDatabase.Me.logon;
                var mailCreateTime = create;
                if(roleRegisterTime <= mailCreateTime)
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
                return true;
            }
        }
    }


    [ShowInInspector]
    SdkMailInfo _sdkMailInfo;
    bool _sdkMailInfoParsed;
    public SdkMailInfo SdkMailInfo
    {
        get
        {
            if (!_sdkMailInfoParsed)
            {
                try
                {
                    this._sdkMailInfo = JsonMapper.Instance.ToObject<SdkMailInfo>(this.content);
                }
                catch
                {

                }
                if (this._sdkMailInfo == null)
                {
                    this._sdkMailInfo = new SdkMailInfo();
                }
                _sdkMailInfoParsed = true;
            }
            return _sdkMailInfo;
        }
    }

    public string SuperMailLogicContent
    {
        get
        {
            var language = LocalizationManager.Stuff.Language;
            var ret = this.SdkMailInfo.GetContent(language);
            return ret;
        }
    }


    public string SuperMailLogicTitle
    {
        get
        {
            var language = LocalizationManager.Stuff.Language;
            var ret = this.SdkMailInfo.GetTitle(language);
            return ret;
        }
    }

}

public enum ConfigType
{
    Notice = 1,
    SuperMail = 2,
    CreateTimeSuperMail = 3,
}
