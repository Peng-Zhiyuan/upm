using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BattleTimelineManager : BattleComponent<BattleTimelineManager>
{
    //public const string TimelineResPath = "{0}.prefab";
    public TimelineUnit CurTimeLine;
    public TimelineProxy _proxy = new TimelineProxy();

    public void Init()
    {
        _proxy = new TimelineProxy();
    }

    public async void PlayAsync(string timelinePath, Creature obj, List<GameObject> targets = null, Action onEnd = null)
    {
        var cadata = CameraManager.Instance.MainCamera.GetComponent<CameraCustomData>();
        cadata.colorAdjustments.openColorAdjustments = true;
        cadata.colorAdjustments.fadeScene = 0.9f; //1-0.5, 0.5-0*/
        //var name = string.Format(TimelineResPath, timelineName);
        var timeline = await FetchAsync(timelinePath);
        if (timeline == null)
            return;
        CameraManager.Instance.MainCamera.gameObject.SetActive(false);
        //CameraManager.Instance.MainCamera.gameObject.SetActive(false);
        var prevLayer = timeline.gameObject.layer;
        timeline.transform.position = obj.SelfTrans.position;
        timeline.transform.localRotation = obj.SelfTrans.localRotation;

        // 增加打光层
        // timeline.gameObject.layer |= LayerMask.GetMask("TimelineLight");
        foreach (var creature in SceneObjectManager.Instance.GetAllCreatures())
        {
            if (creature.mData.IsDead)
                continue;
            creature.GetModelObject.SetActive(false);
        }
        _proxy.SetAsset(timeline.Director);
        //_proxy.SetBinding("Animation Track（2）", CameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>());
        //_proxy.SetBinding("Player", targetRole);
        //_proxy.SetBinding("PlayerAnimator", targetRole.transform.GetComponentInChildren<Animator>());

        //_proxy.SetBinding("Animation Track", CameraManager.Instance.MainCamera.GetComponent<CinemachineBrain>());
        //_proxy.SetBinding("Animation Track", targetRole);
        //_proxy.SetBinding("PlayerAnimator", targetRole.transform.GetComponentInChildren<Animator>());

        //var CMCamera = GameObject.Find("CM Camera");
        //CMCamera.SetActive(false);
        /*var CMCmaera1 = GameObject.Find("CM vcam1");
        Creature role1 = obj as Creature;
        CMCmaera1.GetComponent<CinemachineVirtualCamera>().LookAt = role1.GetBone("Bip001 Pelvis");

        var CinemachineTrack = _proxy.GetPlayableBinding("CMVCam1").sourceObject as GameObject;
        if (CinemachineTrack != null)
        {
            var com = CinemachineTrack.GetComponentInChildren<CinemachineVirtualCamera>();
            if (com != null)
            {
                Creature role = obj as Creature;
                
                com.LookAt = role.GetBone("Bip001 Pelvis");
            }
        }*/
        var isEnd = false;
        timeline.Director.stopped += (p) =>
        {
            isEnd = true;
            /*CameraManager.Instance.MainCamera.gameObject.SetActive(true);
            if (null != targetRole)
            {
                targetRole.SetActive(true);
            }
            
            foreach (var creature in SceneObjectManager.Instance.GetAllCreatures())
            {
                creature.gameObject.SetActive(true);
                //creature.RePlayAnim();
            }*/

            // 重置层级
            timeline.gameObject.layer = prevLayer;
            obj.GetModelObject.SetActive(true);
            //role.gameObject.SetActive(true);

            //role.Animator.transform.localPosition = Vector3.zero;
            //role.SetPosition(regionPos);
            //role.SetDirection(regionDir);
            //CMCamera.SetActive(true);
            GameObject.Destroy(timeline.gameObject);

            // TimeLineEnd(obj as Creature);
        };
        timeline.Director.Play();
        //timeline.Director.pl
        CurTimeLine = timeline;
        var i = 0;
        while (!isEnd)
        {
            await Task.Delay(100);
            if (i == 0)
            {
                //PostProcessHandler.Ins.BlitFeature.SetActive(false);
                i++;
            }
        }
        if (onEnd != null)
            onEnd();

        //AudioEngine.Stuff.ResumeBgm();
        AudioManager.ResumeBgm();

        // pzy:
        // 此代码已注释
        // 因为重构时发现此功能无效，判断为不使用的代码
        // AudioEngine.Stuff.StopSubBgm();
        timeline.Release();
        Debug.LogFormat("Timeline {0} is end", timelinePath);
    }

    public async void TimeLineEnd(Creature role)
    {
        //设置角色到focus
        var rendersArray = role.gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rendersArray.Length; i++)
        {
            rendersArray[i].gameObject.layer = LayerMask.NameToLayer("FOCUS");
        }

        //2.
        var sse = CameraManager.Instance.MainCamera.GetComponent<SSEffectCtrl>();
        var cadata = CameraManager.Instance.MainCamera.GetComponent<CameraCustomData>();
        //cadata.Focus.showFocusLayer = true;
        //cadata.Focus.fadeBlack = 1f;

        //2.
        /*cadata.colorAdjustments.openColorAdjustments = true;
        cadata.colorAdjustments.fadeScene = 0.5f; //1-0.5, 0.5-0*/
        //DOTween.instance.enabled
        /*
        var fadeBlack = cadata.Focus.fadeBlack;
        float durT = 0.3f;
        DOTween.To(() => fadeBlack, x => fadeBlack = x, 0.9f, durT).OnUpdate(() =>
                        {
                            //cadata.Focus.fadeBlack = fadeBlack;
                        }
        ).OnComplete(() => { });
        await Task.Delay(300);
        await Task.Delay(10000);
        DOTween.To(() => fadeBlack, x => fadeBlack = x, 0f, durT).OnUpdate(() => { cadata.colorAdjustments.fadeScene = fadeBlack; }).OnComplete(() => { cadata.colorAdjustments.openColorAdjustments = false; });
        await Task.Delay(300);
        cadata.Focus.showFocusLayer = false;
        */
        //Layer
        rendersArray = role.gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rendersArray.Length; i++)
        {
            rendersArray[i].gameObject.layer = LayerMask.NameToLayer("Role");
        }
        //role.gameObject.layer = LayerMask.NameToLayer("Role");
    }

    public Transform GetBone(Transform tmp_boneRoot, string name)
    {
        var tmp_bones = ListPool.ListPoolN<Transform>.Get();
        tmp_boneRoot.GetComponentsInChildren(tmp_bones);
        foreach (var tmp_bone in tmp_bones)
        {
            if (tmp_bone.name.Contains(name))
            {
                return tmp_bone;
            }
        }
        return null;
    }

    public void Stop()
    {
        if (CurTimeLine != null)
            CurTimeLine.Director.Stop();
    }

    async void SpecialProcessingAsync(TimelineUnit timeline)
    {
        if (timeline.name.StartsWith("TimelineTemplate"))
        {
            var camera = timeline.transform.Find("CameraRoot/Camera").GetComponent<Camera>();
            camera.cullingMask = 1027;
            //AudioEngine.Stuff.PlayBgm("js_aa1001_ultimate_bgm");
            //AudioManager.PlayBgmInBackground("js_aa1001_ultimate_bgm");
            //AudioEngine.Stuff.PauseBgm();

            // pzy:
            // 注意：在 PlayBgmInBackground 后立即调用 PauseBgm 不一定能正常工作
            AudioManager.PauseBgm();

            //AudioManager.Instance.PlaySeAsync("js_aa1001_ultimate_heartbeat");
            await Task.Delay(750);
            camera.cullingMask = LayerMask.GetMask("Cutscene");
        }
    }

    public async Task<TimelineUnit> FetchAsync(string timelineName)
    {
        var go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<TimelineUnit>(BucketManager.Stuff.Battle, timelineName);
        if (go != null)
        {
            go.transform.parent = null;
            return go;
        }
        return null;
    }

    public async void LoadTargetUnitAsync() { }
}