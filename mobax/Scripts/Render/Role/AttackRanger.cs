using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AttackRanger : MonoBehaviour
{
    /// <summary>
    /// 显示伤害范围
    /// </summary>
    /// <param name="tPos">目标点</param>
    /// <param name="radius">半径</param>
    /// <param name="targets">包含敌人【可选，没有null】</param>
    public void Show(Vector3 tPos,float radius,Vector3[] targets)
    {
        
        var bpos = transform.position;
        bpos.y += offsetBeginY;
        var epos = tPos;
        epos.y += offsetEndY;
        roudLine.BeatShow(bpos,epos);
        area.SetActive(true);
        

        
        
        area.transform.position = tPos;
        area.transform.SetLocalScale(radius*2f*scaleOffset);



        var tnum = targets == null ? 0 : targets.Length;
        
        Debug.Log("tnum:"+tnum);
        for (var i = 0; i < tnum; ++i)
        {
            if (enemyIns.Count <= i)
            {
                enemyIns.Add(FetchLockIns(transform));
            }
            enemyIns[i].transform.position = targets[i];
        }

        for (var i = tnum; i < enemyIns.Count; ++i)
        {
            ReleaseLockIns(enemyIns[i]);
        }
    }

    public void Close()
    {
        roudLine.SwitchDraw(false);
        area.SetActive(false);
        enemyIns.Clear();
        ReleaseAllIns();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isDebugShow = true;

        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            isDebugShow = false;
            Close();
        }

        if (isDebugShow)
        {
            var testEposList = new Vector3[testEnemys.Length];
            Vector3 _pos=Vector3.zero;
            for (int i = 0; i < testEnemys.Length; i++)
            {
                testEposList[i] = testEnemys[i].position;
                _pos.x += testEnemys[i].position.x;
                _pos.z += testEnemys[i].position.z;
            }
            _pos.x/= testEnemys.Length;
            _pos.z/=testEnemys.Length;
            Show(_pos,2f,testEposList);
        }
 #endif       
    }

    void ReleaseAllIns()
    {
        for (var i = 0; i < lockInsList.Count; ++i)
        {
            if (lockInsList[i].activeSelf)
            {
                lockInsList[i].SetActive(false);
                lockInsList[i].transform.parent = transform;
            }
        }
    }

    GameObject FetchLockIns(Transform _owner)
    {
        for (var i = 0; i < lockInsList.Count; ++i)
        {
            if (!lockInsList[i].activeSelf)
            {
                lockInsList[i].SetActive(true);
                lockInsList[i].transform.parent = _owner;
                lockInsList[i].transform.localPosition = Vector3.zero;
                lockInsList[i].transform.localScale = Vector3.one;
                return lockInsList[i];
            }
        }
        //spawrn one;
        var ins=GameObject.Instantiate(lockPref, _owner);
        ins.SetActive(true);
        lockInsList.Add(ins);
        return ins;
    }

    void ReleaseLockIns(GameObject _ins)
    {
        _ins.SetActive(false);
        _ins.transform.parent = transform;
        _ins.transform.localPosition=Vector3.zero;
        _ins.transform.localScale=Vector3.one;
    }

    public void SetOwnerSacle(float _os)
    {
        scaleOffset = 1f / _os;
    }
    private List<GameObject> lockInsList = new List<GameObject>();
    public float offsetBeginY;
    public float offsetEndY;
    public RoundLine roudLine;
    public GameObject area;
    public GameObject lockPref;


    private List<GameObject> enemyIns = new List<GameObject>();

    public float scaleOffset = 1f;

    
#if UNITY_EDITOR
    private bool isDebugShow = false;
    public Transform[] testEnemys;
#endif

}
