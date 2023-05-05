using UnityEngine;
using Object = UnityEngine.Object;
using BattleSystem.ProjectCore;

public class CharCtrl_Probe
{
    public Creature _owner = null;

    private GameObject _probe;

    public void Init(Creature owner)
    {
        _owner = owner;

        // if (!MapUtil.JudgeRoguelikeMap()) return;
        //
        // // 创建一个空的节点对象，用来显示probe
        // _probe = new GameObject($"LightProbe_{_owner.ConfigID}");
        // _probe.transform.SetParent(SceneObjectManager.Instance.ProbeRoot);
        //
        // var skinMesh = owner.transform.GetComponentInChildren<SkinnedMeshRenderer>();
        // skinMesh.probeAnchor = _probe.transform;
        //
        // // SetProbePos();
    }

    public void Update(float param_deltaTime)
    {
        // if (_owner == null || !_owner.IsMoveState() || !MapUtil.JudgeRoguelikeMap())
        //     return;
        //
        // SetProbePos();
    }

    private void SetProbePos()
    {
        // var mapGenerate = Battle.Instance.battleCore.FindStuff<MapGenerateCore>();
        // mapGenerate.SetProbePos(_probe, _owner.transform.localPosition,_owner.ConfigID);
    }

    public void Destroy()
    {
        // _owner = null;
        // Object.Destroy(_probe);
    }
}