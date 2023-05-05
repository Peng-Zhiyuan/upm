//ybzuo-ro
using UnityEngine;
using System.Collections.Generic;















    public enum RoundSegResultType2D
    {
        TWO,
        HEAD,
        TAIL,
        NULL,
    };
    public struct RaySegResult2D
    {
        public bool success;
        public Vector2 position;
        public float distance;
    }
    public struct RoundSegResult2D 
    {
        public RoundSegResultType2D result;
        public Vector2 postion1;
        public Vector2 postion2;
    }

	public class GPolygon
	{
		public List<Vector3> nodes=new List<Vector3>();
	}




    public class GeometryMath
    {

		public static bool PolygonCol(GPolygon _a,GPolygon _b)
		{
			return true;
		}

		



        public static float kdot(Vector2 _l, Vector2 _r)
        {
            return _l.x * _r.x + _l.y * _r.y;
        }
        public static float kcross(Vector2 _l, Vector2 _r)
        {
            return _l.x * _r.y - _r.x * _l.y;
        }
        public static float kmin(float _l, float _r)
        {
            if (_l <= _r)
            {
                return _l;
            }
            else
            {
                return _r;
            }
        }
        public static float kmax(float _l, float _r)
        {
            if (_l >= _r)
            {
                return _l;
            }
            else
            {
                return _r;
            }
        }
        public static Vector2 kreflect(Vector2 enter_vec, Vector2 normal_vec)
        {
            Vector2 enter_vec_op = enter_vec;
            enter_vec_op = new Vector2(-enter_vec_op.x,enter_vec_op.y);
            Vector2 n_vec = normal_vec * kdot(enter_vec_op, normal_vec);
            Vector2 tt = enter_vec + n_vec;
            Vector2 target_vec = tt * 2.0f - enter_vec;
            target_vec = target_vec.normalized;
            return target_vec;
        }
        public static int gm_find_intersection(float u0, float u1, float v0, float v1, float[] w)
        {
            if (u1 < v0 || u0 > v1)
            {
                return 0;
            }
            if (u1 > v0)
            {
                if (u0 < v1)
                {
                    if (u0 < v0)
                    {
                        w[0] = v0;
                    }
                    else
                    {
                        w[0] = u0;
                    }
                    if (u1 > v1)
                    {
                        w[1] = v1;
                    }
                    else
                    {
                        w[1] = u1;
                    }
                    return 2;
                }
                else
                {
                    w[0] = u0;
                    return 1;
                }
            }
            else
            {
                w[0] = u1;
                return 1;
            }
        }
        const float sqrEpsilon = 0.0001f;
        public static RaySegResult2D gm_raycast_segment(Vector2 ro, Vector2 rd,Vector2 so, Vector2 sd)
        {
            RaySegResult2D temp_result;
            temp_result.position.x = temp_result.position.y = 0.0f;
            temp_result.success = false;
            temp_result.distance = 0.0f;
            //_ro为射线原点，_rd为射线方向，_so为线段原点，_sd为线段方向向量（非标，直接尾-头得来）
            Vector2 rdn = rd;
            rdn = rdn.normalized;
            Vector2 E = so - ro;
            float _kross = kcross(rdn, sd);
            float sqr_kross = _kross * _kross;
            float sqrLen0 = rdn.x * rdn.x + rdn.y * rdn.y;
            float sqrLen1 = sd.x * sd.x + sd.y * sd.y;
            if (sqr_kross > sqrEpsilon * sqrLen0 * sqrLen1)
            {
                //lines are not parallel
                float s = kcross(E, sd) / _kross;
                if (s>=0)
                {
                    float t = kcross(E, rdn) / _kross;
                    if ( t>=0&&t<=1)
                    {
                        temp_result.position = ro + rdn * s;
                        temp_result.success = true;
                        temp_result.distance =s;
                        //UnityEngine.Debug.Log(temp_result.position.get_str_tip());
                    }
                }
            }
            return temp_result;
        }
        public static bool gm_segment_segment(Vector2 p0, Vector2 d0, Vector2 p1,Vector2 d1, out Vector2 _result)
        {
            _result = new Vector2(0.0f, 0.0f);
            Vector2 E = p1 - p0;
            float _kross = kcross(d0, d1);
            float sqr_kross = _kross * _kross;
            float sqrLen0 = d0.x * d0.x + d0.y * d0.y;
            float sqrLen1 = d1.x * d1.x + d1.y * d1.y;
            if (sqr_kross > (sqrEpsilon * sqrLen0 * sqrLen1))
            {
                //线段相交
                float s = (E.x * d1.y - E.y * d1.x) / _kross;
                if ((s < 0.0f) || (s > 1.0f))
                {
                    return false;
                }
                float t = (E.x * d0.y - E.y * d0.x) / _kross;
                if ((t < 0.0f) || (t > 1.0f))
                {
                    return false;
                }
                _result = p0 + d0 * s;
                return true;
            }
            //线段平行
            float sqrLenE = E.x * E.x + E.y * E.y;
            _kross = kcross(E, d0);
            sqr_kross = _kross * _kross;
            if (sqr_kross > sqrEpsilon * sqrLen0 * sqrLenE)
            {
                //线段不重叠
                return false;
            }
            else
            {
                //线段重叠
                float s0 = kdot(d0, E) / sqrLen0;
                float s1 = s0 + kdot(d0, d1) / sqrLen0;
                float smin = kmin(s0, s1);
                float smax = kmax(s0, s1);
                float[] w = new float[2];
                int imax = gm_find_intersection(0.0f, 1.0f, smin, smax, w);
                if (imax > 0)
                {
                    _result = p0 + d0 * w[0];
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
		public static bool gm_round_rect_test2d(Vector2 c,Vector2 h,Vector2 p,float r)
		{
		    //知乎 Milo Yip
			Vector2 v = new Vector2(Mathf.Abs(p.x-c.x),Mathf.Abs(p.y-c.y)); // 第1步：转换至第1象限
			Vector2 u = new Vector2(Mathf.Max(v.x - h.x, 0),Mathf.Max(v.y - h.y, 0)); // 第2步：求圆心至矩形的最短距离矢量
			return u.x*u.x+u.y*u.y<= r * r; // 第3步：长度平方与半径平方比较
		}
		public static bool gm_round_rect_test3d(Vector3 c,Vector3 h,Vector3 p,float r)
		{
//			//知乎 Milo Yip
//			Vector3 v = new Vector3(Mathf.Abs(p.x-c.x),Mathf.Abs(p.y-c.y),Mathf.Abs(p.z-c.z)); // 第1步：转换至第1象限
//			Vector3 u = new Vector3(Mathf.Max(v.x - h.x, 0),Mathf.Max(v.y - h.y, 0),Mathf.Max(v.z-h.z,0)); // 第2步：求圆心至矩形的最短距离矢量
//			return u.x*u.x+u.y*u.y+u.z*u.z<= r * r; // 第3步：长度平方与半径平方比较;
			bool _xz=gm_round_rect_test2d(new Vector2(c.x,c.z),new Vector2(h.x,h.z),new Vector2(p.x,p.z),r);
			bool _xy=gm_round_rect_test2d(new Vector2(c.x,c.y),new Vector2(h.x,h.y),new Vector2(p.x,p.y),r);
			bool _yz=gm_round_rect_test2d(new Vector2(c.z,c.y),new Vector2(h.z,h.y),new Vector2(p.z,p.y),r);
			return _xz&&_xy&&_yz;
		}
        public static RoundSegResult2D gm_round_segment(Vector2 _c, float _r, Vector2 _so, Vector2 _sd)
        {
            RoundSegResult2D rs_result;
            rs_result.result = RoundSegResultType2D.NULL;
            rs_result.postion1 = new Vector2(0.0f, 0.0f);
            rs_result.postion2 = new Vector2(0.0f, 0.0f);
      
            Vector2 temp_vec = _so - _c;
            float d_dot_v = _sd.x * temp_vec.x + _sd.y * temp_vec.y;
            float if_root = (d_dot_v * d_dot_v) - _sd.magnitude *(temp_vec.magnitude - _r * _r);
            if (if_root > 0)
            {
                //相交于两点。
                float t1 = ((-d_dot_v) - UnityEngine.Mathf.Sqrt(if_root)) / _sd.magnitude;
                float t2 = ((-d_dot_v) + UnityEngine.Mathf.Sqrt(if_root)) / _sd.magnitude;
                if (t1 >= 0.0f && t1 <= 1.0f && t2 >= 0.0f && t2 <= 1.0f)
                {
                    rs_result.postion1 =_so + _sd * t1;
                    rs_result.postion2=_so + _sd * t2;
                    rs_result.result = RoundSegResultType2D.TWO;
                }
                else if (t1 >= 0.0f && t1 <= 1.0f)
                {
                    rs_result.postion1 = _so + _sd * t1;
                    rs_result.result = RoundSegResultType2D.HEAD;
                }
                else if (t2 >= 0.0f && t2 <= 1.0f)
                {
                    rs_result.postion2 = _so + _sd * t2;
                    rs_result.result = RoundSegResultType2D.TAIL;
                }
            }
            return rs_result;
        }

		public static Vector3 GetRoundPoint(Vector3 _center,float _radius,float _deg)
		{
			float _rad=_deg*Mathf.Deg2Rad;
			Vector3 _tpos;
			_tpos.y=0;
			_tpos.x=_center.x+_radius*Mathf.Cos(_rad);
			_tpos.z=_center.z+_radius*Mathf.Sin(_rad);
			return _tpos;
		}

		public static float GetRoundDeg(Vector3 _center,float _radius,Vector3 _pos)
		{
			return UnityEngine.Mathf.Acos((_pos.x-_center.x)/_radius)*UnityEngine.Mathf.Rad2Deg;
		}

		public static bool PointInRect2D(Vector2 _center,Vector2 _size,Vector2 _pos)
		{
			if(Mathf.Abs(_pos.x-_center.x)>_size.x*0.5)
			{
				return false;
			}
			if(Mathf.Abs(_pos.y-_center.y)>_size.y*0.5)
			{
				return false;
			}
			return true;
		}
		



		
		public static bool IsInPolygon(Vector3 _pos, Vector3[] vertex, int num)
		{
			if( num < 3 )
				return false;
			
			int count = 0;
			for (int i = 0; i < num; i++)
			{
				Vector3 p1 = new Vector2( vertex[i].x, vertex[i].z);
				Vector3 p2 = new Vector2(vertex[(i+1) % num].x, vertex[(i+1) % num].z );
				if( p2.z == p1.z)
					continue;
				if( _pos.z < p2.z && _pos.z < p1.z)
					continue;
				if( _pos.z > p2.z && _pos.z > p1.z)
						continue;
				float xx = (_pos.z - p1.z) * ( p2.x - p1.x)/(p2.z - p1.z) + p1.x;
				if( xx >= _pos.x )
						count++;
			}
			
			return (count % 2 == 1);
		}


    }
