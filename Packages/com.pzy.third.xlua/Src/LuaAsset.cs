using System;
using System.Security.Cryptography;
using UnityEngine;
using System.Text;

[Serializable]
public class LuaAsset : ScriptableObject
{
    public static string LuaDecodeKey = "LuaDecodeKey"; //TODO: use a safe method to hide decode key
    public static string[] LuaSearchingPaths = new []{
        "lua/",
        "lua/utility/",
    };
    
    public bool encode = true;
    public byte[] data;
    
    public byte[] GetDecodeBytes()
    {
        // TODO: your decode function
        var decode = encode ? Security.XXTEA.Decrypt(this.data, LuaAsset.LuaDecodeKey) : this.data;

        var ret = RemoveBom(decode);

        return ret;
    }

    byte[] RemoveBom(byte[] data)
    {
        var length = data.Length;
        if (length >= 3)
        {
            var first = data[0];
            var second = data[1];
            var third = data[2];
            if (first == 239 && second == 187 && third == 191)
            {
                var ret = new byte[length - 3];
                for (int i = 3; i < length; i++)
                {
                    var _byte = data[i];
                    var index = i - 3;
                    ret[index] = _byte;
                }
                return ret;
            }
        }
        return data;
    }

    public string GetDecodedString()
    {
        var decoded = GetDecodeBytes();
        var code = Encoding.UTF8.GetString(decoded);
        return code;
    }
    
    public static byte[] Require(ref string luapath)
    {
        return Require(luapath);
    }

    public static byte[] Require(string luapath, string search = "", int retry = 0)
    {
        if(string.IsNullOrEmpty(luapath))
            return null;
            
        var LuaExtension = ".lua";

        if(luapath.EndsWith(LuaExtension))
        {
            luapath = luapath.Remove(luapath.LastIndexOf(LuaExtension));
        }

        byte[] bytes = null;
        var assetName = search + luapath.Replace(".", "/") + LuaExtension;
        {
            //TODO: your bundle load method
            // var asset = AssetSys.GetAssetSync<LuaAsset>(assetName);
            var asset = Resources.Load<LuaAsset>(assetName);
            if (asset != null)
            {
                bytes = asset.GetDecodeBytes();
            }
        }

        // try next searching path
        if(bytes == null && retry < LuaSearchingPaths.Length)
        {
            bytes = Require(luapath, LuaSearchingPaths[retry], 1+retry);
        }
        
        Debug.Assert(bytes != null, $"{luapath} not found.");
        return bytes;
    }
}