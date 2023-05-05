using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioEngine : StuffObject<AudioEngine>, IUpdatable
{
    private GameObject root;

    public float ListenerVolume
    {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }

    void Awake()
    {
        root = new GameObject("AudioServiceRoot");
        //root.AddComponent<AudioListener> ();
        UpdateManager.Stuff.Add(this);
        GameObject.DontDestroyOnLoad(root);
    }

    List<AudioTask> audioTaskList = new List<AudioTask>();

    public void PauseGroup(string group)
    {
        foreach (var one in audioTaskList)
        {
            if (one.group == group)
            {
                one.player.Pause();
                one.isPaused = true;
            }
        }
    }

    public void ResumeGroup(string group)
    {
        foreach (var one in audioTaskList)
        {
            if (one.group == group)
            {
                one.player.UnPause();
                one.isPaused = false;
            }
        }
    }

    public void StopGroup(string group)
    {
        foreach (var one in audioTaskList)
        {
            if (one.group == group)
            {
                one.destroy = true;
            }
        }
    }

    class AudioTask
    {
        public Parameter parameter;
        public AudioClip clip;
        public AudioSource player;
        public float deadTime;
        public bool registerInSingleDic;
        public bool destroy;
        public float volume;
        public string group; //播放音效的类型
        public bool isPaused;
        public bool isLoop;
    }

    public void OnUpdate()
    {
        frameOnceDic.Clear();
        for (int i = audioTaskList.Count - 1; i >= 0; i--)
        {
            var task = audioTaskList[i];
            var isPuased = task.isPaused;
         
            if (isPuased)
            {
                task.deadTime += Time.deltaTime;
                continue;
            }

            if ((Time.time > task.deadTime && !task.isLoop)|| task.destroy)
            {
                task.player.gameObject.SetActive(false);
                PutPlayer(task.player);
                audioTaskList.RemoveAt(i);
                if (task.registerInSingleDic)
                {
                    singleDic.Remove(task.clip);
                }

                PutAudioTask(task);
            }
        }
    }

    Dictionary<string, bool> groupToEnabledDic = new Dictionary<string, bool>();

    public bool GetEnabled(string group)
    {
        var b = groupToEnabledDic.TryGetValue(group, out var enabled);
        if (b)
        {
            return enabled;
        }
        else
        {
            var intValue = PlayerPrefs.GetInt($"AudioEngine.{group}.enabled", 1);
            var cached = intValue == 1;
            groupToEnabledDic[group] = cached;
            return cached;
        }
    }

    public void SetEnabeled(string group, bool enabled)
    {
        groupToEnabledDic[group] = enabled;
        PlayerPrefs.SetInt($"AudioEngine.{group}.enabled", enabled ? 1 : 0);
        if (!enabled)
        {
            this.StopGroup(group);
        }
    }

    Dictionary<string, float> groupToVolumeDic = new Dictionary<string, float>();

    public void DeleteCacheForGroup(string group)
    {
        PlayerPrefs.DeleteKey($"AudioEngine.{group}.volume");
        groupToVolumeDic.Remove(group);
    }

    public float GetGroupVolume(string group)
    {
        var b = groupToVolumeDic.TryGetValue(group, out var volume);
        if (b)
        {
            return volume;
        }
        else
        {
            var defaultValueStr = GameManifestManager.GetInLocal($"audioEngine.{group}.default", "1");
            var defaultValue = float.Parse(defaultValueStr);
            var v = PlayerPrefs.GetFloat($"AudioEngine.{group}.volume", defaultValue);
            groupToVolumeDic[group] = v;
            return v;
        }
    }

    public void SetGroupVolume(string group, float volume)
    {
        groupToVolumeDic[group] = volume;
        PlayerPrefs.SetFloat($"AudioEngine.{group}.volume", volume);
        this.RecalculateRoutingFinalVolum();
    }

    /// <summary>
    /// 获取当前播放的bgm的名称
    /// </summary>
    /// <returns></returns>
    public string GetCurrentBGMName()
    {
        if (this.audioTaskList.Count <= 0) return default;

        var bgm = this.audioTaskList.Find(val => val.@group == AudioType.Bgm);
        return bgm?.clip.name;
    }

    public void RecalculateRoutingFinalVolum()
    {
        foreach (var task in this.audioTaskList)
        {
            var player = task.player;
            var group = task.group;
            var groupVolume = GetGroupVolume(group);
            player.volume = task.volume * groupVolume;
        }
    }


    Dictionary<AudioClip, AudioTask> singleDic = new Dictionary<AudioClip, AudioTask>();
    Dictionary<AudioClip, AudioTask> frameOnceDic = new Dictionary<AudioClip, AudioTask>();

    public void Play(AudioClip clip, float volume = 1.0f, Parameter parameter = Parameter.MutiTask, string group = "default")
    {
        var isGroupEnabled = GetEnabled(group);
        if (!isGroupEnabled)
        {
            return;
        }

        if (clip == null)
        {
            return;
        }

        if (parameter == Parameter.SingleTaskEarly)
        {
            AudioTask task2;
            singleDic.TryGetValue(clip, out task2);
            if (task2 != null)
            {
                return;
            }
        }
        else if (parameter == Parameter.SingleTaskLater)
        {
            AudioTask task2;
            singleDic.TryGetValue(clip, out task2);
            if (task2 != null)
            {
                task2.player.Stop();
                task2.registerInSingleDic = false;
                task2.destroy = true;
            }
        }
        else if (parameter == Parameter.FrameOnce)
        {
            AudioTask task2;
            frameOnceDic.TryGetValue(clip, out task2);
            if (task2 != null)
            {
                return;
            }
        }
        bool isLoop = group == AudioType.Bgm;
        var groupVolume = GetGroupVolume(group);
        var player = TakePlayer(clip.name);
        player.clip = clip;

        player.loop = isLoop;
        player.volume = volume * groupVolume;
        player.Play();

        var task = TakeAudioTask();
        task.player = player;
        task.clip = clip;
        task.isLoop = isLoop;
        task.deadTime = Time.time + clip.length;
        task.destroy = false;
        task.registerInSingleDic = false;
        task.volume = volume;
        task.group = group;

        audioTaskList.Add(task);

        if (parameter == Parameter.SingleTaskEarly || parameter == Parameter.SingleTaskLater)
        {
            task.registerInSingleDic = true;
            singleDic[clip] = task;
        }
        else if (parameter == Parameter.FrameOnce)
        {
            frameOnceDic[clip] = task;
        }
    }

    public enum Parameter
    {
        /// <summary>
        /// 允许多实例播放
        /// </summary>
        MutiTask,

        /// <summary>
        /// 只允许单一实例播放，如果正在播放时再次请求播放，请求会被无视
        /// </summary>
        SingleTaskEarly,

        /// <summary>
        /// 只允许单一实例播放，如果正在播放时再次请求播放，会先停止已播放的实例
        /// </summary>
        SingleTaskLater,

        /// <summary>
        /// 每一帧只允许一个实例播放
        /// </summary>
        FrameOnce,
    }

    AudioSource NewPlayer(string name)
    {
        var obj = new GameObject();
        var player = obj.AddComponent<AudioSource>();
        obj.transform.parent = root.transform;
        player.name = name;
        return player;
    }

    Queue<AudioTask> audioTaskPool = new Queue<AudioTask>();

    AudioTask TakeAudioTask()
    {
        if (audioTaskPool.Count > 0)
        {
            return audioTaskPool.Dequeue();
        }
        else
        {
            return new AudioTask();
        }
    }

    void PutAudioTask(AudioTask task)
    {
        audioTaskPool.Enqueue(task);
    }

    Queue<AudioSource> playerPool = new Queue<AudioSource>();

    AudioSource TakePlayer(string name)
    {
        if (playerPool.Count > 0)
        {
            var p = playerPool.Dequeue();
            p.name = name;
            p.gameObject.SetActive(true);
            return p;
        }
        else
        {
            return NewPlayer(name);
        }
    }

    void PutPlayer(AudioSource player)
    {
        playerPool.Enqueue(player);
    }
}