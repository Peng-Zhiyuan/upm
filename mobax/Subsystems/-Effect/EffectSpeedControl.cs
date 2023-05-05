using UnityEngine;
using System.Collections.Generic;
using MathDL;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
[ExecuteInEditMode]
public class EffectSpeedControl : MonoBehaviour
{
    //public float time = 1;//销毁时间
    public float speed = 1f;//速度
    private float cacheSpeed = 1f;
    public ParticleSystem[] particleList;
    public Animator[] animatorList;
    public Animation[] animationList;
#if UNITY_EDITOR
    public bool m_refreshNow = false;
#endif

    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;

        if (m_refreshNow)
        {
            m_refreshNow = false;
            DoRefresh(true);
        }
#endif
        if (speed != cacheSpeed)
        {
            cacheSpeed = speed;
            this.ChangeSpeed();
        }
    }

    void DoRefresh(bool recursive)
    {
#if UNITY_EDITOR
        if (m_refreshNow) m_refreshNow = false;
        this.RefreshNow();
#endif
    }

    protected void RefreshNow()
    {           
        particleList = this.GetComponentsInChildren<ParticleSystem>();
        animatorList = this.GetComponentsInChildren<Animator>();
        animationList = this.GetComponentsInChildren<Animation>();
        cacheSpeed = speed;
        this.ChangeSpeed();
    }


    public void ChangeSpeed()
    {
        for (int i = 0; i < particleList.Length; i++)
        {
            var main = particleList[i].main;
            main.simulationSpeed = speed;
        }
        for (int i = 0; i < animatorList.Length; i++)
        {
            animatorList[i].speed = speed;
        }

        for (int i = 0; i < animationList.Length; i++)
        {
            foreach (AnimationState animState in animationList[i])
            {
                animState.speed = speed;
            }
        }

    }


    /*
    [SerializeField, SetProperty("Speed")]
    private float mSpeed;
    public float Speed
    {
        get
        {
            return mSpeed;
        }
        private set
        {
            mSpeed = value;
            ChangeSpeed();
        }
    }
    */
   
    /*
    void Update()
    {
        time -= Time.deltaTime * mSpeed;
        if (time < 0)
        {
            Destroy(this.gameObject);

        }
    }
    */
}