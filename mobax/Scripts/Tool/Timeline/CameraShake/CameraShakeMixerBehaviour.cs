using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CameraShakeMixerBehaviour : PlayableBehaviour
{
    // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if (!trackBinding)
            return;
        // Retrieve the track time (playhead position) from the root playable.
        var rootPlayable = playable.GetGraph().GetRootPlayable(0);
        var time = (float)rootPlayable.GetTime();

        int inputCount = playable.GetInputCount ();
        var begin = float.MaxValue;
        var elasped = 0f;
        var duration = 0f;
        var intensity = 0f;

        for (int i = 0; i < inputCount; i++)
        {
            var clip = ((ScriptPlayable<CameraShakeBehaviour>)playable.GetInput(i)).GetBehaviour();
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<CameraShakeBehaviour> inputPlayable = (ScriptPlayable<CameraShakeBehaviour>)playable.GetInput(i);
            CameraShakeBehaviour input = inputPlayable.GetBehaviour ();

            // check begin
            if (clip.Clip != null) {
                var b = clip.Clip.start;
                var e = clip.Clip.end;
                if (inputWeight > 0 && b < begin) {
                    begin = (float)b;
                    duration = (float)e;
                    intensity = clip.Intensity;
                }
            }
        }
        elasped = time - begin;
        if (elasped < 0) {
            trackBinding.localPosition = Vector3.zero;
            return;
        }

        float factor = intensity;
        float t = elasped / duration;
        factor = Mathf.Lerp(intensity, 0f, t);

        mShakeOffset = UnityEngine.Random.insideUnitSphere * factor;
        if (duration > 0f) {
            if (elasped >= duration) {
                mShakeOffset = Vector2.zero;
                trackBinding.localPosition = Vector2.zero;
                //StopShake();
            }
        }

        if (trackBinding.gameObject.activeSelf) {
            trackBinding.localPosition += new Vector3(ShakeOffset.x, ShakeOffset.y, 0f);
        }
    }

    private Vector2 mShakeOffset;
    public Vector2 ShakeOffset
    {
        get { return mShakeOffset; }
    }
}
