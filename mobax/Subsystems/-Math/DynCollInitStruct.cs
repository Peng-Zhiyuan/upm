//ybzuo-dena
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Detectors;
    public class DynCollInitStruct
    {
        public PhyObjectType object_type=PhyObjectType.LAND;
        public Vector3 init_pos = new Vector3(0.0f, 0.0f,0.0f);
        public Vector3 init_speed = new Vector3(0.0f, 0.0f,0.0f);
        public Vector3 acceleration = new Vector3(0.0f, -1000.0f,0.0f);
		public ObscuredFloat init_time = 0.0f;
		public ObscuredFloat finish_time=0.0f;
		public Vector3 hit_pos;
        public override string ToString()
        {
            return "object_type:" + object_type + " init_pos:" + init_pos + " init_speed:" + init_speed + " acc:" + acceleration + " init_time:"
			+ init_time+" finish_time:"+finish_time+" hit_pos:"+hit_pos;
        }
//        public static bool operator ==(DynCollInitStruct _lv, DynCollInitStruct _rv)
//        {
//            if (_lv.init_pos != _rv.init_pos ||
//                _lv.init_speed != _rv.init_speed ||
//                _lv.acceleration != _rv.acceleration
//                )
//            {
//                return false;
//            }
//            else
//            {
//                return true;
//            }
//        }
//        public static bool operator !=(DynCollInitStruct _lv, DynCollInitStruct _rv)
//        {
//            return !(_lv == _rv);
//        }
//        public override bool Equals(object obj)
//        {
//            if (obj is DynCollInitStruct)
//            {
//                return this == (DynCollInitStruct)obj;
//            }
//            else
//            {
//                return false;
//            }
//        }
//        public override int GetHashCode()
//        {
//            return init_pos.GetHashCode() ^ init_speed.GetHashCode() ^ init_time.GetHashCode();
//        }
    }
