//ybzuo-ro
//动态碰撞结果
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Detectors;
    public class DynCollResult
    {
		public static int _copmass(DynCollResult _l,DynCollResult _r)
		{
			if(_l.time>_r.time)
			{
				return 1;
			}
			else if(_l.time<_r.time)
			{
				return -1;
			}
			else	
			{
				return 0;
			}
		}
		public ObscuredBool hit;
        public PhyObjectType object_type;
		public ObscuredFloat time;
        public Vector3 position;
		public Vector3 dir;
		public Vector3 reflex;
		public Vector3 speed;
    }
    public struct SphereCollResult
    {
		public ObscuredInt id;
		public ObscuredFloat distance;
    }
