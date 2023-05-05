// using System.Collections.Generic;
// using UnityEngine;
//
// namespace BattleEngine.View {
//
//     public class SiblingComparer : IComparer<Room> {
//         public int Compare(Room x, Room y) {
//             int xOrder = x.transform.GetSiblingIndex();
//             int yOrder = y.transform.GetSiblingIndex();
//             if (xOrder < yOrder) {
//                 return -1;
//             }
//             else if (xOrder > yOrder) {
//                 return 1;
//             }
//             else {
//                 return 0;
//             }
//         }
//     }
//
//     /// <summary>
//     /// 房间管理
//     /// 导航烘焙
//     /// 房间开关管理
//     /// 按照现在的游戏机制，以前的房间无法回去，且推动游戏进程的本质是Clear房间
//     /// </summary>
//     public sealed class BattleRoomManager {
//         private List<Room> m_Rooms;
//         public List<Room> Rooms {
//             get {
//                 // 查询并排序房间
//                 if (m_Rooms == null) {
//                     m_Rooms = MonoExt.SearchEntire<Room>();
//                     m_Rooms.Sort(new SiblingComparer());
//                 }
//                 return m_Rooms;
//             }
//         }
//         public Queue<Room> ActivedRooms { get; set; } = new Queue<Room>();
//         private int enteredMax = 2;
//         public Room Current { get; private set; }
//
//         /// <summary>
//         /// 进入 index 房间
//         /// </summary>
//         /// <returns></returns>
//         public Room EnterRoom(int index) {
//             Debug.Log($"进入房间 {index}");
//             Current = Rooms[index];
//             if (ActivedRooms.Count < enteredMax) {
//                 ActivedRooms.Enqueue(Current);
//             }
//             else {
//                 Debug.LogError($"进入的房间不能超过 {enteredMax} 个，请 ExitRoom()");
//             }
//             ShowNavHitRange(false, true);
//             return Current;
//         }
//
//         /// <summary>
//         /// 离开一个房间
//         /// </summary>
//         public Room ExitRoom() {
//             return ActivedRooms.Dequeue();
//         }
//
//         /// <summary>
//         /// 获取进入的房间
//         /// </summary>
//         /// <returns></returns>
//         public List<Room> GetActivedRooms() {
//             return new List<Room>(ActivedRooms.ToArray());
//         }
//
//         /// <summary>
//         /// 获取当前房间的边框
//         /// </summary>
//         /// <returns></returns>
//         public Bounds GetBounds() {
//             return new Bounds(Current.transform.position + Current.Volume.center, Current.Volume.size);
//         }
//
//         private bool isShowNavHitRanage = false;
//
//         public void ShowNavHitRange(bool isShow, bool force = false) {
//             if (Current == null || Current.NavHitRangeMesh == null)
//                 return;
//             if (isShowNavHitRanage == isShow && !force)
//                 return;
//             isShowNavHitRanage = isShow;
//             if (isShow)
//                 Current.NavHitRangeMesh.renderingLayerMask = 1;
//             else
//                 Current.NavHitRangeMesh.renderingLayerMask = 0;
//         }
//     }
//
// }

