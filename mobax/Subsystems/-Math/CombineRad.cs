//yzbuo
//组合角
//先确定水平角，在确定垂直角（次序）
//水平角是从z正轴开始顺时针一圈360度(为了和u3d统一)
//垂直角是从"前方向"往后倒0到90度
using UnityEngine;
    public struct CombineRad
    {
        public float h_rad;
        public float v_rad;
        public CombineRad(float _h_rad,float _v_rad)
        {
            h_rad = _h_rad;
            v_rad = _v_rad;
        }
        public Vector3 to_vector()
        {
            Vector3 _temp_vec3;
            Vector2 _temp_xz_dir;
            //根据水平角求出xz平面上的dir---xz_dir
            _temp_xz_dir.x = Mathf.Sin(h_rad);
            _temp_xz_dir.y = Mathf.Cos(h_rad);
            _temp_xz_dir = _temp_xz_dir.normalized;
            //根据垂直角求出xz_dir的长度
            float _xz_lenght = Mathf.Cos(v_rad);
            //根据以上二者求出x,z值
           _temp_xz_dir=_temp_xz_dir*_xz_lenght;
           _temp_vec3.x = _temp_xz_dir.x;
           _temp_vec3.z = _temp_xz_dir.y;
           _temp_vec3.y = Mathf.Sin(v_rad);
           //根据垂直角确定y值
           _temp_vec3 = _temp_vec3.normalized;
           return _temp_vec3;
        }
        public CombineDeg convert_to_deg()
        {
            CombineDeg temp_deg;
            temp_deg.h_deg = (int)(RadianMath._RAD2DEG *h_rad);
            temp_deg.v_deg = (int)(RadianMath._RAD2DEG *v_rad);
            return temp_deg;
        }
        public override string ToString()
        {
            return "h_rad:" + h_rad.ToString() + " v_rad:" + v_rad.ToString();
        }
    }