using System;

using System.Collections;
using System.Collections.Generic;
//using LuaInterface;
using UnityEngine;
using DG.Tweening;

//可交互物品
public class InterActiveItem : SceneObject
{
    private GameObject m_selectEffect = null;
    public InterActiveItem(string param_ID, int configID) : base(param_ID, configID)
    {
        //Name = param_ID;
        //GameEventCenter.Broadcast(GameEvent.CreateHud_Success, this);
    }

    protected override void InitializeGameObject()
    {
        var go = new GameObject();//CreateModel("Character/Item/Cube");
        go.transform.SetParent(this.transform);
        go.transform.localPosition = Vector3.zero;

        var controller = gameObject.AddComponent<CharacterController>();
        controller.center = new Vector3(0, 0.4f, 0);
        controller.radius = 2f;
        controller.height = 2;
        controller.skinWidth = 0.03f;
        float tmp_offset = 2 + 2 * 2;
        tmp_offset = tmp_offset > 2 ? 2 : tmp_offset;
        controller.stepOffset = 0.5f > tmp_offset ? tmp_offset : 0.5f;

        var tmp_resource = Resources.Load("Character/Guide/Ring");
        if (tmp_resource == null)
        {
            return;
        }

        m_selectEffect = GameObject.Instantiate(tmp_resource) as GameObject;
        m_selectEffect.transform.SetParent(go.transform);
        m_selectEffect.transform.localPosition = Vector3.zero;
        m_selectEffect.transform.localScale = Vector3.one*2.5f;
        m_selectEffect.SetActive(false);

        
    }

    protected override void DestroyObject()
    {

    }
    //private int _frame;
    public override void Update(float param_deltaTime)
    {
        //_frame = Time.frameCount % 16;
        //if (_frame == 1)
        //{
        //    if (isOtherClearAction)
        //    {
        //        this.SetPlaySceneObjectAction(true);
        //    }
        //    if (isOtherPlayAction)
        //    {
        //        this.SetPlaySceneObjectAction(false);
        //    }
        //}
    }

    public override void LateUpdate(float param_deltaTime)
    {

    }

    public void ShowSelectEffect(bool vis)
    {
        m_selectEffect.SetActive(vis);
        if (vis)
        {

        }
        else
        {

        }

    }

    public override SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.Item; }
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
    }
}
