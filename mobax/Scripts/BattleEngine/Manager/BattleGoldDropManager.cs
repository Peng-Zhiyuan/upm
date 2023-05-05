/* Created:Loki Date:2022-10-11*/

namespace BattleEngine.View
{
    using UnityEngine;
    using Task = System.Threading.Tasks.Task;
    using System.Collections;
    using System.Collections.Generic;
    using Logic;

    public sealed class BattleGoldDropManager : StuffObject<BattleGoldDropManager>
    {
        public const string SingleGoldFXPath = "fx_gold.prefab"; //炸少量金币,body_hit
        public const string MoreGoldFXPath = "fx_gold_2.prefab"; //炸大量金币,body_hit
        public const string DeadGoldFXPath = "fx_gold_3.prefab"; //偷怪死亡炸出金币,foot,重复调用,持续3s
        public const string DeadGoldFX2Path = "fx_gold_3_2.prefab"; //偷怪死亡地面效果,foot
        public const string GetSingleGoldFXPath = "fx_gold_get.prefab"; //少量金币获取效果,body_hit
        public const string GetMoreGoldFXPath = "fx_gold_get_2.prefab"; //大量金币获取效果,body_hit

        public Dictionary<string, GoldDropCompent> goldDropCompentDic = new Dictionary<string, GoldDropCompent>();

        private DropRuleData _dropRuleData;
        private readonly float _speed = 5.0f; //米/秒
        private GoldDropCompent _stealMonster = null;
        private GoldDropCompent currentTarget = null;

        private GameObject UIGoldRoot;

        public void InitDropActor(BattleData data, DropRuleData dropRuleData)
        {
            goldDropCompentDic.Clear();
            _dropRuleData = dropRuleData;
            _stealMonster = null;
            currentTarget = null;
            List<CombatActorEntity> heroLst = data.atkActorLst;
            for (int i = 0; i < heroLst.Count; i++)
            {
                if (heroLst[i].CurrentLifeState == ACTOR_LIFE_STATE.LookAt)
                {
                    continue;
                }
                Creature actorCreature = BattleManager.Instance.ActorMgr.GetActor(heroLst[i].UID);
                if (actorCreature == null)
                {
                    continue;
                }
                GoldDropCompent compent = actorCreature.gameObject.AddComponent<GoldDropCompent>();
                compent.InitData(actorCreature, _dropRuleData, false);
                goldDropCompentDic[heroLst[i].UID] = compent;
            }
            List<CombatActorEntity> enemyLst = data.defActorLst;
            for (int i = 0; i < enemyLst.Count; i++)
            {
                Creature monsterCreature = BattleManager.Instance.ActorMgr.GetActor(enemyLst[i].UID);
                if (monsterCreature == null)
                {
                    continue;
                }
                GoldDropCompent compent = monsterCreature.gameObject.AddComponent<GoldDropCompent>();
                compent.InitData(monsterCreature, _dropRuleData, i == 0);
                goldDropCompentDic[enemyLst[i].UID] = compent;
                if (i == 0)
                {
                    _stealMonster = compent;
                }
            }
        }

        public async Task PreLoadFXRes()
        {
            await BucketManager.Stuff.Battle.AquireIfNeedAsync(SingleGoldFXPath);
            await BucketManager.Stuff.Battle.AquireIfNeedAsync(MoreGoldFXPath);
            await BucketManager.Stuff.Battle.AquireIfNeedAsync(DeadGoldFXPath);
            await BucketManager.Stuff.Battle.AquireIfNeedAsync(DeadGoldFX2Path);
            await BucketManager.Stuff.Battle.AquireIfNeedAsync(GetSingleGoldFXPath);
            await BucketManager.Stuff.Battle.AquireIfNeedAsync(GetMoreGoldFXPath);
        }

        public void ImmediateAddGold(Creature targetCreature)
        {
            if (goldDropCompentDic.ContainsKey(targetCreature.mData.UID))
            {
                GoldDropCompent goldDrop = goldDropCompentDic[targetCreature.mData.UID];
                goldDrop.AddGoldNum(_dropRuleData.HpToGoldValue);
            }
        }

        public async void PlayGoldFX(Creature originCreature, Creature targetCreature, string fxPath, bool isSingle)
        {
            if (currentTarget != null
                && _stealMonster != null
                && _stealMonster.OwnCreature.mData.CurrentHealth.Value > 0
                && currentTarget.OwnCreature.mData.UID != _stealMonster.OwnCreature.mData.UID
                && originCreature.mData.UID != _stealMonster.OwnCreature.mData.UID)
            {
                targetCreature = _stealMonster.OwnCreature;
                currentTarget = _stealMonster;
            }
            else
            {
                if (goldDropCompentDic.ContainsKey(targetCreature.mData.UID))
                {
                    currentTarget = goldDropCompentDic[targetCreature.mData.UID];
                }
            }
            if (currentTarget != null)
            {
                GameObject goldFx = await BattleResManager.Instance.CreatorFx(fxPath);
                StartCoroutine(IEPlayGoldFX(originCreature, targetCreature, isSingle, goldFx));
            }
        }

        private IEnumerator IEPlayGoldFX(Creature originCreature, Creature targetCreature, bool isSingle, GameObject goldFx)
        {
            Transform ownBodyHitTrans = originCreature.GetBone("body_hit");
            Transform targetBodyHitTrans = targetCreature.GetBone("body_hit");
            Vector3 initPos = ownBodyHitTrans != null ? ownBodyHitTrans.position : originCreature.SelfTrans.position;
            Vector3 targetPos = targetBodyHitTrans != null ? targetBodyHitTrans.position : targetCreature.SelfTrans.position;
            TransformUtil.InitTransformInfo(goldFx, null);
            goldFx.transform.position = initPos - Vector3.up * 0.2f;
            ParticleSystemPlayCtr ctr = goldFx.GetComponent<ParticleSystemPlayCtr>();
            float distance = Vector3.Distance(targetPos, initPos);
            float time = distance / _speed;
            ctr.SetSpeed(1.0f / (time + 0.5f));
            yield return new WaitForSeconds(0.3f);
            GoldFlyToUGUI(originCreature, targetCreature, ctr, time);
        }

        private async void GoldHitEffect(Creature targetCreature, string fxPath)
        {
            if (!goldDropCompentDic.ContainsKey(targetCreature.mData.UID))
            {
                return;
            }
            GoldDropCompent goldDrop = goldDropCompentDic[targetCreature.mData.UID];
            goldDrop.AddGoldNum(_dropRuleData.HpToGoldValue);
            GameObject hitFx = await BattleResManager.Instance.CreatorFx(fxPath);
            Transform targetBodyHitTrans = targetCreature.GetBone("body_hit");
            TransformUtil.InitTransformInfo(hitFx, null);
            hitFx.transform.position = targetBodyHitTrans.position;
            ParticleSystemPlayCtr ctr = hitFx.GetComponent<ParticleSystemPlayCtr>();
            ctr.Play();
            EventManager.Instance.SendEvent("GoldModeGetGold", GetGoldNum());
        }

        public async void PlayDeadFx(Creature originCreature, Creature targetCreature)
        {
            GameObject goldDeadFx = await BattleResManager.Instance.CreatorFx(DeadGoldFX2Path);
            TransformUtil.InitTransformInfo(goldDeadFx, null);
            goldDeadFx.transform.position = originCreature.SelfTrans.position;
            for (int i = 0; i < 10; i++)
            {
                PlayDeadLoopFx(originCreature, targetCreature);
                await Task.Delay(300);
            }
        }

        private async void PlayDeadLoopFx(Creature originCreature, Creature targetCreature)
        {
            GameObject goldFx = await BattleResManager.Instance.CreatorFx(DeadGoldFXPath);
            StartCoroutine(PlayDeadLoopFx(originCreature, targetCreature, goldFx));
        }

        private IEnumerator PlayDeadLoopFx(Creature originCreature, Creature targetCreature, GameObject goldFx)
        {
            Transform targetBodyHitTrans = targetCreature.GetBone("body_hit");
            Vector3 initPos = originCreature.SelfTrans.position;
            Vector3 targetPos = targetBodyHitTrans != null ? targetBodyHitTrans.position : targetCreature.SelfTrans.position;
            TransformUtil.InitTransformInfo(goldFx, null);
            goldFx.transform.position = initPos - Vector3.up * 0.2f;
            ParticleSystemPlayCtr ctr = goldFx.GetComponent<ParticleSystemPlayCtr>();
            float distance = Vector3.Distance(targetPos, initPos);
            float time = distance / _speed;
            ctr.SetSpeed(1.0f / (time + 0.5f));
            yield return new WaitForSeconds(0.3f);
            GoldFlyToUGUI(originCreature, targetCreature, ctr, time);
        }

        public int GetGoldNum()
        {
            int gold = 0;
            var data = goldDropCompentDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.OwnCreature.mData.isAtker)
                {
                    gold += data.Current.Value.GetGoldNum;
                }
            }
            return gold;
        }

        private void GoldFlyToUGUI(Creature originCreature, Creature targetCreature, ParticleSystemPlayCtr fx, float time)
        {
            if (!goldDropCompentDic.ContainsKey(targetCreature.mData.UID))
            {
                return;
            }
            GoldDropCompent goldDrop = goldDropCompentDic[targetCreature.mData.UID];
            goldDrop.AddGoldNum(_dropRuleData.HpToGoldValue);
            BattlePage battlePage = UIEngine.Stuff.FindPage("BattlePage") as BattlePage;
            if (battlePage == null)
            {
                BattleLog.LogError("Cant find BattlePage");
                return;
            }
            fx.Stop();
            fx.ToRecycle();
            Vector3 screenPos = CameraUtil.WorldPointToScreenPoint(originCreature.SelfTrans.position);
            Vector3 uiPos = CameraUtil.ScreenPointToUIPoint(UIEngine.Stuff.CanvasTransform, screenPos);
            battlePage.ExecuteGetGoldFx(uiPos);
        }
    }
}