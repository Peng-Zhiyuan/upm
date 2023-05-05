using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeInfo 
{
   public static string PlatformName
   {
       get
       {
            if(Application.platform == RuntimePlatform.Android)
            {
               return "android";
            }
            else if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return "ios";
            }
            else 
            {
                return "editor";
            }
       }
   }
}
