using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/*public class ParticleEffectManager : Single<ParticleEffectManager>
{
    public  async Task<ParticleEffectUnit> PlayEffectAsync(int effId, Transform parent, Vector2 pos = default, bool disableSe = false)    {
        if(!StaticData.SkillEffectTable.ContainsKey(effId)) 
        {
            Debug.LogError("特效Id不存在:"+effId);
            return null;
        }
        //Debug.LogError("ParticleEffectManager PlayEffect:"+effId);
        var effectInfo = StaticData.SkillEffectTable[effId];
        // var delay = effectInfo.delay/30.0f;
        // await BattleUtil.Instance.DelaySeconedsAsync(delay);
        int sound = disableSe?0: effectInfo.sound;
        var eu = await  this.PlayEffectAsync(effectInfo.effect, parent, pos, effectInfo.loop, sound, effectInfo.delay);
        return eu;
    }

    public void PlayEffect(int effId, Transform parent, Vector2 pos = default)    {
        if(!StaticData.SkillEffectTable.ContainsKey(effId)) 
        {
            Debug.LogError("特效Id不存在:"+effId);
            return;
        }
        var effectInfo = StaticData.SkillEffectTable[effId];
        this.PlayEffectAsync(effectInfo.effect, parent, pos, effectInfo.loop, effectInfo.sound, effectInfo.delay);
    }

    public  async Task<ParticleEffectUnit> PlayRaycastAsync(int effId, Transform parent, Vector2 dir, Vector2 pos = default)    
    {
        var effectInfo = StaticData.SkillEffectTable[effId];
        var delay = effectInfo.delay/30.0f;
        await BattleHelper.Instance.DelaySeconedsAsync(delay);
        //string path = $"EffectsPrefab/Battle/{effectInfo.effect}";
        string path = effectInfo.effect + ".prefab";
        ParticleEffectUnit eu = null;
        //await AssetCacher.Stuff.LoadAndCacheAsync(path);
        var bucket = BuketManager.Stuff.GetBucket();
        await bucket.AquireIfNeedAsync(path);

        if (effectInfo.sound > 0 && StaticData.SoundConfTable.ContainsKey(effectInfo.sound))
        {
            await this.PreLoadEffectSe(effectInfo.sound);
            this.PlayEffectSe(effectInfo.sound);
        }
        eu = await GameObjectPoolUtil.ReuseAddressableObjectAsync<ParticleEffectUnit>(path);
        if(eu == null)
        {
            Debug.LogError("effect not loaded:"+path);
            return null;
        }

        //parent.localScale = Vector3.one;
        eu.transform.CustomSetParent(parent, pos);
        var angle = BattleHelper.Instance.VectorsToDegress(dir);

        eu.EffectRoot.rotation = Quaternion.Euler(0,0, angle);
        if(effectInfo.loop == 0)
        {
           await BattleHelper.Instance.DelayAsync(200);
        }
        this.RecylceDelay(eu,1000);
        return eu;
    }

     public async Task PreLoadEffectSe(int soundId)
     {
        if(!StaticData.SoundConfTable.ContainsKey(soundId)) return;
        string se = AudioManager.Stuff.GetLocationSound(soundId);
        if(string.IsNullOrEmpty(se)) return;
        await AudioManager.Stuff.PreloadSeAsync(se);
     }
     public async Task PlayEffectSe(int soundId)
     {
        if(!StaticData.SoundConfTable.ContainsKey(soundId)) return;
        var soundConf = StaticData.SoundConfTable[soundId];
        if(soundConf.delay > 0)
        {
            await BattleHelper.Instance.DelayAsync(soundConf.delay);
        }
        string se =  AudioManager.Stuff.GetLocationSound(soundId);
        if(string.IsNullOrEmpty(se)) return;
        AudioManager.Stuff.PlaySe(se);
        if(soundConf.count > 0)
        {
            int index = 0;
            while(index  < soundConf.count )
            {
                index++;
                await BattleHelper.Instance.DelayAsync(200);
                AudioManager.Stuff.PlaySe(se);
            }
        }
       
     }

     private async void RecylceDelay(ParticleEffectUnit eu, int delayTime = 0)
     {
        await BattleHelper.Instance.DelayAsync(delayTime);
        ParticleEffectManager.Instance.RecycleEffect(eu);
     }

    public  async Task PreloadEffectAsync(int effId) {
        if(!StaticData.SkillEffectTable.ContainsKey(effId)) 
        {
            return;
        }
        var effectInfo = StaticData.SkillEffectTable[effId];
        //string path = $"EffectsPrefab/Battle/{effectInfo.effect}";
        string path = effectInfo.effect + ".prefab";
        //await AssetCacher.Stuff.LoadAndCacheAsync(path);
        var bucket = BuketManager.Stuff.GetBucket();
        await bucket.AquireIfNeedAsync(path);
    }

    public  async Task<ParticleEffectUnit> PlayEffectAsync(string effName, Transform parent, Vector2 pos = default, int loop = 0, int sound = 0, int delayFrame = 0)    {
        if(string.IsNullOrEmpty(effName)) return null;
        //string path = $"EffectsPrefab/Battle/{effName}";
        string path = effName + ".prefab";
        //await AssetCacher.Stuff.LoadAndCacheAsync(path);
        var bucket = BuketManager.Stuff.GetBucket();
        await bucket.AquireIfNeedAsync(path);

        if (StaticData.SoundConfTable.ContainsKey(sound))
        {
            await this.PreLoadEffectSe(sound);
            this.PlayEffectSe(sound);
        }

        var delay = delayFrame/30.0f;
        await BattleHelper.Instance.DelaySeconedsAsync(delay);

        ParticleEffectUnit eu = await GameObjectPoolUtil.ReuseAddressableObjectAsync<ParticleEffectUnit>(path);
        if(eu == null)
        {
            Debug.LogError("effect not loaded:"+path);
            return null;
        }
        eu.transform.CustomSetParent(parent, pos);
        eu.transform.localEulerAngles = Vector3.zero;
        if(loop == 0)
        {
           await BattleHelper.Instance.DelayAsync(1000);
           this.RecycleEffect(eu);
           return null;
        }
         else return eu;
    }

    public void RecycleEffect(ParticleEffectUnit unit)
    {
        GameObjectPool.Stuff.Recycle(unit);
        return;
    }
}
*/