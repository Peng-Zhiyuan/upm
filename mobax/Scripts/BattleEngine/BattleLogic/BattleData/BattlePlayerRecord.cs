/* Created:Loki Date:2022-11-25*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class BattlePlayerRecord
    {
        public string _id;
        public string KillMeUID = "";
        public int id;
        public List<string> killList = new List<string>();

        public BattlePlayerRecord(string uid, int id)
        {
            _id = uid;
            KillMeUID = "";
            this.id = id;
            killList.Clear();
        }

        private float op_AttackValue; //伤害输出
        public int OP_AttackValue
        {
            get { return Mathf.FloorToInt(op_AttackValue); }
            set { op_AttackValue = value; }
        }
        private int op_CureValue; //治疗输出
        public int OP_CureValue
        {
            get { return op_CureValue; }
            set { op_CureValue = value; }
        }
        private float receiveDamageValue; //承受伤害
        public int ReceiveDamageValue
        {
            get { return Mathf.FloorToInt(receiveDamageValue); }
            set { receiveDamageValue = value; }
        }
        private int receiveCureValue; //接收治疗
        public int ReceiveCureValue
        {
            get { return receiveCureValue; }
            set { receiveCureValue = value; }
        }
    }
}