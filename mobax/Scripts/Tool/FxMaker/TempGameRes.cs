using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempGameRes : MonoBehaviour
{


    private void Start()
    {
        for(var i = 0; i < mSkillList.Length; ++i)
        {
            mSkillDic[mSkillList[i]] = i;
        }
    }

    public void PlayFxShot(string _name,FX_TOOL_DIR _dir,Transform _obj)
    {
        var fname = _name;
        switch (_dir)
        {
            case FX_TOOL_DIR.FORWARD:
                fname += "F";
                break;
            case FX_TOOL_DIR.BACK:
                fname += "B";
                break;
            case FX_TOOL_DIR.LEFT:
                fname += "L";
                break;
            case FX_TOOL_DIR.RIGHT:
                fname += "R";
                break;

        }
        if (!mSkillDic.ContainsKey(fname))
        {
            return;
        }
        var _index = mSkillDic[fname];
        var _pref=mFxList[_index];
        var _ins = GameObject.Instantiate(_pref,Vector3.zero,Quaternion.identity,_obj);
        StartCoroutine(DelayRemove(_ins,10.0f));
    }
    IEnumerator DelayRemove(GameObject _go,float _delayTime)
    {
        yield return new WaitForSeconds(_delayTime);
        GameObject.Destroy(_go);
    }


    public static TempGameRes Single()
    {
        if (single == null)
        {
            single = GameObject.FindObjectOfType<TempGameRes>();
        }
        return single;
    }

    static TempGameRes single = null;

    public GameObject[] mFxList;
    public string[] mSkillList;
    Dictionary<string, int> mSkillDic = new Dictionary<string, int>();
}
