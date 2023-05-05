using UnityEngine.ResourceManagement.ResourceProviders;

namespace Plot.Runtime
{
    public class PlotRuntimeSceneCacheInfo
    {
        public bool IsActive = false;
        public SceneInstance CacheSceneInstance;
    }
    
    public class PlotRuntimeSceneCacheManager
    {
        private static PlotRuntimeSceneCacheInfo _sceneCacheInfo;
        public static PlotRuntimeSceneCacheInfo SceneCacheInfo => _sceneCacheInfo;

        public static void SaveCacheSceneInstance(SceneInstance sceneInstance)
        {
            _sceneCacheInfo = new PlotRuntimeSceneCacheInfo()
            {
                IsActive = true,
                CacheSceneInstance = sceneInstance,
            };
        }

        public static void RemoveCacheSceneInstance()
        {
            _sceneCacheInfo = new PlotRuntimeSceneCacheInfo()
            {
                IsActive = false,
            };
        }
    }
}