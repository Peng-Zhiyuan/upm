using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class VersionChecker
{
    private static bool IsBigger(string ver1, string ver2)
    {
        var parts1 = ver1.Split('.');
        var parts2 = ver2.Split('.');
        if(parts1.Length != 3 || parts2.Length != 3) return false;
        for(int i = 0; i < 3; i++)
        {
            int v1 = int.Parse(parts1[i]);
            int v2 = int.Parse(parts2[i]);
            if(v1 == v2)continue;
            else return v1 > v2;
        }
        return false;
        
    }
   public static async Task CheckNewVersionAsync()
   {
        //string validGameVersion = RemoteConfManager.Instance.Conf.TryGet("version","");
        //bool forceUpdate = RemoteConfManager.Instance.Conf.TryGet("forceUpdate","false") == "true";
        //string curGameVersion = GameManifestManager.Get("version");
       
        ////Debug.Log("[VersionChecker] validGameVersion:"+validGameVersion +   "    curGameVersion:"+curGameVersion);  
        //if(IsBigger(validGameVersion,curGameVersion))
        //{
        //    //bool result = await Dialog.AskAsync(BaseDialogStyle.ConfirmCancel,LocalizationManager.Stuff.GetText("message"),LocalizationManager.Stuff.GetText("detect_new_version"));
        //    //if(result)
        //    //{
        //    //    string url = RemoteConfManager.Instance.Conf["url."+RuntimeInfo.PlatformName].ToString();
        //    //    if(!string.IsNullOrEmpty(url))Application.OpenURL(url);
        //    //    throw new SilentException("client version out date");
        //    //}
        //    //else
        //    //{
        //    //    if(forceUpdate)
        //    //    {
        //    //        throw new SilentException("client version out date");
        //    //    }
        //    //    return;   
        //    //}
        //}
   }
}
