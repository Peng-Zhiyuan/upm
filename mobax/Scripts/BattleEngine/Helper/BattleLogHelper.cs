using UnityEngine;

public class BattleLog
{
    public static bool isOpenBuffLog = true;

    public static void Log(string log)
    {
#if !SERVER
        if (isOpenBuffLog)
        {
            Debug.Log("[BattleLogic] : " + log);
        }
#endif
    }

    public static void LogWarning(string log)
    {
#if !SERVER
        if (isOpenBuffLog)
        {
            Debug.LogWarning("[BattleLogic] : " + log);
        }
#endif
    }

    public static void LogError(string log)
    {
#if !SERVER
        if (isOpenBuffLog)
        {
            Debug.LogError("[BattleLogic] : " + log);
        }
#endif
    }
}