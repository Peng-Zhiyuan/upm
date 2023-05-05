// using BattleEngine.Logic;
// using BattleEngine.Logic;
// using System;
// using System.Collections.Generic;
// using System.Linq.Expressions;
// using UnityEngine;

// namespace ComboSystem.Factory {
//     public partial class ComboController {
//         public class Combo {
//             public class Express {
//                 public string name;
//                 public object[] args;
//                 public Express(string _name, params object[] _args) {
//                     this.name = _name.Trim(' ');
//                     this.args = _args;
//                 }
//             }
//             public class Segment {
//                 public string name;
//                 public Express action;
//                 public int status;
//                 public string skillName;
//                 public SkillAbility ability;
//                 public SkillAbilityExecution abilityExecution;

//                 public bool Cooling { get { return !ability.CooldownTimer.IsRunning; } }
//                 public bool Running { get { return ability.Spelling; } }

//             }

//             public string Name { get; set; }

//             public int CastType { get; set; }
//             public Express ForeAction { get; set; }
//             public List<Express> PickFilters { get; set; }



//             public int CastWeight { get; set; }
//             public int CoolDown { get; set; }
//             public int CoolDownMax { get; set; }
//             public List<Segment> Segments { get; set; } = new List<Segment>();
//             public List<Segment> References { get; set; } = new List<Segment>();
//             public Segment Executing { get; set; }
//             public Combo(string _name) {
//                 this.Name = _name;
//             }
//         }
//         public Combo Executing { get; set; }
//         public Combo.Segment Running { get; set; }
//         /// <summary>
//         /// 主动连段
//         /// </summary>
//         public Dictionary<int, Combo> ActiveCombos { get; set; } = new Dictionary<int, Combo>();
//         /// <summary>
//         /// 被动连段
//         /// </summary>
//         public Dictionary<int, Combo> PassiveCombos { get; set; } = new Dictionary<int, Combo>();
//         public ComboController() {

//         }
//         public void Break(CombatActorEntity localContext) {

//         }
//         private void BreakRunning(Combo.Segment running) {

//         }
//         public void Clean() {
//             Running = null;
//             Executing = null;
//         }
//         public void Update() {

//         }

//         public void MakeCombo(ComboRow row) {

//             Combo combo = new Combo(row.name);
//             combo.Name = row.name;
//             combo.CoolDown = row.cd; // 暂定使用次数
//             combo.CoolDownMax = row.cd;
//             combo.CastWeight = row.weight;
//             combo.CastType = row.castType;
//             combo.ForeAction = this.Analysis(row.foreaction);
//             combo.PickFilters = new List<Combo.Express>();
//             combo.PickFilters.Add(this.Analysis("NOTCOOLING:1")); // 默认增加一个无CD表达式，参数暂时无用
//             for (int i = 0; i < row.filters.Count; i++) {
//                 string codeString = row.filters[i];
//                 Combo.Express express = this.Analysis(codeString);
//                 if (express != null) { combo.PickFilters.Add(express); }
//             }

//             for (int i = 0; i < row.segments.Count; i++) {
//                 string code = row.segments[i];
//                 if (string.IsNullOrEmpty(code)) { continue; }
//                 Combo.Segment segment = new Combo.Segment();
//                 segment.name = "";
//                 segment.action = this.Analysis(code);
//                 combo.Segments.Add(segment);
//             }
//             //UnityEngine.Debug.Log("error combo: " + row.name + ", row id:" + row.id);
//             if (row.castType == 0) {
//                 this.ActiveCombos.Add(row.id, combo);
//             } else {
//                 this.PassiveCombos.Add(row.id, combo);
//             }
//         }

//         public Combo.Express Analysis(string code) {
//             if (string.IsNullOrEmpty(code)) { return null; }
//             string[] temp = code.Split(':');
//             string name = temp[0];
//             object[] args = null;
//             // ���Բ�������
//             if (temp.Length > 1) { args = temp[1].Split(','); }
//             return new Combo.Express(name, args);
//         }
//         /// <summary>
//         /// 通过筛选来挑选技能序列
//         /// </summary>
//         /// <param name="localContext"></param>
//         /// <returns></returns>
//         public Combo PickByFilters(CombatActorEntity localContext, int castType = 0) {
//             if (localContext.battleItemInfo.id == 53001) {
//                 //int a = 0;
//             }
//             Dictionary<int, Combo> tempCombos;
//             if (castType == 0) {
//                 // 主动行动序列
//                 tempCombos = this.ActiveCombos;
//             } else {
//                 // 被动行动序列
//                 tempCombos = this.PassiveCombos;
//             }
//             List<Combo> pickedCombos = new List<Combo>();
//             // 通过筛选
//             foreach (var temp in tempCombos.Values) {
//                 int result = 0;
//                 if (temp.CoolDown <= 0) { continue; }
//                 //string log = "";
//                 for (int i = 0; i < temp.PickFilters.Count; i++) {
//                     result += TODOManager.ReadAsJudge(localContext, temp.PickFilters[i]);
//                     //log += $" <{temp.PickFilters[i].name}: {result}> ";
//                 }
//                 //if (localContext.battleItemInfo.id == 53001)
//                 //{
//                 //    UnityEngine.Debug.Log($"Pick: {temp.Name}, with log: {log}");
//                 //}

//                 if (result == 0) {
//                     pickedCombos.Add(temp);
//                 }
//             }
//             if (pickedCombos.Count == 0) {
//                 //throw new Exception($"maybe have not enough \"combos\" within actor[{localContext.Name}]");
//                 return null;
//             }

//             int totalWeight = 0;
//             List<int> weights = new List<int>();
//             for (int i = 0; i < pickedCombos.Count; i++) {
//                 int weight = pickedCombos[i].CastWeight;
//                 totalWeight += weight;
//                 weights.Add(weight);// 添加权重
//             }

//             //先临时卸载这里
//             List<int[]> sections = this.MakeRoulette(weights);

//             int seedWeight = UnityEngine.Random.Range(0, totalWeight);
//             int sectionIndex = -1;
//             for (int i = 0; i < sections.Count; i++) {
//                 int[] section = sections[i];
//                 int below = section[0];
//                 int above = section[1];
//                 if (seedWeight >= below && seedWeight < above) {
//                     sectionIndex = i;
//                     break;
//                 }
//             }
//             Combo pickedCombo = null;
//             pickedCombo = pickedCombos[sectionIndex];
//             this.ComboReady(ref pickedCombo);

//             Executing = pickedCombo;// 暂存选中结果
//             if (localContext.battleItemInfo.id == 53001) {
//                 Debug.Log($"boss 挑选 技能：{Executing.Name}, 段数：{Executing.Segments.Count}");
//             }
//             return pickedCombo;
//         }
//         /// <summary>
//         /// temp get lockon type
//         /// </summary>
//         /// <returns></returns>
//         public string GetLockOnType() {
//             if (Executing == null) {
//                 throw new Exception("did not pick Executing combo.......");
//             }
//             if (Executing.ForeAction == null) {
//                 return "NEAR";
//             }
//             object[] args = Executing.ForeAction.args;
//             string argString = "";
//             if (args != null && args.Length != 0) { argString = args[0] as string; }
//             if (string.IsNullOrEmpty(argString)) {
//                 argString = "NEAR";
//             }
//             return argString;
//         }
//         public void DoAutomic(CombatActorEntity localContext, int auto = 0) {
//             if (Executing == null) {
//                 // 这段留下以防万一
//                 Executing = this.PickByFilters(localContext);

//                 if (Executing == null) {
//                     return;
//                 }
//             }

//             if (Running == null) {
//                 //暂不删除出列项目
//                 Running = POPSegment(Executing, false);
//                 if (Running == null) {
//                     Running = null;
//                     Executing = null;
//                     return;
//                 }
//             } else {
//                 return;
//             }

//             TODOManager.ReadAsTODO(localContext, Running.action);
//         }

//         public void DoImmediatly(CombatActorEntity localContext) {
//             Executing = this.PickByFilters(localContext);
//             Running = POPSegment(Executing, true);
//             TODOManager.ReadAsTODO(localContext, Running.action);
//         }

//         /// <summary>
//         /// 出列一个段
//         /// </summary>
//         /// <param name="sequence"></param>
//         /// <param name="fromIndex"></param>
//         /// <param name="remove"></param>
//         /// <returns></returns>
//         private Combo.Segment POPSegment(Combo sequence, bool remove = true) {
//             Combo.Segment segment = null;
//             if (sequence.References.Count == 0)
//                 return null;
//             segment = sequence.References[0];
//             if (remove) { sequence.References.RemoveAt(0); }
//             return segment;
//         }




//         /// <summary>
//         /// 构造轮盘
//         /// </summary>
//         /// <param name="weights"></param>
//         /// <returns></returns>
//         private List<int[]> MakeRoulette(List<int> weights) {
//             if (weights.Count == 0) { throw new Exception("weights list must have length....."); }
//             // 创建起点，第一个必须是0
//             if (weights[0] != 0) { weights.Insert(0, 0); }
//             // ---------------------------【暂】递加操作
//             for (int i = 0; i < weights.Count; i++) {
//                 // 当前扇区，当前扇区尾部加下个扇区头部
//                 int curWeight = weights[i];
//                 int nextStartIndex = i + 1;
//                 if (nextStartIndex >= weights.Count) {
//                     break;
//                 }
//                 for (int j = nextStartIndex; j < weights.Count; j++) {
//                     weights[j] += curWeight;
//                 }
//             }
//             // --------------------------- 构造扇区
//             List<int[]> sections = new List<int[]>();
//             for (int i = 0; i < weights.Count; i++) {
//                 int indexNow = i;
//                 int indexNext = i + 1;
//                 int lastIndex = weights.Count - 1;
//                 int[] section = new int[2];
//                 if (indexNow == lastIndex) {
//                     // 最后一个
//                 } else {
//                     section[0] = weights[indexNow];
//                     section[1] = weights[indexNext];
//                     sections.Add(section);
//                 }
//             }
//             return sections;
//         }

//         private void ComboReady(ref Combo combo) {
//             combo.CoolDown--;// 准备成功一次减少一次
//             combo.References.Clear();
//             for (int i = 0; i < combo.Segments.Count; i++) {
//                 combo.References.Add(combo.Segments[i]);
//             }
//         }
//     }

//     /// <summary>
//     /// ҵ��������ݶ���Comboϵͳ
//     /// </summary>
//     public class TODOManager {
//         public static Dictionary<string, TODO> TODOs = new Dictionary<string, TODO>();
//         public static void Initialize() {
//             TODOs.Add("NOTCOOLING", new NOTCOOLING());
//             TODOs.Add("NOTRUNNING", new NOTRUNNING());
//             TODOs.Add("NOTEXISTPART", new NOTEXISTPART());
//             TODOs.Add("EXISTPART", new EXISTPART());
//             TODOs.Add("HPRATE", new HPRATE());
//             TODOs.Add("PLACE", new PLACE());
//             TODOs.Add("LOCKON", new LOCKON());
//             TODOs.Add("PHASE", new PHASE());
//             TODOs.Add("PLAY", new PLAY());
//         }
//         public static int ReadAsJudge(object localContext, ComboController.Combo.Express express) {
//             if (express == null)
//                 return 0; // 没有表达式，默认成功
//             string funcName = express.name;
//             object[] args = express.args;
//             if (TODOs.ContainsKey(funcName)) {
//                 return TODOs[funcName].Execute(localContext, args);
//             }
//             return 0;
//         }
//         public static void ReadAsTODO(object localContext, ComboController.Combo.Express express) {
//             string funcName = express.name;
//             object[] args = express.args;
//             if (TODOs.ContainsKey(funcName)) {
//                 TODOs[funcName].Execute(localContext, args);
//             }
//         }
//     }
//     #region ϵͳ��ҵ����չ����
//     public abstract class Chunk {
//         public int id;
//         public string name;
//     }

//     /// <summary>
//     /// Ҫ����ҵ�����������ģ�
//     /// </summary>
//     public abstract class TODO : Chunk {
//         public abstract int Execute(object context, object[] args);
//     }


//     /// <summary>
//     /// ����ҵ��
//     /// </summary>
//     public class PLAY : TODO {
//         public override int Execute(object context, object[] args) {
//             var actor = context as CombatActorEntity;

//             uint skillID = (uint)int.Parse(args[0] as string); // ���ܲ�λ
//             SkillAbility skillConfigure = null;
//             if (actor.SkillSlots.ContainsKey(skillID)) {
//                 skillConfigure = actor.SkillSlots[skillID];
//             } else {
//                 actor.ComboController.Running = null;
//                 return 0;
//             }
//             actor.SetActionState(ACTOR_ACTION_STATE.ATK);
//             actor.norAtkCD = 1;
//             actor.isAtking = true;


//             var action = actor.CreateCombatAction<SpellSkillAction>();
//             action.SkillAbility = skillConfigure; // ����������������
//             action.SkillAbilityExecution = action.SkillAbility.CreateAbilityExecution() as SkillAbilityExecution; // ��������ִ���߼�
//             action.SkillAbilityExecution.onOver = () => {

//                 actor.ComboController.Running = null;
//             };

//             action.SkillAbilityExecution.onBreak = () => {

//             };

//             if (!string.IsNullOrEmpty(actor.targetKey)) {
//                 action.SkillAbilityExecution.InputCombatEntity = BattleLogicManager.Instance.BattleData.allActorDic[actor.targetKey];
//             } else {
//                 action.SkillAbilityExecution.InputCombatEntity = actor;
//             }


//             action.SpellSkill(); // �ͷż��� 
//             action.SkillAbility.CooldownTimer.Reset(); // ����CD
//             return 0;
//         }
//     }


//     // 默认创建业务
//     public class NOTCOOLING : TODO {
//         public override int Execute(object context, object[] args) {
//             return 0;
//         }
//     }
//     public class NOTRUNNING : TODO {
//         public override int Execute(object context, object[] _Args) {
//             var actor = context as CombatActorEntity;
//             return actor.ComboController.Executing.Executing.Running ? 1 : 0;
//         }
//     }

//     public class PHASE : TODO {
//         public override int Execute(object context, object[] args) {
//             var actor = context as CombatActorEntity;
//             int nowPhase = 1;//BattleManager.Instance.TempCurrentBattleStatus;
//             int targetPhase = int.Parse(args[0] as string);
//             return nowPhase == targetPhase ? 0 : 1;
//         }
//     }

//     public class EXISTPART : TODO {
//         public override int Execute(object context, object[] args) {
//             var actor = context as CombatActorEntity;
//             string partName = args[0] as string;

//             string[] targetParts = new string[] { partName };
//             int result = BattleLogicManager.Instance.BattleData.IsBrokeBossPart(actor.UID, targetParts);
//             return result == 0 ? 0 : 1;
//         }
//     }
//     public class NOTEXISTPART : TODO {
//         public override int Execute(object context, object[] args) {
//             var actor = context as CombatActorEntity;
//             string partName = args[0] as string;
//             string[] targetParts = new string[] { partName };
//             int result = BattleLogicManager.Instance.BattleData.IsBrokeBossPart(actor.UID, targetParts);
//             return result == 1 ? 0 : 1;
//         }
//     }
//     public class PLACE : TODO {
//         public override int Execute(object context, object[] args) {
//             var actor = context as CombatActorEntity;
//             string targetPlace = args[0] as string;
//             string actorPlace = "";
//             switch (actor.Action_actorActionState) {
//                 case ACTOR_ACTION_STATE.Fly:
//                     actorPlace = "air";
//                     break;
//                 default:
//                     actorPlace = "ground";
//                     break;
//             }
//             if (string.IsNullOrEmpty(actorPlace)) {
//                 throw new Exception("error actor place with [null]..........");
//             }
//             return targetPlace == actorPlace ? 0 : 1;
//         }
//     }
//     public class HPRATE : TODO {
//         public override int Execute(object context, object[] args) {
//             var actor = context as CombatActorEntity;
//             float limitRate = float.Parse(args[0] as string) / 1000F;
//             float rate = actor.CurrentHealth.Percent();
//             return rate <= limitRate ? 0 : 1;
//         }
//     }

//     /// <summary>
//     /// 锁定并切换目标
//     /// </summary>
//     public class LOCKON : TODO {
//         public override int Execute(object context, object[] args) {
//             //第一次释放暂时不能锁定
//             //var actor = context as CombatActorEntity;
//             //string argString = "";
//             //if (args != null && args.Length != 0) { argString = args[0] as string; }
//             //CombatActorEntity targetActor;
//             //switch (argString)
//             //{
//             //    case "DPS":
//             //        // 找到全部的ADC
//             //        targetActor = BattleLogicManager.Instance.BattleData.GetHighestATKActor(actor);
//             //        if (actor.battleItemInfo.id == 53001)
//             //        {
//             //            Debug.Log($"Boss 朝向 DPS 目标 {targetActor.battleItemInfo.GetHeroRow().name}");
//             //        }
//             //        break;
//             //    default:
//             //        targetActor = BattleLogicManager.Instance.BattleData.GetNearestTarget(actor);
//             //        if (actor.battleItemInfo.id == 53001)
//             //        {
//             //            Debug.Log($"Boss 朝向最近目标 {targetActor.battleItemInfo.GetHeroRow().name}");
//             //        }
//             //        break;
//             //}
//             //if (targetActor != null)
//             //{
//             //    actor.SetTargetInfo(targetActor);
//             //    UnityEngine.Vector3 direction = (targetActor.GetPosition() - actor.GetPosition()).normalized;
//             //    direction.y = 0;
//             //    actor.SetForward(direction);
//             //    return 0;
//             //}
//             //else
//             //{
//             //    return 1;
//             //}

//             Debug.Log("LOCKON temp remove.........");
//             return 0;
//         }

//     }
//     #endregion
// }



