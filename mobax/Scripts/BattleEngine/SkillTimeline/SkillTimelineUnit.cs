using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using BattleEngine.View;
using UnityEditor;
using UnityEngine.Playables;

public class SkillTimelineUnit : RecycledGameObject
{
    public GameObject CharactRoot;
    public List<GameObject> AttachFx = new List<GameObject>();
    [HideInInspector]
    public PlayableDirector Director;

    private readonly Dictionary<string, PlayableBinding> bindingDict = new Dictionary<string, PlayableBinding>();

    [HideInInspector]
    public GameObject AvatarObj = null;

    public async Task PlaySkillTimeLine(string path)
    {
        AvatarObj = await BattleResManager.Instance.LoadAvatarModel(path);
        TransformUtil.InitTransformInfo(AvatarObj, CharactRoot.transform);
        Director = gameObject.GetComponent<PlayableDirector>();
        //开始的时候，储存所有轨道信息，轨道名称作为key，Track作为value，用于动态设置
        foreach (var bind in Director.playableAsset.outputs)
        {
            if (!bindingDict.ContainsKey(bind.streamName))
            {
                bindingDict.Add(bind.streamName, bind);
            }
        }
        SetTrackDynamic("Player", AvatarObj);
        SetTrackDynamic("PlayerAnimation", AvatarObj);
        AttachBone ab = AvatarObj.GetComponent<AttachBone>();
        if (ab != null
            && ab.FacePart != null)
        {
            SetTrackDynamic("PlayerFace", AvatarObj);
            SetTrackDynamic("PlayerFaceAnimation", AvatarObj);
        }
        //await Task.Delay(100);
        for (int i = 0; i < AttachFx.Count; i++)
        {
            if (AttachFx[i] == null)
            {
                continue;
            }
            Transform parentTrans = TransformUtil.FindChild(AvatarObj.transform, AttachFx[i].name);
            if (parentTrans != null)
            {
                TransformUtil.InitTransformInfo(AttachFx[i], parentTrans);
            }
        }
    }

    public void EditorPlaySkillTimeLine(string heroPath)
    {
#if UNITY_EDITOR
        GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arts/SkillTimeline/{0}.prefab".Fmt(heroPath));
#endif
    }

    /// <summary>
    /// 动态设置轨道
    /// </summary>
    /// <param name="trackName"></param>
    /// <param name="gameObject"></param>
    public void SetTrackDynamic(string trackName, GameObject gameObject)
    {
        if (bindingDict.TryGetValue(trackName, out PlayableBinding pb))
        {
            Director.SetGenericBinding(pb.sourceObject, gameObject);
        }
    }

    public void EditorInitTrack(GameObject avatar)
    {
        Director = GetComponent<PlayableDirector>();
        bindingDict.Clear();
        foreach (var bind in Director.playableAsset.outputs)
        {
            if (!bindingDict.ContainsKey(bind.streamName))
            {
                bindingDict.Add(bind.streamName, bind);
            }
        }
        for (int i = 0; i < AttachFx.Count; i++)
        {
            if (AttachFx[i] == null)
            {
                continue;
            }
            Transform parentTrans = TransformUtil.FindChild(avatar.transform, AttachFx[i].name);
            if (parentTrans != null)
            {
                TransformUtil.InitTransformInfo(AttachFx[i], parentTrans);
            }
        }
    }

    public void EditorSetTrackDynamic(string trackName, GameObject gameObject)
    {
        if (bindingDict.TryGetValue(trackName, out PlayableBinding pb))
        {
            Director.SetGenericBinding(pb.sourceObject, gameObject);
        }
    }

#if UNITY_EDITOR

    [ContextMenu("RefreshTimeLine")]
    public void RefreshTimeline()
    {
        if (CharactRoot == null
            || CharactRoot.GetComponentsInChildren<Animator>() == null
            || CharactRoot.GetComponentsInChildren<Animator>(true).Length != 1
            || AttachFx.Count > 0)
        {
            BattleLog.LogError("检查下人物根节点信息！！！！！！！！！！！！！！！！");
            return;
        }
        if (PrefabUtility.IsAnyPrefabInstanceRoot(this.gameObject))
        {
            PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        AttachFx.Clear();
        GameObject obj = CharactRoot.GetComponentInChildren<Animator>().gameObject;
        Fx[] attachFx = obj.transform.GetComponentsInChildren<Fx>(true);
        for (int i = 0; i < attachFx.Length; i++)
        {
            BattleLog.LogWarning("Attach Bone Fx " + attachFx[i].name);
        }
        GameObject FxRoot = GameObject.Find("FxRoot");
        if (FxRoot == null)
        {
            FxRoot = new GameObject();
            FxRoot.name = "FxRoot";
            TransformUtil.InitTransformInfo(FxRoot, transform);
        }
        for (int i = 0; i < attachFx.Length; i++)
        {
            GameObject fxBone = new GameObject();
            TransformUtil.InitTransformInfo(fxBone, FxRoot.transform);
            fxBone.name = attachFx[i].transform.parent.name;
            Vector3 fxLocalPosition = attachFx[i].transform.localPosition;
            Quaternion fxLocalRotation = attachFx[i].transform.localRotation;
            Vector3 fxLocalScale = attachFx[i].transform.localScale;
            TransformUtil.InitTransformInfo(attachFx[i].gameObject, fxBone.transform);
            attachFx[i].transform.localPosition = fxLocalPosition;
            attachFx[i].transform.localRotation = fxLocalRotation;
            attachFx[i].transform.localScale = fxLocalScale;
            AttachFx.Add(fxBone);
        }
    }

    [ContextMenu("ResetTimeLine")]
    public async void RevertTimeline()
    {
        if (PrefabUtility.IsAnyPrefabInstanceRoot(this.gameObject))
        {
            PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        if (CharactRoot == null
            || CharactRoot.GetComponentInChildren<AttachBone>() == null)
        {
            BattleLog.LogError("检查下人物根节点信息！！！！！！！！！！！！！！！！");
            return;
        }
        Director = gameObject.GetComponent<PlayableDirector>();
        foreach (var bind in Director.playableAsset.outputs)
        {
            if (!bindingDict.ContainsKey(bind.streamName))
            {
                bindingDict.Add(bind.streamName, bind);
            }
        }
        AttachBone ab = CharactRoot.GetComponentInChildren<AttachBone>();
        GameObject avatarObj = ab.gameObject;
        SetTrackDynamic("Player", avatarObj);
        SetTrackDynamic("PlayerAnimation", avatarObj);
        if (ab != null
            && ab.FacePart != null)
        {
            SetTrackDynamic("PlayerFace", avatarObj);
            SetTrackDynamic("PlayerFaceAnimation", avatarObj);
        }
        await Task.Delay(100);
        for (int i = 0; i < AttachFx.Count; i++)
        {
            if (AttachFx[i] == null)
            {
                continue;
            }
            Transform parentTrans = TransformUtil.FindChild(avatarObj.transform, AttachFx[i].name);
            if (parentTrans != null)
            {
                TransformUtil.InitTransformInfo(AttachFx[i], parentTrans);
            }
        }
        AttachFx.Clear();
    }
#endif
}