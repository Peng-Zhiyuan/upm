using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPlatformInfo 
{
    public string guid;
    public int[] gzone;
    public string token;
    public string bind;

     /// <summary>
     /// 最后一次选择的 sid
     /// </summary>
     public int LastSelectedSid
     {
        get
        {
            if(this.guid == null)
            {
                return -1;
            }
            if (this.gzone.Length == 0)
            {
                return -1;
            }
            else
            {
                var index = this.gzone.Length - 1;
                var sid = this.gzone[index];
                return sid;
            }
        }
     }

}
