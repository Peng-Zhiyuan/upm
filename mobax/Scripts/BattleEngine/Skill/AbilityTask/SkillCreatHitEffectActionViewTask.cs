namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class CreatHitEffectTaskData : AbilityTaskData
    {
        public int SkillID;
        public string hitFxPrefabName;
        public Vector3 fxScale;
        public string attachPoint = "";
        public Vector3 offset = Vector3.zero;
        public Vector3 euler = Vector3.zero;
        public string hitAuidoName;
        public float hitAuidoVolume = 1.0f;
        public string hitAnim = "";

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.HitEffect;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CreateHitEffectActionElement hitEffectElement = element as CreateHitEffectActionElement;
            if (hitEffectElement == null)
            {
                return;
            }
            attachPoint = hitEffectElement.attachPoint;
            offset = hitEffectElement.offset;
            fxScale = hitEffectElement.resScale;
            euler = hitEffectElement.euler;
            hitFxPrefabName = AddressablePathConst.SkillEditorPathParse(hitEffectElement.res);
            hitAuidoName = AddressablePathConst.SkillEditorPathParse(hitEffectElement.audio);
            hitAuidoVolume = hitEffectElement.volume;
            if (fxScale == null)
            {
                fxScale = Vector3.one;
            }
            hitAnim = hitEffectElement.hitAim;
        }
    }
}