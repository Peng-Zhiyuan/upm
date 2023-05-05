using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TransformTable
{
    BeginSkillAbility,
    EndSkillTimeline,
    EndSkillAbility,
    SkillHit,
    TimelineStart,
    TimelineEnd,
    MovieStart,
    SceneLoad,
    UiControls,
    UiOpen,
    UiClose,
    EffectStart,
    EffectEnd,

    //AudioClipSe,
    Custom,
    
    Voice, 
    HeroVoice_Dead,
    HeroVoice_Hit,
    Comics, // 漫画
    
    LockGears,//齿轮定住
    GearsRotating,//齿轮转动
}
