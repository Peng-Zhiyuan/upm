using System.Collections;
using System.Collections.Generic;
using CustomLitJson;
using UnityEngine;
using UnityEngine.UI;

class EnvInfo {
    public static string Language {
        get {
            var language = EnvManager.GetConfigOfFinalEnv ("language");
            return language;
        }
    }

}