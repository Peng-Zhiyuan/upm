using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleSystem.Core;

public enum StateEmotion
{
    Idle,
    Alert,
    Flee,
    Patrol,
    Sos,
}
public class EmoteRoot : MonoBehaviour
{
    private RecycledGameObject emoteObject = null;
    public  async void UpdateEmote(StateEmotion emotion)
    {
        if (emoteObject != null)
        {
            BucketManager.Stuff.Battle.Pool.Recycle(emoteObject);
        }
        if (emotion == StateEmotion.Idle) return;
        emoteObject = await EmotionManager.Instance.ShowEmotePanel(this.transform,emotion);
    }

    public async void UpdateEmote(int emotion)
    {
        this.UpdateEmote((StateEmotion)emotion);
    }

}
