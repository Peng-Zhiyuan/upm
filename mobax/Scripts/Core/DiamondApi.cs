using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public class DiamondApi : MonoBehaviour
{
    public static async Task ExchangeAsync(JsonData pageArg)
    {
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "business/diamond/exchange", pageArg, isAutoShowReward: DisplayType.Show);
    }


}
