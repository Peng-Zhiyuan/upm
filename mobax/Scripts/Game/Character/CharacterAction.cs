using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Spine.Unity;
using static Spine.AnimationState;
using static SpineUtil;

public class CharacterActionConst
{
    public const string Stand = "stand";
    public const string Idle = "idle";
    public const string Run = "run";
    public const string RunWeapon = "run1";
    public const string Attack = "attack1";
    public const string Walk = "walk";
    public const string Hurt = "hit";
    public const string FightIdle = "idle";
    public const string Win = "win";
    public const string Dead = "dead";
    public const string Wave = "inspire";
    public const string Stun = "coma";
    public const string ShowWeapon = "pullarms";
    public const string FloatUp = "float1";
    public const string FloatHover = "float2";
    public const string FloatDown = "float3";
}

public class AnimParam
{
    public int face;
    public float start;
    public float end;
    public string name;
    public bool flip = false;
    public bool success = true;
    public bool loop;
}

public class CharacterActionController
{
    private Creature _owner = null;

    private string m_LastUpAction = string.Empty;

    private List<Animator> SubAnimtors = new List<Animator>();

#region 初始化和重置
    public void Init(Creature owner)
    {
        _owner = owner;
        // SubAnimtors.AddRange(_owner.Animator.transform.GetComponentsInChildren<Animator>(true));
        //SubAnimtors.RemoveAt(0);

        //InitWeaponInfo();
    }
#endregion

    public void Update(float param_deltaTime) { }

#region 动作改变
    public void ChangeAction(string param_Action, bool loop = false, Action<object[]> func = null, bool param_Fade = true, float aniSpeed = 1.0f)
    {
        return;
    }

    private Dictionary<string, AnimationRow> AnimationDic = new Dictionary<string, AnimationRow>();
    private List<string> Weapons = new List<string>();

    private void InitWeaponInfo()
    {
        var items = StaticData.AnimationTable.TryGet(_owner.mData.ConfigID);
        if (items == null)
            return;
        foreach (var VARIABLE in items.Weapons)
        {
            Weapons.Add(VARIABLE);
        }
        AnimationDic.Add("idle", items);
    }

    private void WeaponAttachCheck(string name)
    {
        bool attach = true;
        if (!AnimationDic.TryGetValue(name, out AnimationRow item))
        {
            attach = true;
        }
        else
        {
            if (item.Hide == true)
            {
                attach = false;
            }
        }
        AttachWeapon(attach);
    }

    private void AttachWeapon(bool vis)
    {
        foreach (var VARIABLE in Weapons)
        {
            if (string.IsNullOrEmpty(VARIABLE))
                continue;
            Transform bone = _owner.GetBone(VARIABLE);
            if (bone != null)
            {
                bone.gameObject.SetActive(vis);
            }
        }
    }

    private void PlayAnimation(string param_Name, int param_Layer, bool param_Fade, float aniSpeed = 1.0f)
    {
        return;
    }

    private void PlaySubAnimation(string param_Name)
    {
        return;
    }

    public void RePlayAnim()
    {
        string temp = m_LastUpAction;
        m_LastUpAction = "";
        ChangeAction(temp);
    }

    public void ClearAction(bool param_Fade = true) { }
#endregion

#region 销毁
    public void Destroy()
    {
        //_owner = null;
    }
#endregion
}