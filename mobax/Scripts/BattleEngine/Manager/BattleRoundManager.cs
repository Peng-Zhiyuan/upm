// using System.Collections.Generic;
// using UnityEngine;
//
// namespace BattleEngine.View {
//
//     /// <summary>
//     /// 战斗回合数据管理
//     /// </summary>
//     public sealed class BattleRoundManager {
//         /// <summary>
//         /// 房间战斗信息
//         /// </summary>
//         public class Round {
//             public int Index { get; private set; }
//             public int ID { get; private set; }
//             public List<Wave> Waves { get; set; } = new List<Wave>();
//
//             public Round(int index, int id) {
//                 Index = index;
//                 ID = id;
//             }
//         }
//
//         /// <summary>
//         /// 波次【索引/数据】
//         /// </summary>
//         public class Wave {
//             public int ID { get; set; }
//             // 默认锚点 GRID_6_8x5
//             public string Anchors { get; set; } = "GRID_6_8x5";
//             public List<Born> Borns { get; set; } = new List<Born>();
//
//             public Wave(int id) {
//                 ID = id;
//             }
//         }
//
//         /// <summary>
//         /// 出生信息【索引/数据】
//         /// </summary>
//         public class Born {
//             public int ID { get; set; }
//             public int Lv { get; set; }
//             public float Size { get; set; }
//             public int Actor { get; set; }
//
//             public Born(int id) {
//                 ID = id;
//             }
//
//             public Stage_bornRow bornRow;
//         }
//
//         private List<Round> Rounds { get; set; }
//         public Round NowRound { get; private set; }
//         public Wave NowWave { get; private set; }
//
//         public BattleRoundManager(int stageID) {
//             InitRoundManager(stageID);
//         }
//
//         public void InitRoundManager(int stageID) {
//             Debug.LogWarning("Create StageID");
//             // ---------- ---------- ---------- ----------
//             //              创建关卡数据模型/索引
//             // ---------- ---------- ---------- ----------
//             Rounds = new List<Round>();
//             // 安插一个 0 回合作为战斗编队回合
//             //Rounds.Add(new Round(0, -1));
//             StageRow stage_row = ProtoStaticData.StageTable[stageID];
//             for (int i = 0; i < stage_row.rooms.Count; i++) {
//                 int roomID = stage_row.rooms[i];
//                 // 列表长度索引
//                 int roomIndex = Rounds.Count;
//                 if (roomID == 0) continue; // 由于导表机制，roomID == 0 代表没有填写房间信息
//                 Round room = new Round(roomIndex, roomID);
//                 Rounds.Add(room);
//                 // ---------- ---------- ---------- ----------
//                 //              创建房间数据模型/索引
//                 // ---------- ---------- ---------- ----------
//                 Stage_roomRow room_row = ProtoStaticData.Stage_roomTable[roomID];
//                 for (int j = 0; j < room_row.waves.Count; j++) {
//                     int waveID = room_row.waves[j];
//                     if (waveID == 0) continue;
//                     // ---------- ---------- ---------- ----------
//                     //              创建出生数据模型/索引
//                     // ---------- ---------- ---------- ----------
//                     Stage_waveRow wave_row = ProtoStaticData.Stage_waveTable[waveID];
//                     Wave wave = new Wave(waveID);
//                     // 波次出生锚点
//                     wave.Anchors = wave_row.anchor_group;
//                     room.Waves.Add(wave);
//                     for (int m = 0; m < wave_row.borns.Count; m++) {
//                         int bornID = wave_row.borns[m];
//                         if (bornID == 0) {
//                             // 加个空站位，布阵房间专用
//                             wave.Borns.Add(null);
//                             continue;
//                         }
//                         Stage_bornRow born_row = ProtoStaticData.Stage_bornTable[bornID];
//                         Born born = new Born(bornID);
//                         born.Lv = born_row.lv;
//                         born.Size = born_row.size;
//                         born.Actor = born_row.mid;
//                         born.bornRow = born_row;
//                         wave.Borns.Add(born);
//                     }
//                 }
//             }
//         }
//
//         public Round CurrentRound() {
//             return NowRound;
//         }
//
//         public Round NextRound() {
//             if (Rounds == null || Rounds.Count == 0) {
//                 return null;
//             }
//             // 首次，这里【不加】容错
//             NowRound = this.Rounds[0];
//             // 出列一个对象
//             Rounds.RemoveAt(0);
//             return NowRound;
//         }
//
//         /// <summary>
//         /// 从当前回合获取波次
//         /// </summary>
//         /// <param name="round"></param>
//         /// <returns></returns>
//         public Wave NextWave() {
//             if (NowRound == null) {
//                 return null;
//             }
//             if (NowRound.Waves.Count <= 0) {
//                 return null;
//             }
//             // 首次，这里【不加】容错
//             NowWave = NowRound.Waves[0];
//             // 出列一个对象
//             NowRound.Waves.RemoveAt(0);
//             return NowWave;
//         }
//
//         public bool IsHaveNextRound() {
//             return Rounds.Count != 0;
//         }
//
//         public bool IsHaveNextWave() {
//             if (Rounds == null) {
//                 return false;
//             }
//             if (NowRound == null) {
//                 return false;
//             }
//             if (NowRound.Waves.Count <= 0) {
//                 return false;
//             }
//             return true;
//         }
//     }
//
// }

