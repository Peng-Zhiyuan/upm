using UnityEngine;

public class LocalPlayerCamera : SceneObject
{
    private Creature _followTarget = null;

    protected CameraFollowController _camra_ctrl = new CameraFollowController();

    private bool _bDisableFollowSelect = false;

    //private Behavior behaviourTree = null;
    public LocalPlayerCamera(string param_ID) : base(param_ID)
    {
        //_camra_ctrl.Init(this);

        //InitTree();
        AttachMask();
    }

    private void AttachMask()
    {
        GameObject go = GameObject.Find("MaskObject");
        if (go != null)
        {
            //go.transform.SetParent(this.transform);
            //go.transform.localPosition = Vector3.zero;
        }
    }

    //    private void InitTree()
    //    {
    //        behaviourTree = this.gameObject.AddComponent<Behavior>();
    //        var behaviorTree = BehaviourTreeManager.Instance.TakeExternalBehaviourTree("FlowStage");
    //        behaviourTree.ExternalBehavior = behaviorTree;
    //        behaviourTree.StartWhenEnabled = true;
    //        behaviourTree.RestartWhenComplete = true;
    //        behaviourTree.PauseWhenDisabled = true;
    //        behaviourTree.ResetValuesOnRestart = true;
    //#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
    //        behaviourTree.showBehaviorDesignerGizmo = false;
    //#endif
    //        behaviourTree.EnableBehavior();

    //       // BehaviourTreeManager.Instance.treeList.Add(behaviourTree);
    //    }

    protected override void InitializeGameObject() { }

    public void SelectPlayer(object[] data)
    {
        if (CameraManager.Instance.MainCamera == null)
            return;
        var tmp_creature = SceneObjectManager.Instance.GetSelectPlayer();
        if (tmp_creature != null)
        {
            _followTarget = tmp_creature;
        }
        else
        {
            if (_followTarget != null)
            {
                /*if (_followTarget.sceneObjectType == SceneObjectType.Player)
                {
                    _followTarget = null;
                }*/
                /*if (_followTarget.sceneObjectType != SceneObjectType.NPC)
                {
                    _followTarget = null;
                }*/
            }
        }
        if (_followTarget == null
            || _followTarget.transform == null)
        {
            _followTarget = null;
            foreach (var VARIABLE in SceneObjectManager.Instance.GetAllCreatures())
            {
                if (VARIABLE.sceneObjectType == SceneObjectType.NPC)
                {
                    _followTarget = VARIABLE;
                    break;
                }
            }
            if (_followTarget == null)
            {
                Creature[] players = SceneObjectManager.Instance.GetAllPlayer();
                foreach (var player in players)
                {
                    _followTarget = player;
                    break;
                }
            }
        }
        if (_followTarget != null
            && _followTarget.transform.position != transform.position)
        {
            Move(_followTarget.transform.position);
        }
    }

    public override void LateUpdate(float param_deltaTime)
    {
        // if(!IsFollow)
        //return;

        //if(/*GameManager.Instance.GameMode != GameModeType.Map && */!_bDisableFollowSelect)
        //SelectPlayer(null);
    }

    public void Move(Vector3 target, bool isAuto = false)
    {
        objectEvent.Broadcast(GameEvent.SyncPos, target);
    }

    public void MoveTo(Vector3 target, float time = 2f)
    {
        _camra_ctrl.MoveTo(target, time);
    }

    public void MoveImmediate(Vector3 target)
    {
        _camra_ctrl.MoveImmediate(target);
    }

    public void SetFocusTarget(Creature target)
    {
        //_followTarget = target;
        //SetPosition(target.GetPosition());
        CameraManager.Instance.SetTarget(target.transform, null);
    }

    public void SetFollowTarget(Creature target)
    {
        SceneObjectManager.Instance.CleanSelectPlayers();
        _followTarget = target;
    }

    public override void Update(float param_deltaTime)
    {
        if (CameraManager.Instance.GetCurrentStateID() == (int)CameraState.Disabled)
            return;

        //_camra_ctrl.Update(param_deltaTime);

        //BehaviorManager.instance.Tick(behaviourTree);
    }

    public override SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.LocalPlayer; }
    }

    protected override void DestroyObject()
    {
        _camra_ctrl.OnDestroy();
    }

    public bool DisableFollowSelect
    {
        get { return _bDisableFollowSelect; }
        set { _bDisableFollowSelect = value; }
    }

    public bool IsFollow { get; set; } = false;
}