using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using Task = BehaviorDesigner.Runtime.Tasks.Task;

/// <summary>
/// 猫猫/海鸥等特殊事件的巡逻ai
/// </summary>
public class CatWanderAI : MonoBehaviour
{
    // 起始点
    private Vector3 _sp;

    // 结束点
    private Vector3 _ep;

    // 当前位置
    private Vector3 _curPos;

    // 间隔时间 可以用动画时间来计算
    private float _intervalTime = 0f;

    // 需要改变的时间 这里先用随机时间代替
    private float _changeTime = 0f;

    // 可以移动
    private bool _canMove = false;

    // 可以更新
    private bool _canChanged = false;

    private Animator _animtor;

    // tween运动的时间
    private float _duration = 10f;
    private float _stayDurationMin;
    private float _stayDurationMax;
    private Tweener _tween;

    // 猫猫当前状态
    private CatWanderState _curState;

    private Action _clickAction;

    private void OnDrawGizmos()
    {
        var sp = this._sp;
        var ep = this._ep;
        var p1 = sp;
        var p2 = new Vector3(sp.x, sp.y, ep.z);
        var p3 = ep;
        var p4 = new Vector3(ep.x, ep.y, sp.z);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }

    public void SetClickAction(Action clickAction)
    {
        this._clickAction = clickAction;
    }

    // 初始化行动区域
    public void InitMoveArea(Vector3 sp, Vector3 ep)
    {
        this._sp = sp;
        this._ep = ep;
    }

    public void Start2Move()
    {
        this._tween?.Kill();
        this._canMove = true;
        this._canChanged = true;
        this._intervalTime = 0f;
        this._changeTime = 0f;
        this.StartAI();
    }

    public async void BornFly()
    {
        this._curState = CatWanderState.FLy;
        this._curPos = this.RandomPoint(this._sp, this._ep);
        this._animtor = transform.GetComponent<Animator>();
        this._animtor.Play("land", 0, 0);
        this.transform.localPosition = this._curPos + new Vector3(0, 5, 0);
        this._tween = this.transform.DOMoveY(0, 0.5f).SetEase(Ease.InCirc).OnComplete(() => { });
        TimerMgr.Instance.BattleSchedulerTimerDelay(1, () =>
                        {
                            if (this == null
                                || this.transform == null)
                            {
                                return;
                            }
                            this._curState = CatWanderState.Move;
                            this.Move();
                            this._sp = this._sp + new Vector3(-5, 0, -5);
                            this._ep = this._ep + new Vector3(5, 0, 5);
                            ResetCurPos();
                            this._curState = CatWanderState.Move;
                        }
        );
    }

    public void SetDuration(float duration, float stayDurationMin, float stayDurationMax)
    {
        this._duration = duration;
        this._stayDurationMax = stayDurationMax;
        this._stayDurationMin = stayDurationMin;
    }

    public void Init(bool init)
    {
        this._canChanged = !init;
        this.StartAI();
    }

    public void InitCatInfo(StageCatInfo catInfo, Action clickCb)
    {
        this.InitMoveArea(catInfo.sp, catInfo.ep);
        this.SetDuration(catInfo.duration, catInfo.stayDurationMin, catInfo.stayDurationMax);
        this.SetClickAction(clickCb);
    }

    public void StartAI()
    {
        this._curPos = this.RandomPoint(this._sp, this._ep);
        this._animtor = transform.GetComponent<Animator>();
        this._curState = CatWanderState.Idle;
        this.transform.localPosition = this._curPos;
    }

    private async void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // this.DoClick();
        }
        switch (this._curState)
        {
            case CatWanderState.Idle:
                this.DoIdle();
                break;
            case CatWanderState.Move:
                this.DoMove();
                break;
        }
    }

    private void DoClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo)) //如果碰撞检测到物体
        {
            if (!hitInfo.collider.gameObject.CompareTag("StageCat")) return;
            // this._clickAction?.Invoke();
            Debug.Log("[CatMove]:点击到小猫咪了,赶紧打死");
        }
    }

    private void DoIdle()
    {
        if (this._changeTime <= 0)
        {
            this._changeTime = Random.Range(this._stayDurationMin, this._stayDurationMax);
        }
        if (this._intervalTime >= this._changeTime)
        {
            if (this._canChanged)
            {
                this.RandomState();
            }
        }
        else
        {
            this._intervalTime += Time.deltaTime;
        }
    }

    private void DoMove()
    {
        if (this._canMove)
        {
            this.Move2Pos();
        }
    }

    private void OnDestroy()
    {
        this._tween?.Kill();
    }

    private void Move2Pos()
    {
        this._canMove = false;
        this.Move();
        var distance = Vector3.Distance(this._curPos, this.transform.localPosition);
        Debug.Log("[CatMove]:当前距离为" + distance);
        // 计算匀速效果
        var speed = distance / (0.1f * this._duration);
        this._tween = this.transform.DOLocalMove(this._curPos, speed).OnComplete(() => { this.RandomState(); });
        this._animtor.speed = 2;
    }

    private void Move()
    {
        if (!this._animtor) return;
        this._animtor.Play(CharacterActionConst.Walk);
    }

    private void RandomState()
    {
        this._canMove = false;
        var random = Random.Range(0f, 1f);
        this._curState = random > 0.5f ? CatWanderState.Move : CatWanderState.Idle;
        Debug.Log("[CatMove]:当前状态为" + this._curState);
        if (this._curState == CatWanderState.Move)
        {
            this.ResetCurPos();
        }
        else if (this._curState == CatWanderState.Idle)
        {
            this._intervalTime = 0f;
            this._changeTime = 0f;
            this._animtor.speed = 1;
            this._animtor.Play("idle");
        }
    }

    private void ResetCurPos()
    {
        this._curPos = this.RandomPoint(this._sp, this._ep);
        //刷新时利用lookAt转向
        this.SetLookAt();
        this._intervalTime = 0;
        this._canMove = true;
    }

    private void SetLookAt()
    {
        var pos = this.transform.localPosition;
        var targetPos = this._curPos;
        // var targetDir = targetPos - pos;
        // var angle = Vector3.Angle(targetDir, Vector3.forward);
        // if (_curPos.x > pos.x)
        // {
        //     angle = -angle;
        // }
        //
        this.transform.LookAt(targetPos);
        //
        // var rotation = this.transform.localRotation;
        // this.transform.localRotation = Quaternion.Euler(rotation.x, -angle, rotation.z);
    }

    //根据两点在范围内随机坐标
    private Vector3 RandomPoint(Vector3 a, Vector3 b)
    {
        Vector3 x = new Vector3(Random.Range(a.x, b.x), a.y, Random.Range(a.z, b.z));
        return x;
    }
}