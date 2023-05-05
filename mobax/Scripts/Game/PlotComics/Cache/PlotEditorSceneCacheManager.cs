using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Plot.Runtime
{
    public class PlotEditorSceneCacheInfo
    {
        public bool IsActive = false;
        public Scene CacheScene;
    }

    public class PlotEditorSceneInstanceCacheInfo
    {
        public bool IsActive = false;
        public SceneInstance CacheSceneInstance;
    }

    public class PlotEditorSceneCacheManager
    {
        private static PlotEditorSceneInstanceCacheInfo _sceneInstanceCacheInfo;
        public static PlotEditorSceneInstanceCacheInfo SceneInstanceCacheInfo => _sceneInstanceCacheInfo;

        public static void SaveCacheSceneInstanceCache(SceneInstance sceneInstance)
        {
            _sceneInstanceCacheInfo = new PlotEditorSceneInstanceCacheInfo()
            {
                IsActive = true,
                CacheSceneInstance = sceneInstance,
            };
        }

        public static void RemoveCacheSceneInstanceCache()
        {
            _sceneInstanceCacheInfo = new PlotEditorSceneInstanceCacheInfo()
            {
                IsActive = false,
            };
        }

        private static PlotEditorSceneCacheInfo _sceneCacheInfo;
        public static PlotEditorSceneCacheInfo SceneCacheInfo => _sceneCacheInfo;

        public static void SaveCacheSceneCache(Scene scene)
        {
            _sceneCacheInfo = new PlotEditorSceneCacheInfo()
            {
                IsActive = true,
                CacheScene = scene,
            };
        }

        public static void RemoveCacheSceneCache()
        {
            _sceneCacheInfo = new PlotEditorSceneCacheInfo()
            {
                IsActive = false,
            };
        }
    }
}