using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using UnityEngine;
using UnityEngine.UI;

/*public class AntiItem
{
    public GameObject root;
    public Image job;
}*/
public partial class JobAnti : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> JobRoots = new List<GameObject>();
    public List<Image> Jobs = new List<Image>();

    public List<GameObject> Links = new List<GameObject>();
    public List<GameObject> Arrows = new List<GameObject>();
    void Awake()
    {
        for (int i = 1; i <= 5; i++)
        {
            var root = this.transform.Find($"Job{i}").gameObject;
            JobRoots.Add(root);

            var image = this.transform.Find($"Job{i}/Job").GetComponent<Image>();
            Jobs.Add(image);
        }
    }

    public void ShowAnitJobFloating()
    {
        UIEngine.Stuff.ShowFloating<AnitiJobFloating>();
    }
    
    public void SelectChange()
    {
        var role = SceneObjectManager.Instance.CurSelectHero;
        if (role == null)
            return;
        
        Creature target = null;
        target = SceneObjectManager.Instance.Find(role.mData.targetKey);
        if(target == null)
            return;
        
        foreach (var VARIABLE in Links)
        {
            VARIABLE.SetActive(false);
        }

        foreach (var VARIABLE in Jobs)
        {
            VARIABLE.color = Color.white;
        }
        
        for (int i = 0; i < JobRoots.Count; i++)
        {
            JobRoots[i].transform.localScale = Vector3.one;
        }
        
        foreach (var VARIABLE in Arrows)
        {
            VARIABLE.SetActive(true);
        }
        
        BattleRestraintIconRed.SetActive(false);
        BattleRestraintIconGreen.SetActive(false);

        string line = "";
        string hide = "";
        if (role.mData.Job == target.mData.Job)
        {
            return;
        }
        else if (role.mData.Job == target.mData.AnitJob)
        {
            //红色
            line = $"Other{(int)target.mData.Job}_{(int)role.mData.Job}";
            //hide = $"Arrow{(int)target.mData.Job}_{(int)role.mData.Job}";
        }
        else if (role.mData.AnitJob == target.mData.Job)
        {
            //绿色
            line = $"Me{(int)role.mData.Job}_{(int)target.mData.Job}";
            //hide = $"Arrow{(int)role.mData.Job}_{(int)target.mData.Job}";
        }

        //Jobs[(int)role.mData.Job - 1].color = Color.green;
        //Jobs[(int)target.mData.Job - 1].color = Color.red;
        BattleRestraintIconRed.SetActive(true);
        BattleRestraintIconGreen.SetActive(true);
        BattleRestraintIconGreen.SetPosition(Jobs[(int)role.mData.Job - 1].transform.position);
        BattleRestraintIconRed.SetPosition(Jobs[(int)target.mData.Job - 1].transform.position);

        for (int i = 0; i < JobRoots.Count; i++)
        {
            if (i + 1 == (int)role.mData.Job)
            {
                JobRoots[i].transform.localScale = Vector3.one * 1.2f;
                break;
            }
        }

        //this.transform.Find($"Job{(int) role.mData.Job}").localScale = Vector3.one * 1.2f;
        if (!string.IsNullOrEmpty(line))
        {
            foreach (var VARIABLE in Links)
            {
                if (VARIABLE.name.Equals(line))
                {
                    VARIABLE.SetActive(true);
                    break;
                }
            }
        }
        
        /*if (!string.IsNullOrEmpty(hide))
        {
            foreach (var VARIABLE in Arrows)
            {
                if (VARIABLE.name.Equals(hide))
                {
                    VARIABLE.SetActive(false);
                }
                else
                {
                    VARIABLE.SetActive(true);
                }
            }
        }*/
        
    }
}
