using Plot.Runtime;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotRuntimeMaskCacheInfo
    {
        public GameObject FragPictureCacheObj;
        public GameObject MaskFrameCacheObj;
        public GameObject RawImgCacheObj;
        public GameObject MaskPictureObj;
        public PlotMaskFrameConfigTaskData MaskConfigData;
    }

    public class PlotRuntimeMaskCacheManager
    {
        // 最新分镜的mask得缓存数据
        public static PlotRuntimeMaskCacheInfo MaskCacheInfo { get; private set; }

        public static void SaveMaskCacheObj(PlotMaskFrameConfigTaskData data, GameObject obj1, GameObject obj2,
            GameObject obj3, GameObject obj4)
        {
            MaskCacheInfo = new PlotRuntimeMaskCacheInfo()
            {
                FragPictureCacheObj = obj3,
                MaskFrameCacheObj = obj2,
                RawImgCacheObj = obj1,
                MaskPictureObj = obj4,
                MaskConfigData = data,
            };
        }

        public static void ClearMaskCacheInfo()
        {
            MaskCacheInfo = null;
        }
    }
}