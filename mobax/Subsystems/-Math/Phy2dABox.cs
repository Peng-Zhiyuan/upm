//ybzuo-ro
 /*                       
 *              0--------------1
 *      
 * 
 *                      
 *             3--------------2
*/
using System.Collections.Generic;
using UnityEngine;
    public class Phy2dABox
    {
        public Phy2dABox()
        {

        }
        public Phy2dABox clone()
        {
            Phy2dABox temp_2dbox = new Phy2dABox();
            temp_2dbox.m_position = m_position;
            temp_2dbox.m_size = m_size;
            temp_2dbox.m_half_size = m_half_size;
            for (int i = 0; i < BOX_POINT_NUM;++i)
            {
                temp_2dbox.m_point_coll[i] = m_point_coll[i];
            }
            return temp_2dbox;
        }
        public void set_postion(Vector2 _pos)
        {
            m_position = _pos;
            refresh();
        }
        public void set_size(Vector2 _size)
        {
            m_size = _size;
            m_half_size = m_size * 0.5f;
            refresh();
        }
        public void reset_box(Vector2 _pos, Vector2 _size)
        {
            m_position = _pos;
            m_size = _size;
            m_half_size = m_size * 0.5f;
            refresh();
        }
        void refresh()
        {
            m_point_coll[LT] = m_position + new Vector2(-m_half_size.x, m_half_size.y);
            m_point_coll[RT] = m_position + new Vector2(m_half_size.x, m_half_size.y);
            m_point_coll[RB] = m_position + new Vector2(m_half_size.x, -m_half_size.y);
            m_point_coll[LB] = m_position + new Vector2(-m_half_size.x, -m_half_size.y);
        }
        public bool collision_box(Phy2dABox _tar)
        {
            if (m_point_coll[RT].x < _tar.m_point_coll[LT].x ||
                    _tar.m_point_coll[RT].x < m_point_coll[LT].x)
            {
                return false;
            }
            if (m_point_coll[RB].y > _tar.m_point_coll[RT].y ||
                    _tar.m_point_coll[RB].y > m_point_coll[RT].y)
            {
                return false;
            }
             return true;
        }
        public bool collision_tri(Vector2 _p1,Vector2 _p2,Vector2 _p3,int _deep)
        {
            //检测和三角形相交
            //三角形上有任意一点在盒子里 肯定相交
            if (test_point_in(_p1)&&test_point_in(_p2)&&test_point_in(_p3))
            {
                return true;
            }
            //盒子上有任意一点在三角形里，肯定相交
            if (test_in_tri(lt(), _p1, _p2, _p3)&&test_in_tri(rt(), _p1, _p2, _p3)&&test_in_tri(lb(), _p1, _p2, _p3)&&test_in_tri(rb(), _p1, _p2, _p3))
            {
                //UnityEngine.Debug.Log(_deep);
                //UnityEngine.Debug.Log(_p1.get_str_tip());
                //UnityEngine.Debug.Log(_p2.get_str_tip());
                //UnityEngine.Debug.Log(_p3.get_str_tip());
                return true;
            }

            Vector2 _temp_dir = _p2 - _p1;
            float _temp_lenght = _temp_dir.sqrMagnitude;
            RaySegResult2D _rsr = ray_test(_p1, _temp_dir);
            if (_rsr.success && (_rsr.distance <= _temp_lenght))
            {
                return true;
            }


            _temp_dir = _p3 - _p2;
            _temp_lenght = _temp_dir.sqrMagnitude;
            _rsr = ray_test(_p2, _temp_dir);
            if (_rsr.success && (_rsr.distance <= _temp_lenght))
            {
                return true;
            }


            _temp_dir = _p1 - _p3;
            _temp_lenght = _temp_dir.sqrMagnitude;
            _rsr = ray_test(_p3, _temp_dir);
            if (_rsr.success && (_rsr.distance <= _temp_lenght))
            {
                return true;
            }
            return false;
        }
        bool test_in_tri(Vector2 _p,Vector2 _t1,Vector2 _t2,Vector2 _t3)
        {
            float _face = get_tri_face(_t1, _t2, _t3);
         
            /*
            if (_face==0.0f)
            {
                return false;
            }
             */ 
         
            float _face1 = get_tri_face(_p, _t1, _t2);
            float _face2 = get_tri_face(_p, _t2, _t3);
            float _face3 = get_tri_face(_p, _t1, _t3);

            float _total_face=_face1 + _face2 + _face3;
            float _sub=_total_face - _face;
            //UnityEngine.Debug.Log("face:" + _face + " total_face:" + _total_face + " sub:" + _sub);
            if (_sub<= face_equals)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        float get_tri_face(Vector2 _t1,Vector2 _t2,Vector2 _t3)
        {
             //求出三条边长
            float _l1 =Vector2.Distance(_t1,_t2);
            float _l2 = Vector2.Distance(_t2,_t3);
            float _l3 =Vector2.Distance(_t3,_t1);
            float _p = (_l1 + _l2 + _l3) / 2.0f;
            float _r = UnityEngine.Mathf.Sqrt(_p * (_p - _l1) * (_p - _l2) * (_p - _l3));
            return _r;
        }

        public Vector2 get_position(int _index) { return m_point_coll[_index]; }
        public Vector2 get_center()
        {
            return new Vector2(get_x_min() + m_half_size.x, get_y_min() + m_half_size.y);
        }
        public bool test_point_in(Vector2 _pos)
        {
            if ((_pos.x >= lt().x) && (_pos.x <= rt().x) && (_pos.y >= lb().y) && (_pos.y <= lt().y))
            {
                return true;
            }
            else
                return false;
        }
        public RaySegResult2D ray_test(Vector2 ro, Vector2 rd)
        {
            RaySegResult2D final_result;
            final_result.success = false;
            final_result.distance = 999999.0f;
            final_result.position = new Vector2(0.0f, 0.0f);
               //上
             RaySegResult2D temp_result = GeometryMath.gm_raycast_segment(ro, rd, lt(), rt() - lt());
             if (temp_result.success)
            {
                if (temp_result.distance <final_result.distance)
                {
                    final_result = temp_result;
                }
            }
            //右
            temp_result= GeometryMath.gm_raycast_segment(ro, rd, rt(), rb() - rt());
            if (temp_result.success)
            {
                if (temp_result.distance < final_result.distance)
                {
                    final_result = temp_result;
                }
            }
            //下
            temp_result = GeometryMath.gm_raycast_segment(ro, rd, rb(), lb() - rb());
            if (temp_result.success)
            {
                if (temp_result.distance < final_result.distance)
                {
                    final_result = temp_result;
                }
            }
            //左
            temp_result = GeometryMath.gm_raycast_segment(ro, rd, lb(), lt() - lb());
            if (temp_result.success)
            {
                if (temp_result.distance < final_result.distance)
                {
                    final_result = temp_result;
                }
            }
            return final_result;
        }
        public bool round_test(Vector2 _center,float _radius)
        {
			//yuanxin zai  ju xing nei
			if(GeometryMath.PointInRect2D(m_position,m_size,_center))
			{
				return true;
			}

			//juxing de yige dian  zai yuan nei
			for(int i=0;i<m_point_coll.Length;++i)
			{
				if(Vector2.Distance(m_point_coll[i],_center)<_radius)
				{
					return true;
				}
			}

			//top
			RoundSegResult2D _reuslt=GeometryMath.gm_round_segment(_center,_radius,lt(),rt());
			if(_reuslt.result!=RoundSegResultType2D.NULL)
			{
				return true;
			}

			//right
			_reuslt=GeometryMath.gm_round_segment(_center,_radius,rt(),rb());
			if(_reuslt.result!=RoundSegResultType2D.NULL)
			{
				return true;
			}

			//bottom
			_reuslt=GeometryMath.gm_round_segment(_center,_radius,rb(),lb());
			if(_reuslt.result!=RoundSegResultType2D.NULL)
			{
				return true;
			}

			//left
			_reuslt=GeometryMath.gm_round_segment(_center,_radius,lb(),lt());
			if(_reuslt.result!=RoundSegResultType2D.NULL)
			{
				return true;
			}
            return false;
        }
        public override string ToString()
        {
            return "(" + "xmin:" + get_x_min().ToString() + ",xmax:" + get_x_max().ToString() + ",ymin:" + get_y_min().ToString()
                + ",ymax:" + get_y_max().ToString();
        }
        public float get_x_min() { return lt().x; }
        public float get_x_max() { return rt().x; }
        public float get_y_min() { return lb().y; }
        public float get_y_max() { return lt().y; }
        const int LT = 0;
        const int RT = 1;
        const int RB = 2;
        const int LB = 3;
        const int BOX_POINT_NUM = 4;
        const float face_equals =1.1f;
        Vector2 lt() { return m_point_coll[LT]; }
        Vector2 rt() { return m_point_coll[RT]; }
        Vector2 lb() { return m_point_coll[LB]; }
        Vector2 rb() { return m_point_coll[RB]; }
        Vector2[] m_point_coll = new Vector2[BOX_POINT_NUM];
        Vector2 m_position = new Vector2(0.0f, 0.0f);
        Vector2 m_size = new Vector2(1.0f, 1.0f);
        Vector2 m_half_size = new Vector2(0.5f, 0.5f);
    }
