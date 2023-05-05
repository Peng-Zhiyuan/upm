using UnityEngine;

public static class LauncherLoadingHelper
{
    public static bool BuildRewards(LauncherSimpleItem itemPrefab, Transform parentNode)
    {
        var rewardsString = EnvManager.GetConfigOfFinalEnv("rewards");
        Debug.Log($"【rewardsString】{rewardsString}");
        var available = !string.IsNullOrEmpty(rewardsString);
        if (!available) return false;
        
        var items = rewardsString.Split(';');
        foreach (var itemStr in items)
        {
            if (string.IsNullOrEmpty(itemStr)) continue;
                
            var itemInfos = itemStr.Split(',');
            int.TryParse(itemInfos[0], out var itemId);
            int.TryParse(itemInfos[1], out var count);
            if (itemInfos.Length <= 2 || !int.TryParse(itemInfos[2], out var rarity))
            {
                rarity = 4;
            }
            var itemInstance = Object.Instantiate(itemPrefab, parentNode);
            itemInstance.Set(itemId, count, rarity);
        }

        return true;
    }
}