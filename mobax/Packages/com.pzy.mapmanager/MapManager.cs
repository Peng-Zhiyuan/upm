using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System;

public class MapManager : StuffObject<MapManager>
{
    public Dictionary<string, Map> nameToMapDic = new Dictionary<string, Map>();
    Map _selectedMap;
    public Map SelectedMap{
        get{
            return _selectedMap;
        }
        set{
            if(_selectedMap != value){
                if(_selectedMap != null){
                    _selectedMap.OnDeselect();
                }
                if(value != null){
                    value.OnSelect();
                }
            }
            _selectedMap = value;
        }
    }

    [ShowInInspector]
    public string UnityActivedSceneName
    {
        get
        {
            var scene = SceneManager.GetActiveScene();
            return scene.name;
        }
    }

    public async Task LoadAsync<T>() where T : Map
    {
        var mapType = typeof(T);
        var sceneName = mapType.Name;

        await LoadAsync<T>(sceneName);
    }

    public async Task LoadAsync<T>(string mapName) where T : Map
    {

        var isLoaded = IsMapAlreadyLoaded(mapName);
        if (isLoaded)
        {
            throw new Exception($"[MapManager] Map Scene {mapName} already laoded");
        }


        var operation = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Additive);
        var task = AsyncOperationToTask(operation);
        await task;
        var map = GetOrCreateMapComponent<T>(mapName);

        nameToMapDic[mapName] = map;
        SelectedMap = map;

        Debug.Log("[MapManager] load map: " + mapName);
    }

    T GetOrCreateMapComponent<T>(string sceneName) where T : Map
    {
        var mapGo = GameObject.Find(sceneName);
        if (mapGo == null)
        {
            //throw new Exception($"[MapManager] Map Scene {sceneName} not contains a Root GameObject named {sceneName}");
            mapGo = new GameObject(sceneName);
            mapGo.name = sceneName;
        }
        var map = mapGo.GetComponent<T>();
        if (map == null)
        {
            //throw new Exception($"[MapManager] Map Scene {sceneName} not contains a Root GameObject named {sceneName} with samename Compoennt");
            map = mapGo.AddComponent<T>();
        }
        return map;
    }

    public void SelectMap<T>() where T : Map
    {
        var type = typeof(T);
        var mapName = type.Name;
        var map = nameToMapDic[mapName];
        this.SelectMap(map);
    }

    

    public void SelectMap(Map map)
    {
        var scene = map.gameObject.scene;
        SceneManager.SetActiveScene(scene);
        SelectedMap = map;
    }

    public async Task Unload<T>() where T : Map
    {
        var type = typeof(T);
        var mapName = type.Name;
        await Unload(mapName);

    }

    public async Task Unload(Map map)
    {
        var type = map.GetType();
        var mapName = type.Name;
        await Unload(mapName);
    }

    public async Task Unload(string mapName)
    {
        var isLoaded = IsMapAlreadyLoaded(mapName);
        if(!isLoaded)
        {
            throw new Exception($"[MapManager] Map Scene {mapName} already laoded");
        }

        var operation = SceneManager.UnloadSceneAsync(mapName);
        await AsyncOperationToTask(operation);
        this.nameToMapDic.Remove(mapName);
    }

    bool IsMapAlreadyLoaded(string mapName)
    {
        if(nameToMapDic.ContainsKey(mapName))
        {
            return true;
        }
        return false;
    }

    Task AsyncOperationToTask(AsyncOperation operation)
    {
        var tcs = new TaskCompletionSource<bool>();
        operation.completed += res =>
        {
            tcs.SetResult(true);
        };
        return tcs.Task;
    }
}
 