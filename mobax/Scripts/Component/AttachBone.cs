using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class AttachItem
{
    public string root;
    public string child;
}

[ExecuteAlways]
public class AttachBone : MonoBehaviour
{
    public Dictionary<string, Transform> m_Bones = new Dictionary<string, Transform>();
    public Dictionary<string, Transform> m_BoneSlots = new Dictionary<string, Transform>();
    public Dictionary<string, Transform> m_WeaponBones = new Dictionary<string, Transform>();

    public GameObject Part;
    public List<AttachItem> BindList = new List<AttachItem>();
    public GameObject Hair;
    public List<AttachItem> BindList2 = new List<AttachItem>();

    public Dictionary<string, string> BoneNode = new Dictionary<string, string>();

    public GameObject FacePart;

    public void Init()
    {
#if UNITY_EDITOR
        if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(this.gameObject))
        {
            return;
        }
#endif
        Clear();
        this.gameObject.SetActive(false);
        RefreshCachedBones(this.gameObject);
        Attach();
        if (GetComponent<RoleRender>() != null)
        {
            GetComponent<RoleRender>().RefreshRenderList(true);
        }
        this.gameObject.SetActive(true);
    }

    private void RefreshCachedBones(GameObject rootBone)
    {
        Transform tmp_boneRoot = rootBone.transform;
        m_Bones.Clear();
        AttachList.Clear();
        var tmp_bones = ListPool.ListPoolN<Transform>.Get();
        tmp_boneRoot.GetComponentsInChildren(tmp_bones);
        m_Bones.Add("root", tmp_boneRoot);
        foreach (var tmp_bone in tmp_bones)
        {
            var boneName = tmp_bone.name.ToLower();
            if (boneName.StartsWith("bip")
                || boneName.StartsWith("bone")
                || boneName.StartsWith("b_")
                || boneName.StartsWith("body_"))
            {
                if (m_Bones.ContainsKey(boneName))
                {
                    continue;
                }
                m_Bones.Add(boneName, tmp_bone);
            }
        }
        ListPool.ListPoolN<Transform>.Release(tmp_bones);
    }

    private void RefreshCachedBones_New(GameObject rootBone, Dictionary<string, Transform> dic)
    {
        Transform tmp_boneRoot = rootBone.transform;
        dic.Clear();
        var tmp_bones = ListPool.ListPoolN<Transform>.Get();
        tmp_boneRoot.GetComponentsInChildren(tmp_bones);
        dic.Add("root", tmp_boneRoot);
        foreach (var tmp_bone in tmp_bones)
        {
            var boneName = tmp_bone.name.ToLower();
            if (boneName.StartsWith("bip")
                || boneName.StartsWith("bone")
                || boneName.StartsWith("b_")
                || boneName.StartsWith("body_")
                || boneName.StartsWith("hair"))
            {
                if (dic.ContainsKey(boneName))
                {
                    continue;
                }
                dic.Add(boneName, tmp_bone);
            }
        }
        ListPool.ListPoolN<Transform>.Release(tmp_bones);
    }

    public Transform GetBone(string bone)
    {
        if (bone == "foot")
            return transform;
        if (string.IsNullOrEmpty(bone))
            bone = "root";
        Transform trans = null;
        if (!m_Bones.TryGetValue(bone.ToLower(), out trans))
        {
            BattleLog.LogError("缺少骨骼：" + bone);
        }
        return trans;
    }

    [SerializeField]
    private List<Transform> AttachList = new List<Transform>();

    public void Attach()
    {
        AttachToBone(Part, BindList);
        AttachToBone(Hair, BindList2);
    }

    public void AttachToBone(GameObject part, List<AttachItem> bindList)
    {
        if (part == null)
        {
            return;
        }
        var model = Instantiate(part);
        model.name = part.name;
        TransformUtil.InitTransformInfo(model, this.transform);
        AttachList.Add(model.transform);
        RefreshCachedBones_New(model, m_WeaponBones);
        foreach (var VARIABLE in bindList)
        {
            Transform trans;
            if (m_WeaponBones.TryGetValue(VARIABLE.child.ToLower(), out trans))
            {
                TransformUtil.InitTransformInfo(trans.gameObject, GetBone(VARIABLE.root));
                AttachList.Add(trans);
            }
            else
            {
                BattleLog.LogError(Part.name + "缺少骨骼 + " + VARIABLE.child);
            }
        }
    }

    public void AnimationEventShowWeapon(string param)
    {
        if (!string.IsNullOrEmpty(param))
        {
            string[] wpList = param.Split(',');
            for (int i = 0; i < wpList.Length; i++)
            {
                if (string.IsNullOrEmpty(wpList[i]))
                {
                    continue;
                }
                for (int j = 0; j < AttachList.Count; j++)
                {
                    if (AttachList[j].name == wpList[i])
                    {
                        AttachList[j].SetActive(true);
                    }
                }
            }
        }
        else
        {
            if (Part != null
                && (Part.name.Contains("wp_") || Part.name.Contains("_weapon_")))
            {
                for (int i = 0; i < AttachList.Count; i++)
                {
                    if (AttachList[i].name == Part.name)
                    {
                        AttachList[i].SetActive(true);
                    }
                }
            }
        }
    }

    public void AnimationEventHideWeapon(string param)
    {
        if (!string.IsNullOrEmpty(param))
        {
            string[] wpList = param.Split(',');
            for (int i = 0; i < wpList.Length; i++)
            {
                if (string.IsNullOrEmpty(wpList[i]))
                {
                    continue;
                }
                for (int j = 0; j < AttachList.Count; j++)
                {
                    if (AttachList[j].name == wpList[i])
                    {
                        AttachList[j].SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (Part != null
                && (Part.name.Contains("wp_") || Part.name.Contains("_weapon_")))
            {
                for (int i = 0; i < AttachList.Count; i++)
                {
                    if (AttachList[i].name == Part.name)
                    {
                        AttachList[i].SetActive(false);
                    }
                }
            }
        }
    }

    public void OnDestroy()
    {
        Clear();
    }

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            AttachNode();
#endif
    }

    [ShowInInspector]
    public void AttachNode()
    {
        Init();
    }

    [ShowInInspector]
    public void Clear()
    {
        foreach (var VARIABLE in AttachList)
        {
            if (VARIABLE == null
                || VARIABLE.gameObject == null) continue;
            DestroyImmediate(VARIABLE.gameObject);
        }
        AttachList.Clear();
#if UNITY_EDITOR
        if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(this.gameObject))
        {
            if (this.Part != null)
            {
                string partName = this.Part.gameObject.name;
                var go = this.transform.FindInChildren(partName);
                while (go != null)
                {
                    BattleLog.Log("移除附件:" + go.name);
                    DestroyImmediate(go);
                    go = this.transform.FindInChildren(partName);
                }
                string partCloneName = this.Part.gameObject.name + "(Clone)";
                var goClone = this.transform.FindInChildren(partCloneName);
                while (goClone != null)
                {
                    BattleLog.Log("移除Clone附件:" + goClone.name);
                    DestroyImmediate(goClone);
                    goClone = this.transform.FindInChildren(partCloneName);
                }
            }
            foreach (var VARIABLE in BindList)
            {
                var go = this.transform.FindInChildren(VARIABLE.child);
                while (go != null)
                {
                    BattleLog.Log("移除附件:" + go.name);
                    DestroyImmediate(go);
                    go = this.transform.FindInChildren(VARIABLE.child);
                }
            }
        }
#endif
    }
}