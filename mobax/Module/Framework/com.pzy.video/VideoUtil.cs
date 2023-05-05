using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;

public static class VideoUtil 
{
    static Vector2? _mp4MaxSupportSize;

    /// <summary>
    /// 获取支持的 mp4 格式的最大分辨率，
    /// 返回 zero 表示未知
    /// </summary>
    public static Vector2 Mp4MaxSupportSize
    {
        get
        { 
            if(_mp4MaxSupportSize == null)
            {
                if(Application.platform == RuntimePlatform.Android)
                {
                    try
                    {
                        var json = NativeBridge.Call("VideoProxy", "GetMax");
                        var jd = JsonMapper.Instance.ToObject(json);
                        var w = jd.TryGet<int>("width");
                        var h = jd.TryGet<int>("height");
                        _mp4MaxSupportSize = new Vector2(w, h);
                    }
                        catch
                    {
                        _mp4MaxSupportSize = Vector2.zero;
                    }
            }
                else
                {
                    _mp4MaxSupportSize = Vector2.zero;
                }
            }
            return _mp4MaxSupportSize.Value;
        }
    }
}
