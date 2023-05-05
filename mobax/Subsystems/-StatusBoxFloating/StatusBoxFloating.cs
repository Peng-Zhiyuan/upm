using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class StatusBoxFloating : Floating
{
    public Text text_status;

    public override void OnShow(object param)
    {
        this.Refresh();
    }

    StringBuilder sb = new StringBuilder(); 
    public void AddLine(string line)
    {
        if(sb.Length != 0)
        {
            sb.AppendLine();
        }
        sb.Append(line);
    }

    public void Append(string msg)
    {
        sb.Append(msg);
    }

    public void NewContent()
    {
        sb.Clear();
    }

    public string GetString()
    {
        var ret = sb.ToString();
        return ret;
    }

    public void OnButton(string msg)
    {
        if (msg == "commandline")
        {
            if (DeveloperLocalSettings.IsDevelopmentMode)
            {
                UIEngine.Stuff.ShowFloating<ConsoleFloating>();
            }
        }
        else if (msg == "performance")
        {
            if (DeveloperLocalSettings.IsDevelopmentMode)
            {
                UIEngine.Stuff.ShowFloating<PerformanceFloating>();
            }
        }
    }

    public void Refresh()
    {
        this.NewContent();


        var originEnv = EnvManager.OriginEnv;
        var finalEnv = EnvManager.FinalEnv;
        if(originEnv == finalEnv)
        {
            this.AddLine($"env: {originEnv}");
        }
        else
        {
            this.AddLine($"env: {originEnv} -> {finalEnv}");
        }
        if (!Remote.Stuff.isConnected)
        {
            this.AddLine($"(not connected)");
        }

        //var session = LoginManager.Stuff.session;
        //if (session != null)
        //{
        //    var uid = session.selectedRoleId;
        //    var sid = session.selectedServerId;
        //    this.AddLine("uid:" + uid);
        //    this.AddLine("sid:" + sid);
        //}

        var me = Database.Stuff.roleDatabase.Me;
        if(me != null)
        {
            var uid = me._id;
            this.AddLine("uid: " + uid);

            var guid = me.guid;
            this.AddLine("guid: " + guid);

            var sid = me.sid;
            this.AddLine("sid: " + sid);
        }

        var fps = 1.0f / Time.smoothDeltaTime;
        this.AddLine("fps: " + fps.ToString("0"));

        if(StaticDataRuntime.dataFilled)
        {
            var staticDataVersion = StaticData.MetaTable["version"];
            this.AddLine("staticData: " + staticDataVersion);
        }

        var lockstepSession = LockstepRoomClient.Stuff.session;
        if(lockstepSession != null)
        {
            var targetRoomId = lockstepSession.roomId;
            var isJoiend = lockstepSession.roomJoined;
            if(isJoiend)
            {
                this.AddLine("roomId:" + targetRoomId);
            }
        }

        var topPage = UIEngine.Stuff.Top;
        if(topPage != null)
        {
            var name = topPage.name;
            this.AddLine("page:" + name);
        }


        var channel = ChannelManager.Channel.GetType().Name;
        this.AddLine("channel:" + channel);

        var language = LanguageManager.Language;
        this.AddLine("language:" + language);

        var localizeLanguage = LocalizationManager.Stuff.Language;
        this.AddLine("localizeLanguage:" + localizeLanguage);

        if(LanguageDataRuntime.dataFilled)
        {
            var languageDataVersion = LanguageData.LanguageMetaTable.TryGet("version", "-1");
            this.AddLine("languageDataVersion:" + languageDataVersion);
        }

        var w = Screen.width;
        var h = Screen.height;
        this.AddLine("resolution: " + w + " x " + h);

        var mp4MaxSize = VideoUtil.Mp4MaxSupportSize;
        if(mp4MaxSize != Vector2.zero)
        {
            this.AddLine($"mp4MaxSize: {mp4MaxSize}");
        }
        else
        {
            this.AddLine($"mp4MaxSize: Unknown");
        }


        var mainBucketAquireCount = BucketManager.Stuff.Main.AssetsCount;
        this.AddLine("main bucket:" + mainBucketAquireCount);
        this.AddLine("按 z 关闭");

        // 输出字符串到显示
        var content = this.GetString();
        this.text_status.text = content;
    }

    float lastUpdateTime;
    const float INTERVAL = 1f;
    private void Update()
    {
        var now = Time.time;
        var delta = now - lastUpdateTime;
        if(delta >= INTERVAL)
        {
            lastUpdateTime = now;
            this.Refresh();
        }
    }
}
