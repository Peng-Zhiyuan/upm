using UnityEngine;
using System.Threading.Tasks;
using BattleEngine.Logic;

/// <summary>
/// 仇恨线
/// </summary>
public class CharCtrl_HateLine
{
    public Creature _owner = null;

    private Creature _target = null;

    private HateLine HateLine = null;
    private HateLine ToMeLine = null;

    private Creature _lastTarget = null;
    private Creature _flowTarget = null;

    public void Init(Creature owner)
    {
        _owner = owner;
        _owner.mData.Subscribe<SwitchTargetEvent>(SwitchTargetEventHandler);
        _owner.mData.Subscribe<FocusPreTargetEvent>(FocusPreTargetEventHandler);
    }

    private async Task<HateLine> LoadLine(string prefab)
    {
        var address = prefab + ".prefab";
        var obj = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(address);
        var spine = GameObject.Instantiate(obj) as GameObject;
        return spine.GetComponent<HateLine>();
    }

    public void SwitchTargetEventHandler(SwitchTargetEvent data)
    {
        if (!_owner.IsEnemy)
            return;
        _lastTarget = SceneObjectManager.Instance.Find(data.targetUID);
    }

    public void FocusPreTargetEventHandler(FocusPreTargetEvent data)
    {
        _flowTarget = null;
        _lastTarget = SceneObjectManager.Instance.Find(data.targetUID);
    }

    public void Update(float param_deltaTime)
    {
        if (_flowTarget == _lastTarget)
            return;
        if (Vector3.Distance(_owner.GetPosition(), _owner.mData.GetPosition()) > 5)
            return;
        _flowTarget = _lastTarget;
        SetLine(_lastTarget);
    }

    public async void SetLine(Creature target)
    {
        if (_owner == null
            || target == null)
            return;
        HateLine line = null;
        if (_owner.IsEnemy)
        {
            if (ToMeLine == null)
            {
                ToMeLine = await LoadLine("fx_aimline_enemy");
            }
            line = ToMeLine;
        }
        else if (!_owner.IsMain)
        {
            return;
        }
        else
        {
            if (HateLine == null)
            {
                HateLine = await LoadLine("fx_aimline_friend");
            }
            line = HateLine;
        }
        if (_owner == null)
        {
            return;
        }
        _target = target;
        line.DrawLine(this._owner.GetBone("body_hit").gameObject, _target.GetBone("body_hit").gameObject);
    }

    public async void SetHateLine(Creature target)
    {
        if (_owner == null
            || target == null)
            return;
        if (HateLine == null)
        {
            await LoadLine("fx_aimline_friend");
        }
        if (_owner == null)
        {
            return;
        }
        _target = target;
        HateLine.DrawLine(this._owner.gameObject, target.gameObject);
    }

    public void Destroy()
    {
        if (HateLine != null)
            GameObject.Destroy(HateLine.gameObject);
        _owner.mData.UnSubscribe<SwitchTargetEvent>(SwitchTargetEventHandler);
        _owner.mData.UnSubscribe<FocusPreTargetEvent>(FocusPreTargetEventHandler);
    }
}