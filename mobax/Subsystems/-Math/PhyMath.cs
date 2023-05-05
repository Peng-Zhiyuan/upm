//ybzuo-ro
using UnityEngine;
namespace MathDL
{
	public class PhyMath
    {
        public static float g_gravity_value = 9.8f;
        public static DynCollInitStruct MakeDCIS(Vector3 speed)
        {
            DynCollInitStruct _dcis = new DynCollInitStruct();
            _dcis.acceleration.y = -g_gravity_value;
            _dcis.init_speed = speed;
            //UnityEngine.Debug.Log("初始速度:" + _dcis.init_speed + " G:" + g_gravity_value);
            return _dcis;
        }
		public static DynCollInitStruct MakeDcisFromTargetDeg(Vector3 _bpos,Vector3 _epos,float _deg,bool _first)
		{
			Vector3 _dir=_epos-_bpos;
			_dir.y=0;
			_dir.Normalize();
//			Quaternion _rot=Quaternion.LookRotation(_dir);
			//Vector3 _hit_pos=MatchAniHelper.GetSingle().get_hit1_pos("shotfa_r",m_player.get_pos(),_rot,Vector3.one);
			
			//		Debug.Log("pos:"+m_player.get_pos()+"rot:"+_rot.eulerAngles+"scale:"+m_player.get_scale()+"hit pos:"+CoreDef.ani_hit_pos_dic["shotfa_r"]+"Shot Pos:"+_hit_pos);
			float _y = _epos.y - _bpos.y;
			_dir = _epos - _bpos;
			_dir.y = 0;
			float _x = _dir.magnitude;
			
			CombineRad _cr = new CombineRad(Quaternion.LookRotation(_dir).eulerAngles.y * Mathf.Deg2Rad, _deg * Mathf.Deg2Rad);
			
			float _p = PhyMath.GetPower(_x, _y, _cr);
			if (_p < 0.0f)
			{
				_p = -_p;
			}
			DynCollInitStruct _dcis=PhyMath.MakeDCIS(_cr.to_vector() * _p);
			_dcis.init_pos=_bpos;


			float _otime1=9999.0f;
			float _otime2=9999.0f;
			
			if(PhyMath.GetDtFromAdsV1(_dcis.acceleration.y,_epos.y-_bpos.y,_dcis.init_speed.y,out _otime1,out _otime2))
			{
				if((_otime1<0.0f)&&(_otime2>0.0f))
				{
					_dcis.finish_time=_otime2;
				}else if((_otime1>0.0f)&&(_otime2<0.0f))
				{
					_dcis.finish_time=_otime1;
				}
				else
				{
					if(_first)
					{	
						_dcis.finish_time=Mathf.Min(_otime1,_otime2);
					}
					else
					{
						_dcis.finish_time=Mathf.Max(_otime1,_otime2);
					}
				}
			}
			else
			{
				Debug.LogWarning("No Result!!!!");
			}
			_dcis.hit_pos=_epos;

		//Debug.Log(_dcis.init_speed);

			return _dcis;
		}
		
        public static float GetPower(float _x,float _y,CombineRad _rad)
        {
			if(_x==0.0f)
			{
				if(_y==0.0f)
				{
					return 0.0f;
				}
				else	
				{
					float _r1,_r2;
					bool _r=GetDtFromAdsV1(g_gravity_value,_y,0.0f,out _r1,out _r2);
					if(_r)
					{
						float _rtime=_r1<_r2?_r1:_r2;
						return GetV2FromV1aDt(0.0f,g_gravity_value,_rtime);
					}
					else	
					{
						return 0.0f;
					}
				}
			}
			else
			{
				return _x*Mathf.Sqrt(g_gravity_value/(2*_x*Mathf.Sin(_rad.v_rad)*Mathf.Cos(_rad.v_rad)-2*_y*Mathf.Cos(_rad.v_rad)*Mathf.Cos(_rad.v_rad)));
			}
        }
        public static Vector3 Plerp(DynCollInitStruct _mis, float _time)
        {
            float ds_x = 0.0f;
            float ds_y = 0.0f;
            float ds_z = 0.0f;
            Vector3 v2 = _mis.init_speed + _mis.acceleration * _time;
            if (Mathf.Abs(_mis.acceleration.x) > 0.1f)
            {
                ds_x = (v2.x * v2.x - _mis.init_speed.x * _mis.init_speed.x) / (_mis.acceleration.x * 2.0f);
            }
            else
            {
                ds_x = v2.x * _time;
            }
            if (Mathf.Abs(_mis.acceleration.z) > 0.1f)
            {
                ds_z = (v2.z * v2.z - _mis.init_speed.z * _mis.init_speed.z) / (_mis.acceleration.z * 2.0f);
            }
            else
            {
                ds_z = v2.z * _time;
            }
            if (Mathf.Abs(_mis.acceleration.y) > 0.1f)
            {
                ds_y = (v2.y * v2.y - _mis.init_speed.y * _mis.init_speed.y) / (_mis.acceleration.y * 2.0f);
            }
            else
            {
                ds_y = v2.y * _time;
            }
            //ds_y = (v2.y * v2.y - _mis.init_speed.y * _mis.init_speed.y) / (_mis.acceleration.y * 2.0f);
            Vector3 temp = new Vector3(ds_x, ds_y,ds_z);
            return temp + _mis.init_pos;
        }
        public static bool GetDtFromAdsV1(float a, float ds, float v1, out float _result1, out float _result2)
        {
            _result1 = _result2 = -1.0f;
            if (a == 0.0f)
            {
                _result1 = _result2 = ds / v1;
                return true;
            }
            //推出二元一次方程式
            //at*t+2v1t-2ds=0
            //判别式
            float sqrt_value = v1 * v1 + 2.0f * a * ds;
            if (sqrt_value < -0.1f)
            {
                return false;
            }
            if (sqrt_value < 0.0f)
            {
                sqrt_value = -sqrt_value;
            }
            float determinant = 2.0f * Mathf.Sqrt(sqrt_value);
            if (determinant < 0.0f)
            {
                return false;
            }
            else
            {
                _result1 = ((-2.0f * v1) + determinant) / (2.0f * a);
                _result2 = ((-2.0f * v1) - determinant) / (2.0f * a);
                return true;
            }
        }
        public static float GetV2FromV1aDt(float v1, float a, float dt)
        {
            return v1 + a * dt;
        }
		public static Vector3 GetV2FromV1aDt(Vector3 v1, float dt)
        {
			Vector3 _t=new Vector3();
			_t.x=v1.x;
			_t.z=v1.z;
			_t.y=v1.y-g_gravity_value*dt;
            return _t;
        }
        public static float GetDsFromV1V2Dt(float v1, float v2, float dt)
        {
            if (v1 + v2 == 0.0f)
            {
                return 0.0f;
            }
            return (dt / 2.0f) * (v1 + v2);
        }
        public static float GetDsFromV1V2A(float v1, float v2, float a)
        {
            if (a == 0.0f)
            {
                return 0.0f;
            }
            return (v2 * v2 - v1 * v1) / (a * 2.0f);
        }
        public static float GetDsFromV1aDt(float v1, float a, float dt)
        {
            float v2 = v1 + a * dt;
            return GetDsFromV1V2Dt(v1, v2, dt);
        }
    }
}