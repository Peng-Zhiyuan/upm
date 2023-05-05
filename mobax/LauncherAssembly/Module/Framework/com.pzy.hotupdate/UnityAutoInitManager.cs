using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System.Reflection;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public static class UnityAutoInitManager 
{
    public static async Task AutoCallAsync()
    {

        Debug.Log("[UnityAutoInitManager] call unit auto init");
        var asset = await Addressables.LoadAssetAsync<TextAsset>("RuntimeInitializeOnLoads.json").Task;
        var json = asset.text;
        var infoList = JsonMapper.Instance.ToObject<List<AutoInitInfo>>(json);

        foreach (var info in infoList)
        {
            var assemblyName = info.assemblyName;
            var className = info.className;
            var methodName = info.methodName;
            var nameSpace = info.nameSpace;


            string fullName = className;
            if (!string.IsNullOrEmpty(nameSpace))
            {
                fullName = $"{nameSpace}.{className}";
            }
            Debug.Log($"[UnityAutoInitManager] invoke {fullName}.{methodName}() ");
            var assembly = ReflectionUtil.GetAssembly(assemblyName);
            if(assembly == null)
            {
                Debug.Log("[UnityAutoInitManager] fail, not found assembly:" + assemblyName);
                continue;
            }
            var type = assembly.GetType(fullName);
            if(type == null)
            {
                Debug.Log("[UnityAutoInitManager] fail, not found type:" + fullName);
                continue;
            }
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if(method == null)
            {
                Debug.Log("[UnityAutoInitManager] fail, not found method:" + methodName);
                continue;
            }
            method.Invoke(null, new object[] { });

        }
    }

}


public class AutoInitInfo
{
    public string assemblyName;
    public string nameSpace;
    public string className;
    public string methodName;
}