/* Created:Loki Date:2023-02-20*/

using System.Threading.Tasks;

namespace BattleEngine.View
{
    using Logic;
    using UnityEngine;
    using System.Collections.Generic;

    public sealed class SkillFlowerBulletActionTaskData : AbilityTaskData
    {
        public string effectPath;
        public List<string> attachPath = new List<string>();
        public int FlyFrame;
        public int hideFrame;
        public List<int> AttackFrameLst = new List<int>();
        public List<int> ChangePosFrameLst = new List<int>();

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.FlowerBulletEffect;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem _element)
        {
            base.Init(_element);
            FlowerBulletActionElement element = (_element as FlowerBulletActionElement);
            effectPath = AddressablePathConst.SkillEditorPathParse(element.effectPath);
            attachPath.AddRange(element.attachPath);
            FlyFrame = element.FlyFrame;
            AttackFrameLst.AddRange(element.AttackFrameLst);
            ChangePosFrameLst.AddRange(element.ChangePosFrameLst);
            hideFrame = element.HideFrame;
        }
    }

    public sealed class SkillFlowerBulletActionViewTask : AbilityViewTask
    {
        private SkillFlowerBulletActionTaskData _taskData;
        public SkillFlowerBulletActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as SkillFlowerBulletActionTaskData;
                }
                return _taskData;
            }
        }

        private CombatActorEntity owner;
        private FlowerBulletEffect bulletEffect;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            owner = SkillAbilityExecution.OwnerEntity;
            if (bulletEffect == null)
            {
                await CreateEffect();
            }
            bulletEffect.SetActive(true);
            Creature targetCreature = BattleManager.Instance.ActorMgr.GetActor(owner.targetKey);
            if (targetCreature == null)
            {
                return;
            }
            TransformUtil.InitTransformInfo(bulletEffect.gameObject, targetCreature.transform);
            Creature ownerCreature = BattleManager.Instance.ActorMgr.GetActor(owner.UID);
            List<Transform> initTransLst = new List<Transform>();
            for (int i = 0; i < TaskData.attachPath.Count; i++)
            {
                Transform initTrans = ownerCreature.GetBone(TaskData.attachPath[i], 0);
                initTransLst.Add(initTrans);
            }
            bulletEffect.InitBulletTrans(initTransLst, targetCreature.gameObject, ownerCreature);
            bulletEffect.ChangePos(TaskData.FlyFrame * BattleLogicDefine.LogicSecTime);
        }

        private int curFrame;

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            if (bulletEffect == null)
            {
                return;
            }
            curFrame = frameIdx - TaskData.startFrame;
            for (int i = 0; i < TaskData.AttackFrameLst.Count; i++)
            {
                if (curFrame == TaskData.AttackFrameLst[i])
                {
                    bulletEffect.OnAttack();
                }
            }
            for (int i = 0; i < TaskData.ChangePosFrameLst.Count; i++)
            {
                if (curFrame == TaskData.ChangePosFrameLst[i])
                {
                    bulletEffect.ChangePos(0.5f);
                }
            }
            if (curFrame == TaskData.hideFrame)
            {
                bulletEffect.OnDestroyBullet();
            }
        }

        public override void BreakExecute(int frameIdx)
        {
            if (bulletEffect != null)
                GameObject.DestroyImmediate(bulletEffect.gameObject);
            base.BreakExecute(frameIdx);
        }

        public async Task CreateEffect()
        {
            if (TaskData == null)
            {
                return;
            }
            string EffectPrefabName = TaskData.effectPath;
            GameObject fx = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(EffectPrefabName);
            GameObject obj = GameObject.Instantiate(fx);
            bulletEffect = obj.GetComponent<FlowerBulletEffect>();
        }
    }
}