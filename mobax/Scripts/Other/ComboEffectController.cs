using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboEffectController : MonoBehaviour
{
    public Image Power;

    public List<GameObject> Types = new List<GameObject>();
    //public GameObject Up;

    public Animator Animator = null;

    public float PowerSpeed = 1f;

    private RectTransform BarRectTransform;

    public GameObject PowerTips;
    
    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        BarRectTransform = Power.GetComponent<RectTransform>();

        //StartPower = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (StartPower && !IsFull)
        {
            BarRectTransform.sizeDelta = new Vector2(BarRectTransform.sizeDelta.x, BarRectTransform.sizeDelta.y + PowerSpeed);
            if (BarRectTransform.sizeDelta.y > 89)
                IsFull = true;
        }
    }

    public bool IsFull
    {
        get;
        set;
    }

    public void SetPercent(int type, float percent)
    {
        if (type == 4)
        {
            //Power.fillAmount = percent;
        }
    }

    public void Reset(int type)
    {
        if (type == 4)
        {
            //Power.fillAmount = 0;
            IsFull = false;
            BarRectTransform.sizeDelta = new Vector2(BarRectTransform.sizeDelta.x, 0);
            /*Transform TempPower = Animator.transform.Find("PowerNode");
            //TempPower.localScale = Vector3.one;
            TempPower.gameObject.SetActive(false);*/
        }
    }

    private bool _startpower = false;
    public bool StartPower
    {
        get { return _startpower;}
        set
        {
            _startpower = value;
            Reset(4);
        }
    }

    public void SetType(int type, bool cooldown)
    {
        /*int index = 1;
        foreach (var VARIABLE in Types)
        {
            VARIABLE.SetActive(type == index++);
        }*/

        if (type <= 3)
        {
            //Animator.gameObject.SetActive(true);
            
            Transform flick = Animator.transform.Find("FlickHint");
            //flick.localScale = Vector3.one;
            if(type == 1)
                flick.localRotation = Quaternion.Euler(0, 0, 90);
            else if(type == 3)
                flick.localRotation = Quaternion.Euler(0, 0, -90);
            else
                flick.localRotation = Quaternion.Euler(0, 0, 0);
            Animator.Play("FlickHint");
        }
        else
        {
            Transform TempPower = Animator.transform.Find("PowerNode");
            TempPower.gameObject.SetActive(!InCD);
            
            ShowPowerTips(!InCD);
        }
    }

    private bool _inCD = false;
    public bool InCD
    {
        get { return _inCD;}
        set
        {
            _inCD = value;
            if(value == false)
                Hide();
            
        }
    }

    public void Hide()
    {
        foreach (var VARIABLE in Types)
        {
            if (VARIABLE != null)
            {
                VARIABLE.SetActive(false);
                //VARIABLE.transform.localScale = Vector3.zero;
            }
        }
        
        //Animator.gameObject.SetActive(false);
        StartPower = false;
    }

    public void PlayAnim(string anim)
    {
        if(Animator != null)
            Animator.Play(anim);
    }

    public void ShowPowerTips(bool vis)
    {
        if (PowerTips != null)
            PowerTips.SetActive(vis);
    }
}
