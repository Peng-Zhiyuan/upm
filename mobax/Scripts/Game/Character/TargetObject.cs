using System;

using System.Collections;
using System.Collections.Generic;
//using LuaInterface;
using UnityEngine;
using DG.Tweening;

//目标点
public class TargetObject : SceneObject
{
    public TargetObject(string param_ID) : base(param_ID)
    {

    }

    protected override void InitializeGameObject()
    {
    }

    protected override void DestroyObject()
    {

    }

    public override void Update(float param_deltaTime)
    {
    }

    public override void LateUpdate(float param_deltaTime)
    {

    }

    public override SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.Target; }
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
    }
}
