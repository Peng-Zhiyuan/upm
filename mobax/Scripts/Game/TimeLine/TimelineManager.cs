/* Created:Loki Date:2023-03-29*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DG.Tweening;
using System;

public class TimelineManager : StuffObject<TimelineManager>
{
    private string skillTimelineRootPath = "SkillTimelineUnitRoot.prefab";
    private SkillTimelineRoot skillTimelineRoot;
    private Dictionary<string, SkillTimelineUnit> skillTimelineDic = new Dictionary<string, SkillTimelineUnit>();

    public async Task<SkillTimelineRoot> CreateSkillTimeRoot()
    {
        if (skillTimelineRoot != null)
        {
            return skillTimelineRoot;
        }
        skillTimelineDic.Clear();
        skillTimelineRoot = await GameObjectPoolUtil.ReuseAddressableObjectAsync<SkillTimelineRoot>(BucketManager.Stuff.Main, skillTimelineRootPath);
        if (skillTimelineRoot == null)
        {
            return null;
        }
        TransformUtil.InitTransformInfo(skillTimelineRoot.GameObject, Stuff.transform);
        skillTimelineRoot.Stop();
        return skillTimelineRoot;
    }

    public async Task<SkillTimelineUnit> LoadSkillTimelineAsync(int heroID, string avatarModelPath = "")
    {
        string path = StrBuild.Instance.ToStringAppend(heroID.ToString(), "timeline_Fight.prefab");
        if (skillTimelineDic.ContainsKey(path))
        {
            return skillTimelineDic[path];
        }
        SkillTimelineUnit TimeLine = await GameObjectPoolUtil.ReuseAddressableObjectAsync<SkillTimelineUnit>(BucketManager.Stuff.Main, path);
        if (TimeLine == null)
            return null;
        skillTimelineDic[path] = TimeLine;
        TimeLine.GameObject.SetActive(false);
        TimeLine.Transform.parent = Stuff.transform;
        TimeLine.Transform.position = Vector3.zero;
        TimeLine.Transform.localRotation = Quaternion.identity;
        if (TimeLine != null
            && TimeLine.AvatarObj == null)
        {
            string modelPath = avatarModelPath;
            if (string.IsNullOrEmpty(modelPath))
            {
                HeroRow heroRow = StaticData.HeroTable.TryGet(heroID);
                if (heroRow == null)
                {
                    return null;
                }
                modelPath = StrBuild.Instance.ToStringAppend(heroRow.Model, ".prefab");
                HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(heroID);
                if (heroInfo.Unlocked)
                {
                    modelPath = StrBuild.Instance.ToStringAppend(RoleHelper.GetAvatarName(heroInfo), ".prefab");
                }
            }
            await TimeLine.PlaySkillTimeLine(modelPath);
        }
        TimeLine.Director.timeUpdateMode = DirectorUpdateMode.GameTime;
        return TimeLine;
    }

    public async Task PlaySkillTimelineAsync(int heroID, string avatarModelPath = "")
    {
        Debug.Log($"PlaySkillTimelineAsync heroID: {heroID}, avatarModelPath: {avatarModelPath}");
        BlockManager.Stuff.AddBlock("PlaySkillTimelineAsync", BlockLevel.Invisible);
        //await BlockManager.Stuff.TransactionInAsync("skillTimeline");
        SkillTimelineRoot root = await CreateSkillTimeRoot();
        if (root == null)
        {
            return;
        }
        SkillTimelineUnit timeline = await LoadSkillTimelineAsync(heroID, avatarModelPath);
        if (timeline == null)
        {
            return;
        }
        //onLoaded?.Invoke(timeline);
        TransformUtil.InitTransformInfo(timeline.GameObject, root.roleRoot);
        UIEngine.Stuff.IsPageLayerEnabled = false;
        BlockManager.Stuff.TransactionOut("skillTimeline");
        root.Play();
        Camera timelineCamera = timeline.GetComponentInChildren<Camera>();
        if (timelineCamera != null)
        {
            timelineCamera.clearFlags = CameraClearFlags.Skybox;
        }
        CameraCustomData timelineCameraCustomData = timeline.GetComponentInChildren<CameraCustomData>();
        if (timelineCameraCustomData != null)
        {
            timelineCameraCustomData.DepthHeightFog = new CameraCustomData.FogSetting() { openFog = false };
        }
        timeline.gameObject.SetActive(true);
        timeline.Director.Play();
        var transactionInDuration = 250;
        var timelineDuration = (int)(timeline.Director.duration * 1000);
        var waitDuration = timelineDuration - transactionInDuration;
        //Debug.Log($"timelineDuration: {timelineDuration}, transactionInDuration: {transactionInDuration}, waitDuration: {waitDuration}");
        await Task.Delay(waitDuration);
        await BlockManager.Stuff.TransactionInAsync("skillTimeline");
        //await Task.Delay(outTransactionDuration);
        BlockManager.Stuff.TransactionOut("skillTimeline");
        //await Task.Delay(timelineDuration);
        timeline.Director.Stop();
        timeline.gameObject.SetActive(false);
        UIEngine.Stuff.IsPageLayerEnabled = true;
        root.Stop();
        BlockManager.Stuff.RemoveBlock("PlaySkillTimelineAsync");
    }

    public void ReleaseSkillTimelineUnit()
    {
        foreach (var VARIABLE in skillTimelineDic)
        {
            BucketManager.Stuff.Main.Release(VARIABLE.Key);
            DestroyImmediate(VARIABLE.Value);
        }
        if (skillTimelineRoot != null)
        {
            DestroyImmediate(skillTimelineRoot.GameObject);
            skillTimelineRoot = null;
            BucketManager.Stuff.Main.Release(skillTimelineRootPath);
        }
        skillTimelineDic.Clear();
    }

    private void ShowDark(Camera rootCamera, Camera cam, float durT = 0.2f)
    {
        if (cam == null)
            return;
        var cadata = cam.transform.GetComponent<CameraCustomData>();
        if (cadata == null)
        {
            Debug.LogError("大招镜头缺少CameraCustomData组件");
            return;
        }
        var main_data = rootCamera.GetComponent<CameraCustomData>();
        if (main_data != null)
        {
            cadata.colorAdjustments = main_data.colorAdjustments;
            cadata.EnvLUT = main_data.EnvLUT;
        }
        cadata.RadiaBlur.openSceneBlur = true;
        cadata.colorAdjustments.openColorAdjustments = true;
        cadata.colorAdjustments.fadeScene = 0f;
        DOTween.To(() => cadata.colorAdjustments.fadeScene, x => { cadata.colorAdjustments.fadeScene = x; }, 0.9f, durT).OnComplete(() => { });
    }
}