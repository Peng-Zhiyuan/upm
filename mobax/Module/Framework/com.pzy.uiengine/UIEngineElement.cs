using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System;

public class UIEngineElement : View
{
    [Tooltip("是否支持全局返回处理，全局返回会在点击安卓物理返回键时触发")]
    [BoxGroup("GlobalBack")]
    public bool hasLogicBack = true;

    FragmentManager _fragmentManager;
    protected FragmentManager fragmentManager
    {
        get
        {
            if (_fragmentManager == null)
            {
                _fragmentManager = new FragmentManager(this.transform);
            }
            return _fragmentManager;
        }
    }

    ReddotManager _reddotManager;
    protected ReddotManager reddotManager
    {
        get
        {
            if(_reddotManager == null)
            {
                _reddotManager = new ReddotManager();
            }
            return _reddotManager;
        }
    }


    private RectTransform _rectTransform;
    public RectTransform rectTransform 
    {
        get 
        {
            if (_rectTransform == null) 
            {
                this._rectTransform = this.gameObject.GetComponent<RectTransform> ();
            }
            return _rectTransform;
        }
    }

    public RectTransform RectTransform
    {
        get
        {
            return this.rectTransform;
        }
    }

    public async Task TryLogicBack()
    {
        if (this.hasLogicBack)
        {
            await this.LogicBackAsync();
        }
    }

    protected virtual Task LogicBackAsync()
    {
        throw new Exception("[View] LogicBack not implement yet");
    }

    Animator _animator;
    bool _animator_searched;
    public Animator Animator
    {
        get
        {
            if (!_animator_searched)
            {
                _animator_searched = true;
                this._animator = this.GetComponent<Animator>();
            }
            return this._animator;
        }
    }



    [ShowInInspector]
    public bool AnimatorStatueIsEntering
    {
        get
        {
            var animator = this.Animator;
            if (animator == null)
            {
                return false;
            }
            var info = animator.GetCurrentAnimatorStateInfo(0);
            var b = info.IsName("Entering");
            return b;
        }
    }

    [ShowInInspector]
    public bool AnimatorNextStatueIsEntering
    {
        get
        {
            var animator = this.Animator;
            if (animator == null)
            {
                return false;
            }
            var info = animator.GetNextAnimatorStateInfo(0);
            var b = info.IsName("Entering");
            return b;
        }
    }

    [ShowInInspector]
    public bool AnimatorStatueIsExiting
    {
        get
        {
            var animator = this.Animator;
            if (animator == null)
            {
                return false;
            }
            var info = animator.GetCurrentAnimatorStateInfo(0);
            var b = info.IsName("Exiting");
            return b;
        }
    }

    [ShowInInspector]
    public bool AnimatorNextStatueIsExiting
    {
        get
        {
            var animator = this.Animator;
            if (animator == null)
            {
                return false;
            }
            var info = animator.GetNextAnimatorStateInfo(0);
            var b = info.IsName("Exiting");
            return b;
        }
    }

    [ShowInInspector]
    public float AnimatorStatueNormlizedTime
    {
        get
        {
            var animator = this.Animator;
            if (animator == null)
            {
                return 0;
            }
            var info = animator.GetCurrentAnimatorStateInfo(0);
            return info.normalizedTime;
        }
    }

    public virtual void OnEnter()
    {
        //Debug.Log($"{this.name}: OnEnter");
        if (this.Animator != null)
        {
            this.Animator.SetTrigger("enter");
            this.Animator.Update(0f);
        }
    }

    public async Task WaitEnteringCompelteAsync()
    {
        if (this.Animator == null)
        {
            return;
        }
        await AnimatorUtil.WaitUtilNotInStateAsync(this.Animator, "Entering");
    }

    public virtual void OnExit()
    {
        //Debug.Log($"{this.name}: OnExit");
        if (this.Animator != null)
        {
            this.Animator.SetTrigger("exit");
            this.Animator.Update(0f);
        }
    }

    public async Task WaitExitingCompelteAsync()
    {
        //Debug.Log($"{this.name} WaitExiting");
        if (this.Animator == null)
        {
            return;
        }
        await AnimatorUtil.WaitUtilNotInStateAsync(this.Animator, "Exiting");
    }
}