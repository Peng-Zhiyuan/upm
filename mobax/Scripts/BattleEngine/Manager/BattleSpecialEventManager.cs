using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace BattleEngine.View
{
    public sealed class BattleSpecialEventManager : BattleComponent<BattleSpecialEventManager>
    {
        public List<SpecailEvent> Events = new List<SpecailEvent>();

        public override void OnBattleCreate()
        {
            Events.Add(new SpecailEvent() { type = SpecailEventType.HideUI, action_in = this.HideUI, action_out = this.ShowUI });
        }

        public void TriggerEvent(SpecailEventType type, float durTime)
        {
            foreach (var VARIABLE in Events)
            {
                if (VARIABLE.type == type)
                {
                    if (VARIABLE.durTime <= 0)
                        VARIABLE.action_in.Invoke();
                    VARIABLE.durTime += durTime;
                    break;
                }
            }
        }

        public void AddEvent(SpecailEventType type)
        {
            foreach (var VARIABLE in Events)
            {
                if (VARIABLE.type == type)
                {
                    if (VARIABLE.count <= 0)
                        VARIABLE.action_in.Invoke();
                    VARIABLE.count += 1;
                    break;
                }
            }
        }

        public void RemoveEvent(SpecailEventType type)
        {
            foreach (var VARIABLE in Events)
            {
                if (VARIABLE.type == type)
                {
                    if (VARIABLE.count > 0)
                    {
                        VARIABLE.count -= 1;
                        VARIABLE.action_out.Invoke();
                    }
                    break;
                }
            }
        }

        public void HideUI()
        {
            GameEventCenter.Broadcast(GameEvent.ShowUI, false);
        }

        public void ShowUI()
        {
            GameEventCenter.Broadcast(GameEvent.ShowUI, true);
        }

        public async void ShowDark(Camera cam, float durT = 0.2f)
        {
            if (cam == null)
                return;
            var cadata = cam.transform.GetComponent<CameraCustomData>();
            if (cadata == null)
            {
                BattleLog.LogError("大招镜头缺少CameraCustomData组件");
                return;
            }
            var main_data = CameraManager.Instance.MainCamera.GetComponent<CameraCustomData>();
            if (main_data != null)
            {
                cadata.colorAdjustments = main_data.colorAdjustments;
                cadata.EnvLUT = main_data.EnvLUT;
            }
            cadata.RadiaBlur.openSceneBlur = true;
            cadata.colorAdjustments.openColorAdjustments = true;
            cadata.colorAdjustments.fadeScene = 0f;
            DOTween.To(() => cadata.colorAdjustments.fadeScene, x => { cadata.colorAdjustments.fadeScene = x; }, 0.9f, durT).OnComplete(() =>
                            {
                                /*cadata.colorAdjustments.openColorAdjustments = false;*/
                            }
            );
        }

        public async void CloseDark(Camera cam)
        {
            if (cam == null)
                return;
            var cadata = cam.transform.GetComponent<CameraCustomData>();
            if (cadata == null)
            {
                BattleLog.LogError("大招镜头缺少CameraCustomData组件");
                return;
            }
            float durT = 0.2f;
            DOTween.To(() => cadata.colorAdjustments.fadeScene, x => { cadata.colorAdjustments.fadeScene = x; }, 0f, durT).OnComplete(() =>
                            {
                                cadata.RadiaBlur.openSceneBlur = false;
                                //cadata.colorAdjustments.openColorAdjustments = false;
                            }
            );
        }

        public async Task LowPlay()
        {
            CameraSetting.Ins.Shake(0.5f, 1f, 0.01f);
            ShowDark(CameraManager.Instance.MainCamera, 0.12f);
            await Task.Delay(10);
            float durT = 0.07f;
            float lastTimeScale = BattleDataManager.Instance.TimeScale;
            DOTween.To(() => lastTimeScale, x => { BattleDataManager.Instance.TimeScale = x; }, 0.05f, durT).OnComplete(() =>
                            {
                                BattleDataManager.Instance.TimeScale = lastTimeScale;
                                CloseDark(CameraManager.Instance.MainCamera);
                            }
            );
            GameEventCenter.Broadcast(GameEvent.ShowBreakCamera);
        }

        /*public override void OnUpdate()
        {
            var t = Time.deltaTime;
            foreach (var VARIABLE in Events)
            {
                if (VARIABLE.durTime > 0)
                {
                    VARIABLE.durTime -= t;
                    if (VARIABLE.durTime <= 0)
                    {
                        VARIABLE.action_out.Invoke();
                    }
                }
            }
        }*/
    }
}

namespace BattleEngine.View
{
    public enum SpecailEventType
    {
        HideUI = 1,
        Dark = 2,
    }

    public class SpecailEvent
    {
        public SpecailEventType type;
        public float durTime;
        public int count;
        public Action action_in;
        public Action action_out;
    }
}