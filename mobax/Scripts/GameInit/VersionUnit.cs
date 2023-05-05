using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VersionUnit : StuffObject<VersionUnit>
{
    //public string GetFullVersion()
    //{
    //    var version = GameManifestManager.Get("version", "0.0.0");
    //    var build = GameManifestManager.Get("build", "0");
    //    var remoteConf = RemoteConfManager.Instance.Conf;
    //    string languageName = Localization.Language;
    //    var staticDataVersion = remoteConf["staticDataVersion"];
    //    string dataName = $"staticDataVersion_{languageName}";
    //    if (remoteConf.HasKey(dataName))
    //    {
    //        staticDataVersion = remoteConf[dataName];
    //    }
    //    return $"v{version}.b{build}.d{staticDataVersion}";
    //}

    public void RefreshView()
    {
        //var versionText = this.GetComponent<Text>();
        //versionText.text = GameUtil.GetFullVersion();
    }



    private int index = 0;
    public void OnVersion()
    {
        index++;
        if(index > 20 || (DeveloperLocalSettings.IsDevelopmentMode && index > 5))
        {
            //IEngine.Stuff.Forward<DashboardPage>();
        }
    }
}
