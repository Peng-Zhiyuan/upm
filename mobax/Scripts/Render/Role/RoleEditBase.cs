using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ScRender;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using System.Text;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;
using System.IO;
using Sirenix.Utilities;
using UnityEngine.EventSystems;

[Serializable]
public class RebData
{
    public int roleSel;
    public int dirSel;
    public int aniSel;
}

public class RoleEditBase : MonoBehaviour
{
    public delegate void SelDel(int a);
    public SelDel _roleDels;
    public GameObject eventSystem;
    public GameObject roleRoot;
    public InputField inputField;
    //private int curRoleId = -1;
    public Camera mCamera;
    public float moveSpeed = 10;
    public UnityEngine.UI.Toggle toggleLock;
    private List<RoleRender> _roleInsList = new List<RoleRender>();
    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
      //
      //ServiceSystem.Stuff.Create();
#endif
        ToolStatic._tooType = ToolStatic.TOOL_TYPE.ROLE_ROOM;
        if (GameObject.FindObjectOfType<EventSystem>() == null)
        {
            eventSystem.SetActive(true);
        }
        if (_cameraControl == null) _cameraControl = GameObject.FindObjectOfType<CustomCameraController>();
         _camera = _cameraControl.GetComponent<Camera>();
        ResetDay();
        ResetCamera();
        roleDropDown.options.Clear();
        dirDropDown.options.Clear();
        toggleLock.onValueChanged.AddListener(OnToggleChanged);
        inputField.onValueChanged.AddListener(FilterChanged);
        CustomCameraCtrl.lockCamera = true;
        this.Init();
    }
    private void OnToggleChanged(bool value)
    {
        CustomCameraCtrl.lockCamera = value;
    }
   

    private void FilterChanged(string value)
    {
        this.SwitchRoleList();
      
    }
    
    public void CameraHDel(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_HAngle = val;
        CameraCheck();
    }
    
    public void CameraVDel(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_VAngle = val;
        CameraCheck();
    }
    public void CameraVDelta(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_VAngle += val;
        CameraCheck();
    }

    public void CameraZoomDel(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_Distance = val;
        CameraCheck();
    }

    public void CameraBiasDelta(float x, float y, float z)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_SideXOffset += x;
        _cameraControl.m_SideYOffset += y;
        _cameraControl.m_SideZOffset += z;
        CameraCheck();
    }


    public void CameraBiasXDel(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_SideXOffset = val;
        CameraCheck();
    }

    public void CameraBiasYDel(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_SideYOffset = val;
        CameraCheck();
    }
    public void CameraBiasZDel(float val)
    {
        if (!_cameraControl)
        {
            return;
        }

        CameraTargetCheck();
        _cameraControl.m_SideZOffset = val;
        CameraCheck();
    }

    public void RotateDel(float val)
    {
        _lightTrans.rotation = Quaternion.Euler(0, val, 0);
    }

    public void MainLightRotateDel(float val)
    {
        _mainLight.transform.rotation = Quaternion.Euler(new Vector3(42.5f, val, 0f));//.finalDir = new Vector3(42.5f, val, 0f);
    }

    public void RoleRotateDelta(float val)
    {
        curRs.transform.rotation = curRs.transform.rotation * Quaternion.Euler(0, val, 0);
    }

    public void RoleRotateDel(float val)
    {
        curRs.transform.rotation = Quaternion.Euler(0, val, 0);
    }
    public void SubLightDisDel(float val)
    {
        _lightFxTrans.localPosition = new Vector3(0, 0, -val);
    }

    public void IntensityDel(float val)
    {
        _pointLight.intensity = val;
        //_pointLight.ResetStatic();
        _lightFxTrans.gameObject.SetActive(val > 0f);
    }

    public void SubRangeDel(float val)
    {
        _pointLight.range= val;
        //_pointLight.ResetStatic();
        _lightFxTrans.gameObject.SetActive(val > 0f);
    }

    public void MainLightIntensityDel(float val)
    {
        _mainLight.intensity = val;
    }

    void Update()
    {
        if (_cameraControl)
        {
            _cameraHLabel.text = _cameraControl.m_HAngle.ToString("0.");
            _cameraVLabel.text = _cameraControl.m_VAngle.ToString("0.");
            _cameraBiasXLabel.text = _cameraControl.m_SideXOffset.ToString("0.0");
            _cameraBiasYLabel.text = _cameraControl.m_SideYOffset.ToString("0.0");
            _cameraBiasZLabel.text = _cameraControl.m_SideZOffset.ToString("0.0");
            _cameraZoomLabel.text = _cameraControl.m_Distance.ToString("0.");

        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            CaptureScreen.QuickCapure();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            CaptureScreen.QuickMapCapure();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (roleDropDown.value > 0)
            {
                roleDropDown.value--;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (roleDropDown.value < heroIdList.Count - 1)
            {
                roleDropDown.value++;
            }
        }
        float speed = Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.W))
        {
            var dir = mCamera.transform.forward;
            dir.y = 0;
            this.roleRoot.transform.position += dir.normalized * speed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            var dir = mCamera.transform.forward;
            dir.y = 0;
            this.roleRoot.transform.position -= dir.normalized * speed;
        }

        if (Input.GetKey(KeyCode.A))
        {

            this.roleRoot.transform.position -= Vector3.Cross(mCamera.transform.up, mCamera.transform.forward) * speed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            this.roleRoot.transform.position += Vector3.Cross(mCamera.transform.up, mCamera.transform.forward) * speed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            this.curRs.PlayEffect(ROLE_EFFECT_TYPE.WEAPON_DISSOLVED);
        }

        if (Input.GetKey(KeyCode.R))
        {
            this.curRs.PlayEffect(ROLE_EFFECT_TYPE.DEAD);
        }

    }


    public void OnNext()
    {
        if (roleDropDown.value < heroIdList.Count - 1)
        {
            roleDropDown.value++;
        }
    }

    public void OnLast()
    {
        if (roleDropDown.value > 0)
        {
            roleDropDown.value--;
        }
    }

    //private RoleRender _focusRole;


    public void ResetCamera()
    {
        CameraTargetCheck();
        //_cameraZoomSlider.value = 12f;
        _cameraZoomSlider.onValueChanged.Invoke(12);
        //_cameraBiasXSlider.value = 0f;
        _cameraBiasXSlider.onValueChanged.Invoke(0);
        //_cameraBiasYSlider.value = 1.3f;
        _cameraBiasYSlider.onValueChanged.Invoke(0.3f);
       // _cameraBiasZSlider.value = 3f;
        _cameraBiasZSlider.onValueChanged.Invoke(-6f);
        //_cameraHSlider.value = 45f;
        _cameraHSlider.onValueChanged.Invoke(45);
        //_cameraVSlider.value = 7;
        _cameraVSlider.onValueChanged.Invoke(10);

    }

    void CameraTargetCheck()
    {
        if (_cameraControl.m_Target == null && curRs != null)
        {
            _cameraControl.m_Target = curRs.transform;
        }
    }

    public Transform CameraTarget
    {
        get 
        {
            if (curRs != null)
            {
                return curRs.transform;
            }
            else return null;
        }
    }

    void CameraCheck()
    {
        if (Time.timeScale <= 0.1f)
        {
            _cameraControl.ForceSet();
        }
    }

    public void ResetDay()
    {
        _mainLightSlider.value = 2f;
        _subLightSlider.value = 0.0f;
    }

    public void ResetNight()
    {
        _mainLightSlider.value = 0.5f;
        _subLightSlider.value = 0.0f;
    }
    /*
    void UpdateAni()
    {
        if (curAniIndex >= aniList.Count)
        {
            curAniIndex = aniList.Count - 1;
        }
    }
    */

    public void AniSelect(int _ani)
    {
        curAniIndex = _ani;
        if (curAnimtor)
        {
            this.ResetAnimator();
            if (aniList[curAniIndex].Contains("run"))
            {
                curAnimtor.SetBool("moving", true);
                curAnimtor.SetBool("run", true);
                curAnimtor.SetBool("walk", false);
            }
            else if (aniList[curAniIndex].Contains("walk"))
            {
                curAnimtor.SetBool("moving", false);
                curAnimtor.SetBool("run", false);
                curAnimtor.SetBool("walk", true);
            }
            else
            {
                curAnimtor.SetBool("moving", false);
                curAnimtor.SetBool("run", false);
                curAnimtor.SetBool("walk", false);
            }
            curAnimtor.Play(aniList[curAniIndex], 0, 0f);
            if (curSubAnimator != null)
            {
                for (int i = 0; i < curSubAnimator.Count; i++)
                {
                    curSubAnimator[i].Play(aniList[curAniIndex], 0, 0f);
                }
            }
        }
    }

    public void PlayAni()
    {
        if (curAnimtor)
        {
            this.ResetAnimator();
            if (aniList[curAniIndex].Contains("run"))
            {
                curAnimtor.SetBool("moving", true);
                curAnimtor.SetBool("run", true);
                curAnimtor.SetBool("walk", false);
            }
            else if (aniList[curAniIndex].Contains("walk"))
            {
                curAnimtor.SetBool("moving", false);
                curAnimtor.SetBool("run", false);
                curAnimtor.SetBool("walk", true);
            }
            else
            {
                curAnimtor.SetBool("moving", false);
                curAnimtor.SetBool("run", false);
                curAnimtor.SetBool("walk", false);
            }
            curAnimtor.Play(aniList[curAniIndex],0,0f);
        }
    }

    private int index = 0;
    public Vector3 RandomPos
    {
        get
        {
            float angle = index * 36f * 3.14f / 180f;
            var r = 1 + index * 0.1f;
            var v3 = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;
            index++;
            return v3;
        }
    }
    public async void OnClone()
    {
        var id = heroIdList[roleDropDown.value];
        for (int i = 0; i < 100; i++)
        {
            var role = await CreateRole(id);
            if (role == null)
            {
                return;
            }
            role.gameObject.SetActive(true);
            role.transform.CustomSetParent(this.roleRoot.transform, this.RandomPos);
            _roleInsList.Add(role);
        }
    }

    public void OnClear()
    {
        if (_roleInsList.Count > 0)
        {
            for (int i = 0; i < _roleInsList.Count; i++)
            {
                GameObject.Destroy(_roleInsList[i].gameObject);
            }
        }
        index = 0;
        _roleInsList.Clear();
    }

    //private RoleRender _focusRole;
    private void ResetAnimator()
    {
        if (curAnimtor)
        {
            curAnimtor.Play(null);
        }
    }

    public async void DirSelect(int _sel)
    {
        this.SwitchRoleList();
    }

    public async void RoleSelect(int _sel)
    {
        var id = heroIdList[_sel];
        if (!rolePrefDic.ContainsKey(id))
        {
            var role = await CreateRole(id);
            if (role == null)
            {
                return;
            }
            role.gameObject.SetActive((false));
            rolePrefDic[id] = role;
        }

        //Vector3 posRecord = Vector3.zero;
        if (curRs != null)
        {
            //posRecord = curRs.transform.localPosition;
            curRs.gameObject.SetActive(false);
        }

        curRs = rolePrefDic[id];
        curRs.gameObject.SetActive(true);
        curRs.transform.localPosition = Vector3.zero;
        _cameraControl.m_Target = curRs.transform;
        var pos = _cameraControl.transform.position;
        pos.y = curRs.transform.position.y;
        // curRs.transform.forward = Vector3.forward;
        //Debug.LogError("curRs.transform:"+ curRs.transform.position.ToDetailString()+"=>"+ pos.ToDetailString());
        curRs.transform.LookAt(pos);
        aniList.Clear();
        mAniDropDown.options.Clear();
        curAnimtor = curRs.GetComponent<Animator>();
        mAniDropDown.captionText.text = "";
        if (curAnimtor && curAnimtor.runtimeAnimatorController)
        {
            var animationClips = curAnimtor.runtimeAnimatorController.animationClips;
            var stateInfo = curAnimtor.GetCurrentAnimatorStateInfo(0);
            foreach (var animClip in animationClips)
            {
                aniList.Add(animClip.name);
                mAniDropDown.options.Add(new UnityEngine.UI.Dropdown.OptionData(animClip.name));
            }
            if (aniList.Count > 0)
            {
                mAniDropDown.captionText.text = aniList[0];
            }
        }
        curSubAnimator.Clear();
        Animator[] animatorS = curRs.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animatorS.Length; i++)
        {
            if (animatorS[i] != curAnimtor)
            {
                curSubAnimator.Add(animatorS[i]);
            }
        }
    }

    async Task<RoleRender> CreateRole(string modelId)
    {
        //Debug.Log("moduleId: " + modelId);
        var address = roleDataDic[modelId];
        //Debug.Log("address: " + address);
        var bucket = BucketManager.Stuff.Tool;
        var prefab = await bucket.GetOrAquireAsync<GameObject>(address);
        //Debug.Log("prefab: " + prefab);

        if (prefab == null)
        {
            Debug.LogError("address is null:"+ address);
            return null;
        }
        var role = GameObject.Instantiate(prefab);
        Debug.Log("role: " + role);

        if (role == null) return null;
        if (this.transform.parent != null)
        {
            role.transform.SetParent(this.roleRoot.transform);
            Debug.Log("SetParent");
        }
        role.transform.localPosition = Vector3.zero;
       // var pos = _cemeraC.transform.position;
       // pos.y = role.transform.position.y ;
       
       // role.transform.LookAt(pos);

        role.transform.localScale = Vector3.one;
        _cameraControl.m_Target = role.transform;
        var roleRender = role.GetOrAddComponent<RoleRender>();
        roleRender.Init();
        return roleRender;
    }

    private async void SwitchRoleList()
    {
        heroIdList.Clear();
        roleDropDown.options.Clear();
        roleDataDic.Clear();
        int val = this.dirDropDown.value;
        string dirType = this.dirList[val];
#if UNITY_EDITOR
         string dirPath = $"Assets/Arts/Models/{dirType}";
         string[] roles = AssetDatabase.FindAssets("t:prefab", new[] { dirPath });
         List<string> models = new List<string>();
         models.AddRange(new List<string>(roles));

         for (var i = 0; i < models.Count; ++i)
         {
             string path = AssetDatabase.GUIDToAssetPath(models[i]);
             string[] pathParts = path.Split('/');
             if (pathParts.Length == 0)
             {
                 Debug.LogError("invalid path:" + path);
                 continue;
             }
             if (!string.IsNullOrEmpty(this.inputField.text) && !path.Contains(this.inputField.text))
             {
                continue;
             }
             var modelName = pathParts[pathParts.Length - 1];

             heroIdList.Add(path);
             roleDropDown.options.Add(new UnityEngine.UI.Dropdown.OptionData(modelName));
             roleDataDic.Add(path, modelName);
         }
#else

        var bucket = BucketManager.Stuff.Tool;
        TextAsset asset = await bucket.GetOrAquireAsync<TextAsset>("ModelList.txt");
        Debug.Log(asset.text);
        string[] models = asset.text.Split(',');
        var count = models.Length;
        for (var i = 0; i < count; ++i)
        {
            string id = i.ToString();
            string path = models[i];
            if (!path.Contains(dirType)) continue;
            string[] pathParts = path.Split('/');
            if (pathParts.Length == 0)
            {
                Debug.LogError("invalid path:" + path);
                continue;
            }
            if (!string.IsNullOrEmpty(this.inputField.text) && !path.Contains(this.inputField.text))
            {
                continue;
            }
            var modelName = pathParts[pathParts.Length - 1];
            heroIdList.Add(path);
            roleDropDown.options.Add(new UnityEngine.UI.Dropdown.OptionData(modelName));
            roleDataDic.Add(path, modelName);
        }
#endif
        RoleSelect(0);
        roleDropDown.captionText.text = roleDataDic[heroIdList[0]];

    }
#if UNITY_EDITOR
    string[] dirList = new[] { "$Pose", "Boss", "Monster", "Role", "Weapon", "EnvItem" };
#else
    string[] dirList = new[] { "Boss", "Monster", "Role", "Weapon", "EnvItem" };
#endif
    public async Task Init()
    {

            await Task.Delay(1000);
           
            for (int i = 0; i < dirList.Length; i++)
            {
                string dir = dirList[i];
                dirDropDown.options.Add(new UnityEngine.UI.Dropdown.OptionData(dir));
            }
            DirSelect(0);
            dirDropDown.captionText.text = dirList[0];


    }

    public RoleRender CRR
    {
        get => curRs;
    }

    public void ResetAll()
    {
        ToolStatic.ResetAll();
    }

    public void HideAll()
    {
        ToolStatic.HideAll();
    }


    public Transform _lightTrans;
    public Transform _lightFxTrans;
    public Slider _mainLightRotateSlider;
    public Light _pointLight;
    public Light _mainLight;
    public Slider _rotateSlider;

    public float _rotateSpeed;
    public Slider _cameraZoomSlider;
    public UnityEngine.UI.Text _cameraZoomLabel;
    public CustomCameraController _cameraControl;
    public Slider _mainLightSlider;
    public Slider _subLightSlider;
    public Slider _cameraBiasXSlider;
    public Slider _cameraBiasYSlider;
    public Slider _cameraBiasZSlider;
    public UnityEngine.UI.Text _cameraBiasXLabel;
    public UnityEngine.UI.Text _cameraBiasYLabel;
    public UnityEngine.UI.Text _cameraBiasZLabel;
    public Slider _cameraHSlider;
    public UnityEngine.UI.Text _cameraHLabel;
    public Slider _cameraVSlider;
    public UnityEngine.UI.Text _cameraVLabel;

    public Slider _roleDirSlider;
    public Dropdown dirDropDown;
    public Dropdown roleDropDown;
    public Dropdown mAniDropDown;


    List<string> heroIdList = new List<string>();
   
    private List<string> aniList = new List<string>();
    Dictionary<string, string> roleDataDic = new Dictionary<string, string>();
    Dictionary<string, RoleRender> rolePrefDic = new Dictionary<string, RoleRender>();
    RoleRender curRs;
    private Animator curAnimtor;
    private List<Animator> curSubAnimator = new List<Animator>();

    private int curAniIndex = 0;
    //private int curRoleId = -1;

    //private List<RoleRender> _roleInsList = new List<RoleRender>();

    private Camera _camera;

    //private Dictionary<int, List<RoleRender>> _roleBank = new Dictionary<int, List<RoleRender>>();
    
}