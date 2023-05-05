using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spine;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
using System.Text;
using DG.Tweening;

[Serializable]
public class FxMakerData
{
    public int fxSel;
}

public class FxMaker : MonoBehaviour
{
    private void Start()
    {
        ToolStatic._tooType = ToolStatic.TOOL_TYPE.FX_MAKER;
        Time.timeScale = 1.0f;
        gameSpeedSlider.value = 1.0f;
        gameSpeedLabel.text = "1.0";
        Init();
    }

    private FxMakerData data = null;

    void SaveData()
    {
        if (data == null)
        {
            data = new FxMakerData();
        }
        data.fxSel = mQuickSelDropDown.value;
        var bytes = SerializationUtility.SerializeValue(data, DataFormat.JSON);
        var id = gameObject.GetInstanceID().ToString();
        var val = Encoding.ASCII.GetString(bytes);
        PlayerPrefs.SetString(id, val);
    }

    void LoadData()
    {
        var id = gameObject.GetInstanceID().ToString();
        var str = PlayerPrefs.GetString(id);
        var val = Encoding.ASCII.GetBytes(str);
        if (val != null)
        {
            data = SerializationUtility.DeserializeValue<FxMakerData>(val, DataFormat.JSON);
            if (data != null)
            {
                ResetFromData();
            }
        }
    }

    void ResetFromData()
    {
        _loadFxId = data.fxSel;
    }

    async void Init()
    {
        await _roleEditBase.Init();
        _roleEditBase._roleDels += RoleSelect;
        var count = StaticData.HeroTable.Count;
        for (var i = 0; i < count; ++i)
        {
            var role = StaticData.HeroTable.TryGet(i);
            var filePath = SpineUtil.Instance.GetSpinePath(role.Id);
            var lIndex = filePath.LastIndexOf('_');
            if (lIndex != -1)
            {
                midNameDic.Add(role.Id, filePath.Substring(lIndex, filePath.Length - lIndex - 7));
            }
            else
            {
                midNameDic[role.Id] = "midName";
            }
        }
        //roleDropDown.value = 0;
        mDelay.Init(0, 5);
        mLife.Init(0, 10);
        FrefshList();
        LoadData();
        //  LoadEnemy();
    }

    async void LoadEnemy()
    {
        var address = SpineUtil.Instance.GetSpinePath(enemyId);
        //var tr = await AddressableRes.AquireAsync<Object>(address);
        var bucket = BucketManager.Stuff.Tool;
        var tr = await bucket.GetOrAquireAsync<Object>(address);
        if (tr == null)
        {
            return;
        }
        var role = GameObject.Instantiate(tr) as GameObject;
        role.transform.SetLocalScale(SpineUtil.Instance.GetSDScale(enemyId));
        role.transform.position = Vector3.zero;
        cemeraC.m_Target = role.transform;
        var roleRender = role.GetComponent<RoleRender>();
        if (roleRender != null)
        {
            //roleRender.subRender.transform.rotation = cemeraC.transform.rotation;
            roleRender.SwitchPlayMode(false);
            roleRender.Init();
            enemyRs = roleRender.GetComponent<RoleRender>();
            enemyGo = roleRender.gameObject;
            enemyRs.gameObject.SetActive(true);
            enemyGo.transform.position = new Vector3(1.5f, 0f, 0f);
        }
    }

    public void RoleSelect(int _sel)
    {
        midName = midNameDic[_sel];
        Debug.Log(midName);
        FrefshList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (Time.timeScale > 0.0f)
            {
                Time.timeScale = 0.0f;
                gameSpeedLabel.text = Time.timeScale.ToString("0.0");
                gameSpeedSlider.value = 0.0f;
            }
            else
            {
                Time.timeScale = 1.0f;
                gameSpeedLabel.text = Time.timeScale.ToString("1.0");
                gameSpeedSlider.value = 1.0f;
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayEffect();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            ClearB();
        }
        /*
        if (_roleEditBase.CRR != null)
        {
            _roleEditBase.CRR.subRender.transform.rotation = cemeraC.transform.rotation;
        }

        if (enemyRs != null)
        {
            enemyRs.subRender.transform.rotation = cemeraC.transform.rotation;
           
            // if (Input.GetMouseButton(0)&& Input.GetKey(KeyCode.LeftControl))
            // {
            //     //control pos
            //     var mpos = Input.mousePosition;
            //     var ray=CurCamera.ScreenPointToRay(mpos);
            //     RaycastHit result;
            //     if (Physics.Raycast(ray, out result))
            //     {
            //         enemyGo.transform.position = result.point;
            //         //Move(result.point);
            //     }
            // }
        }
        */
    }

    void PlayEffect()
    {
        _roleEditBase.PlayAni();
        Play(_roleEditBase.CRR.GetBoneTrans("root"));
        // _roleEditBase.Save();
        SaveData();
    }

    public void ClearB()
    {
        ClearFxGo();
    }

    public void GameSpeedChange(float _v)
    {
        Time.timeScale = _v;
        gameSpeedLabel.text = _v.ToString("0.0");
    }

    static void FecthList(string _filter)
    {
        var filterName = _filter;
        fileNameList.Clear();
        string path = "Assets/Arts/FX/"; //Assets/Scenes
        if (Directory.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (fileInfos[i].Name.EndsWith(".meta")) continue;
                if (fileInfos[i].Name.EndsWith(".prefab"))
                {
                    if (!string.IsNullOrWhiteSpace(filterName))
                    {
                        var ff = filterName.ToLower();
                        var ff1 = fileInfos[i].Name.ToLower();
                        if (!ff1.Contains(ff))
                        {
                            continue;
                        }
                    }
                    fileNameList.Add(fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 7));
                }
            }
        }
        Debug.Log("Sort:" + midName);
        fileNameList.Sort(pathSort);
    }

    public static string midName = "midName";

    static int GetPoint(string path)
    {
        var p = 0;
        if (path.Contains("FX_"))
        {
            p += 10;
        }
        if (path.Contains(midName))
        {
            p += 100;
        }
        return p;
    }

    static int pathSort(string a, string b)
    {
        var pa = GetPoint(a);
        var pb = GetPoint(b);
        if (pa > pb)
        {
            return -1;
        }
        else if (pa < pb)
        {
            return 1;
        }
        else
        {
            return string.Compare(a, b);
        }
    }

    public void FrefshList()
    {
        mQuickSelDropDown.ClearOptions();
        FecthList(mFilterStr.GetValue());
        mQuickSelDropDown.AddOptions(fileNameList);
        if (_loadFxId != 0)
        {
            mQuickSelDropDown.value = _loadFxId;
            _loadFxId = 0;
        }
        else
        {
            mQuickSelDropDown.value = 0;
        }
    }

    public async void Play(Transform root)
    {
        if (mQuickSelDropDown.value >= fileNameList.Count)
        {
            return;
        }
        var fxName = fileNameList[mQuickSelDropDown.value];
        string path = $"{fxName}.prefab";
        //var tr=await AddressableRes.AquireAsync<Object>(path);
        var bucket = BucketManager.Stuff.Tool;
        var tr = await bucket.GetOrAquireAsync<Object>(path);
        if (tr == null)
        {
            return;
        }
        var fxIns = GameObject.Instantiate(tr) as GameObject;
        fxIns.transform.parent = root;
        fxIns.transform.localScale = Vector3.one;
        fxIns.transform.localPosition = Vector3.zero;
        fxIns.transform.localRotation = Quaternion.identity;
        var fxInit = fxIns.GetComponent<ScEffectInit>();
        if (fxInit != null)
        {
            fxInit.Play();
        }
        var _delay = mDelay.GetValue();
        var _life = mLife.GetValue();
        if (_delay > 0.0)
        {
            fxIns.SetActive(false);
            await Task.Delay((int)(_delay * 1000));
            fxIns.SetActive(true);
        }
        if (_life > 0)
        {
            await Task.Delay((int)(_life * 1000));
            GameObject.Destroy(fxIns);
        }
        fxGoList.Add(fxIns);
        if (_toggleFlyObject.isOn)
        {
            var bpos = fxIns.transform.position;
            bpos.y = 1f;
            var epos = bpos + Vector3.right * 5f;
            fxIns.transform.parent = null;
            fxIns.transform.position = bpos;
            fxIns.transform.DOMove(epos, 0.3f);
            //fxIns   
        }
    }

    public void ClearFxGo()
    {
        for (var i = 0; i < fxGoList.Count; ++i)
        {
            if (fxGoList[i])
            {
                GameObject.Destroy(fxGoList[i]);
            }
        }
        fxGoList.Clear();
    }

    private static List<string> fileNameList = new List<string>();

    public ValueChanger mFilterStr;
    public BarValueChanger mDelay;
    public BarValueChanger mLife;

    public Dropdown mQuickSelDropDown;
    private List<GameObject> fxGoList = new List<GameObject>();

    public Toggle _toggleFlyObject;

    private const int enemyId = 50002;
    private GameObject enemyGo;
    private RoleRender enemyRs;

    private Dictionary<int, string> midNameDic = new Dictionary<int, string>();

    public CustomCameraController cemeraC;
    public Slider gameSpeedSlider;
    public Text gameSpeedLabel;
    public Camera CurCamera;
    public RoleEditBase _roleEditBase;

    private int _loadFxId = 0;
}