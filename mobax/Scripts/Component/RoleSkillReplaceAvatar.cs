using System;
using System.Collections.Generic;
using UnityEngine;

public class RoleSkillReplaceAvatar : MonoBehaviour
{
    /// <summary>
    /// 大招时候需要开启的对象
    /// </summary>
    public List<GameObject> SPSTypeMeshType = new List<GameObject>();

    public List<GameObject> NormalMeshType = new List<GameObject>();

    public SKIN_TYPE SwitchSkinType;
    public string SwitchSkinPath;

    public bool isExecuteAvatar = false;

    private void Awake()
    {
        isExecuteAvatar = false;
        ResetAvatarChange();
    }

    public void ExcuteAvatarChange()
    {
        isExecuteAvatar = true;
        for (int i = 0; i < SPSTypeMeshType.Count; i++)
        {
            SPSTypeMeshType[i].SetActive(true);
        }
        for (int i = 0; i < NormalMeshType.Count; i++)
        {
            NormalMeshType[i].SetActive(false);
        }
        RoleRender roleRender = this.gameObject.GetComponent<RoleRender>();
        if (roleRender != null
            && !string.IsNullOrEmpty(SwitchSkinPath))
        {
            roleRender.SwitchSkin(SwitchSkinType, SwitchSkinPath);
        }
    }

    public void ResetAvatarChange()
    {
        for (int i = 0; i < SPSTypeMeshType.Count; i++)
        {
            SPSTypeMeshType[i].SetActive(false);
        }
        for (int i = 0; i < NormalMeshType.Count; i++)
        {
            NormalMeshType[i].SetActive(true);
        }
        if (isExecuteAvatar)
        {
            RoleRender roleRender = this.gameObject.GetComponent<RoleRender>();
            if (roleRender != null)
            {
                roleRender.ResetHeroSkin();
            }
            isExecuteAvatar = false;
        }
    }
}