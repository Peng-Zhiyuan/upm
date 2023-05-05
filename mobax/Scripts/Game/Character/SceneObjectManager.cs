using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleEngine.Manager;
using BattleEngine.View;
using BattleSystem.Core;

public class SceneObjectManager : BattleComponent<SceneObjectManager>
{
    public const string TAG_SCENE_OBJECTS = "SceneObjects";
    public const string PROBE_GROUP_ROOT = "ProbeGroup";
    public const string TAG_SCENE_EFFECTOBJECTS = "SceneEffectObjects";

    //场景中的角色
    //private Dictionary<string, Creature> _creatures = new Dictionary<string, Creature>();

    private LocalPlayerCamera _localPlayerCamera = null;

    //private Dictionary<string, Creature> _selectPlayers = new Dictionary<string, Creature>();

    //场景中的交互物件
    // private Dictionary<string, Creature> _items = new Dictionary<string, Creature>();

    //场景内对象根节点
    private Transform m_RootTransform;
    private Transform m_ProbeRootTransform;
    public Transform GetRootTransform
    {
        get { return m_RootTransform; }
    }

    //场景内特效对象根节点
    private Transform m_EffectRootTransform;
    public Transform GetEffectRootTransform
    {
        get { return m_EffectRootTransform; }
    }

    private string _CurrentSelectPlayerID;

    public BattleActorManager actMgr;

    // 存放光照探针信息的根节点
    public Transform ProbeRoot => m_ProbeRootTransform == null ? GameObject.FindWithTag(PROBE_GROUP_ROOT).transform : m_RootTransform;

    public override void OnBattleCreate()
    {
        m_RootTransform = GameObject.FindWithTag(TAG_SCENE_OBJECTS).transform;
        m_EffectRootTransform = GameObject.FindWithTag(TAG_SCENE_EFFECTOBJECTS).transform;

        //actMgr = BattleEngine.Logic.BattleManager.Instance.ActorMgr;
        //EventManager.Instance.AddListener<SkillAbilityExecution>("BattleSkillBeginExecution", OnSpellSkillPoint);
        EventManager.Instance.AddListener<SkillAbilityExecution>("OnEndSkillPointPoint", OnEndSkillPointPoint);
    }

    public override void LateUpdate()
    {
        float tmp_deltaTime = Time.deltaTime;
        if (_localPlayerCamera != null)
            _localPlayerCamera.LateUpdate(tmp_deltaTime);
    }

    public override void OnUpdate()
    {
        float param_deltaTime = Time.deltaTime;
        if (_localPlayerCamera != null)
            _localPlayerCamera.Update(param_deltaTime);
    }

    public override void OnDestroy()
    {
        if (_localPlayerCamera != null)
        {
            GameObject.Destroy(_localPlayerCamera.gameObject);
            _localPlayerCamera = null;
        }
        //EventManager.Instance.RemoveListener<SkillAbilityExecution>("BattleSkillBeginExecution", OnSpellSkillPoint);
        EventManager.Instance.RemoveListener<SkillAbilityExecution>("OnEndSkillPointPoint", OnEndSkillPointPoint);
    }

    public void OnEndSkillPointPoint(SkillAbilityExecution combatAction)
    {
        if (combatAction.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
        {
            var role = Find(combatAction.OwnerEntity.UID);
            if (role != null)
            {
                CameraManager.Instance.CameraProxy.StopSpellSkill(role);
            }
        }
    }

    /*public void OnSpellSkillPoint(SkillAbilityExecution combatAction)
    {
         if (combatAction.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL && combatAction.OwnerEntity.isAtker)
        {
            var role = Find(combatAction.OwnerEntity.UID);
            if (role != null)
            {
                CameraManager.Instance.CameraProxy.PlaySpellSkill(role);
            }
        }
    }*/

    /// <summary>
    /// 当没有本地玩家时，创建本地玩家
    /// 如果已存在，仅返回已存在的本地玩家
    /// </summary>
    /// <returns></returns>
    public LocalPlayerCamera TryCreateLocalPlayerCamera()
    {
        if (_localPlayerCamera == null)
        {
            _localPlayerCamera = new LocalPlayerCamera("LocalPlayerCamera");
            _localPlayerCamera.gameObject.name = "LocalPlayerCamera";
            //_localPlayerCamera.SetGroundPosition(Vector3.zero);
            _localPlayerCamera.SetDirection(-Vector3.right);
            _localPlayerCamera.gameObject.layer = LayerMask.NameToLayer("Role");
        }
        return _localPlayerCamera;
    }

    public LocalPlayerCamera LocalPlayerCamera
    {
        get
        {
            if (_localPlayerCamera == null)
                TryCreateLocalPlayerCamera();
            return _localPlayerCamera;
        }
    }

    public Creature GetPlayer(string param_ID)
    {
        return actMgr.GetActor(param_ID);
    }

    public Creature[] GetAllPlayer()
    {
        return actMgr.GetAllActors().ToArray();
    }

    public bool MonsterAllDie()
    {
        return true;
    }

    public Creature[] GetAllCreatures()
    {
        return actMgr.GetAllActors().ToArray();
    }

    public Creature FindCreatureByConfigID(int configID)
    {
        Creature tmp_Creature = null;
        foreach (var VARIABLE in actMgr.GetAllActors())
        {
            if (VARIABLE.mData.ConfigID == configID)
            {
                tmp_Creature = VARIABLE;
                break;
            }
        }
        return tmp_Creature;
    }

    public Creature GetSelectPlayer()
    {
        return CurSelectHero;
    }
    
    public Creature CurSelectHero = null;

    public void SetSelectPlayer(Creature creature)
    {
        if (CurSelectHero != null)
            CurSelectHero.OnUnSelected();
        CurSelectHero = creature;
        if (creature != null)
            creature.OnSelected();
        GameEventCenter.Broadcast(GameEvent.SelectPlayer);
        GameEventCenter.Broadcast(GameEvent.SelectChanged, CurSelectHero);
        
        BattleManager.Instance.BattleInfoRecord.AddSwitchCameraNum();
    }

    public void CleanSelectPlayers()
    {
        foreach (var creature in actMgr.GetAllActors())
        {
            if (creature.Selected)
                creature.OnUnSelected();
        }
        GameEventCenter.Broadcast(GameEvent.SelectPlayer);
    }

    public bool AllEnemyDie()
    {
        foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetCamp(1))
        {
            if (!VARIABLE.mData.isAtker
                && !VARIABLE.mData.IsDead)
            {
                return false;
            }
        }
        return true;
    }

    public Creature Find(string param_ID, bool bPlayer = false)
    {
        return actMgr.GetActor(param_ID);
    }

    public Creature Find(GameObject go)
    {
        foreach (var tmp_pairs in actMgr.GetAllActors())
        {
            Creature temp = tmp_pairs;
            if (temp.gameObject == go)
            {
                return temp;
            }
        }
        return null;
    }

    private Dictionary<int, GameObject> PreModels = new Dictionary<int, GameObject>();

   
   
    public void SetObjectLayer(string layer)
    {
        foreach (var VARIABLE in actMgr.GetAllActors())
        {
            var rendersArray = VARIABLE.gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rendersArray.Length; i++)
            {
                rendersArray[i].gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }
    }
}