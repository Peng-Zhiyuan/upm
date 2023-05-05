//ybzuo-ro
using System.Collections.Generic;
using UnityEngine;
    /*                        4--------------5
     *              0--------------1
     *      
     * 
     *                        7--------------6
     *             3--------------2
    */
    //基于恒轴对齐3d包围盒
    public class PhyABox
    {
        public PhyABox(PhyABox _a,PhyABox _b)
        {
            float _temp_x_min;
            float _temp_x_max;
            float _temp_y_min;
            float _temp_y_max;
            float _temp_z_min;
            float _temp_z_max;
            if (_a.get_x_min()<=_b.get_x_min())
            {
                _temp_x_min = _a.get_x_min();
            }
            else
            {
                _temp_x_min = _b.get_x_min();
            }
            if (_a.get_x_max() >= _b.get_x_max())
            {
                _temp_x_max = _a.get_x_max();
            }
            else
            {
                _temp_x_max = _b.get_x_max();
            }


            if (_a.get_y_min() <= _b.get_y_min())
            {
                _temp_y_min = _a.get_y_min();
            }
            else
            {
                _temp_y_min = _b.get_y_min();
            }
            if (_a.get_y_max() >= _b.get_y_max())
            {
                _temp_y_max = _a.get_y_max();
            }
            else
            {
                _temp_y_max = _b.get_y_max();
            }



            if (_a.get_z_min() <= _b.get_z_min())
            {
                _temp_z_min = _a.get_z_min();
            }
            else
            {
                _temp_z_min = _b.get_z_min();
            }
            if (_a.get_z_max() >= _b.get_z_max())
            {
                _temp_z_max = _a.get_z_max();
            }
            else
            {
                _temp_z_max = _b.get_z_max();
            }

            set_postion(_a.get_position());
            set_relatively_position((_a.get_relatively_position() + _b.get_relatively_position()) * 0.5f);
            set_size(new Vector3(_temp_x_max - _temp_x_min, _temp_y_max - _temp_y_min, _temp_z_max - _temp_z_min));
        }




        public PhyABox()
        {

        }
        public Phy2dABox get_2d_box()
        {
            return m_2d_box;
        }
        public PhyABox clone()
        {
            PhyABox temp_abox = new PhyABox();
            for (int i = 0; i < BOX_POINT_NUM;++i)
            {
                temp_abox.m_point_coll[i] = m_point_coll[i];
            }
            temp_abox.m_position = m_position;
            temp_abox.m_relatively_position = m_relatively_position;
            temp_abox.m_size = m_size;
            temp_abox.m_half_size = m_half_size;
            temp_abox.m_2d_box = m_2d_box.clone();
            return temp_abox;
        }
        public void set_postion(Vector3 _pos)
        {
            m_position = _pos;
            m_2d_box.set_postion(new Vector2(m_position.x, m_position.z));
            refresh();
        }
        public Vector3 get_position()
        {
            return m_position;
        }
        public void set_relatively_position(Vector3 _pos)
        {
            m_relatively_position = _pos;
            refresh();
        }
        public void set_size(Vector3 _size)
        {
            m_size = _size;
            m_half_size = m_size * 0.5f;
            m_2d_box.set_size(new Vector2(m_size.x, m_size.z));
            m_volume = m_size.x * m_size.y * m_size.z;
            refresh();
        }
        public Vector3 get_size()
        {
            return m_size;
        }
        public float get_radius()
        {
            float _temp_max = -1.0f;
            if (m_size.x>_temp_max)
            {
                _temp_max = m_size.x;
            }
            if (m_size.z>_temp_max)
            {
                _temp_max = m_size.z;
            }
            return _temp_max/2.0f;
        }
        public List<PhyABox> split()
        {
            List<PhyABox> temp_box_list = new List<PhyABox>();
    
            PhyABox temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(m_size.x * 0.25f, m_size.y * 0.25f, m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);

            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(m_size.x * 0.25f, m_size.y * 0.25f, -m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);

            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(-m_size.x * 0.25f, m_size.y * 0.25f, m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);


            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(-m_size.x * 0.25f, m_size.y * 0.25f, -m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);


            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(m_size.x * 0.25f, -m_size.y * 0.25f, m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);

            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(m_size.x * 0.25f, -m_size.y * 0.25f, -m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);

            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(-m_size.x * 0.25f, -m_size.y * 0.25f, m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);


            temp_box = new PhyABox();
            temp_box.set_size(m_size * 0.5f);
            temp_box.set_relatively_position(m_relatively_position + new Vector3(-m_size.x * 0.25f, -m_size.y * 0.25f, -m_size.z * 0.25f));
            temp_box.set_postion(m_position);
            temp_box_list.Add(temp_box);

            //对称分割
            return temp_box_list;
        }
        public float get_volume()
        {
            return m_volume;
        }
        void refresh()
        {
            m_center = m_position + m_relatively_position;
            m_point_coll[FLT] = m_center + new Vector3(-m_half_size.x, m_half_size.y, -m_half_size.z);
            m_point_coll[FRT] = m_center + new Vector3(m_half_size.x, m_half_size.y, -m_half_size.z);
            m_point_coll[FRB] = m_center + new Vector3(m_half_size.x, -m_half_size.y, -m_half_size.z);
            m_point_coll[FLB] = m_center + new Vector3(-m_half_size.x, -m_half_size.y, -m_half_size.z);

            m_point_coll[BLT] = m_center + new Vector3(-m_half_size.x, m_half_size.y, m_half_size.z);
            m_point_coll[BRT] = m_center + new Vector3(m_half_size.x, m_half_size.y, m_half_size.z);
            m_point_coll[BRB] = m_center + new Vector3(m_half_size.x, -m_half_size.y, m_half_size.z);
            m_point_coll[BLB] = m_center + new Vector3(-m_half_size.x, -m_half_size.y, m_half_size.z);
        }
        public bool collision_box(PhyABox _tar)
        {
            if (m_point_coll[FRT].x < _tar.m_point_coll[FLT].x ||
                    _tar.m_point_coll[FRT].x < m_point_coll[FLT].x)
            {
                return false;
            }
            if (m_point_coll[FRB].y > _tar.m_point_coll[FRT].y ||
                    _tar.m_point_coll[FRB].y > m_point_coll[FRT].y)
            {
                return false;
            }
            if (m_point_coll[FRT].z > _tar.m_point_coll[BRT].z ||
             _tar.m_point_coll[FRT].z > m_point_coll[BRT].z)
            {
                return false;
            }
            return true;
        }
        public bool collision_seg(Vector3 _p1,Vector3 _p2)
        {
            if (test_point_in(_p1)||test_point_in(_p2))
            {
                return true;
            }
            else
            {
               
                Vector3 _dir = _p2 - _p1;
                float _lenght = _dir.magnitude;
                _dir=_dir.normalized;
                Ray temp_ray=new Ray(_p1,_dir);
                float _lenght_result;

                Plane temp_top = new Plane(m_point_coll[FRT], m_point_coll[BRT], m_point_coll[FLT]);
                if (temp_top.Raycast(temp_ray, out _lenght_result))
                {
                    if (_lenght_result<=_lenght)
                    {
                        Vector3 temp_pos = _p1 + _dir * _lenght_result;
                        if (temp_pos.x>=get_x_min()&&temp_pos.x<=get_x_max()&&temp_pos.z>=get_z_min()&&temp_pos.z<=get_z_max())
                        {
                            return true;
                        }
                    }
                }

                Plane temp_bottom = new Plane(m_point_coll[FRB], m_point_coll[BRB], m_point_coll[FLB]);
                if (temp_bottom.Raycast(temp_ray, out _lenght_result))
                {
                    if (_lenght_result <= _lenght)
                    {
                        Vector3 temp_pos = _p1 + _dir * _lenght_result;
                        if (temp_pos.x >= get_x_min() && temp_pos.x <= get_x_max() && temp_pos.z >= get_z_min() && temp_pos.z <= get_z_max())
                        {
                            return true;
                        }
                    }
                }

                Plane temp_left = new Plane(m_point_coll[FLT], m_point_coll[BLT], m_point_coll[FLB]);
                if (temp_left.Raycast(temp_ray, out  _lenght_result))
                {
                    if (_lenght_result <= _lenght)
                    {
                        Vector3 temp_pos = _p1 + _dir * _lenght_result;
                        if (temp_pos.z >= get_z_min() && temp_pos.z<= get_z_max() && temp_pos.y >= get_y_min() && temp_pos.z <= get_y_max())
                        {
                            return true;
                        }
                    }
                }

                Plane temp_right = new Plane(m_point_coll[FRT], m_point_coll[BRT], m_point_coll[FRB]);
                if (temp_right.Raycast(temp_ray, out  _lenght_result))
                {
                    if (_lenght_result <= _lenght)
                    {
                        Vector3 temp_pos = _p1 + _dir * _lenght_result;
                        if (temp_pos.z >= get_z_min() && temp_pos.z<= get_z_max() && temp_pos.y >= get_y_min() && temp_pos.y <= get_y_max())
                        {
                            return true;
                        }
                    }
                }

                Plane temp_front = new Plane(m_point_coll[FRT], m_point_coll[FRB], m_point_coll[FLT]);
                if (temp_front.Raycast(temp_ray, out  _lenght_result))
                {
                    if (_lenght_result <= _lenght)
                    {
                        Vector3 temp_pos = _p1 + _dir * _lenght_result;
                        if (temp_pos.y >= get_y_min() && temp_pos.y <= get_y_max() && temp_pos.x >= get_x_min() && temp_pos.x <= get_x_max())
                        {
                            return true;
                        }
                    }
                }

                Plane temp_back = new Plane(m_point_coll[BRT], m_point_coll[BRB], m_point_coll[BLT]);
                if (temp_back.Raycast(temp_ray, out _lenght_result))
                {
                    if (_lenght_result <= _lenght)
                    {
                        Vector3 temp_pos = _p1 + _dir * _lenght_result;
                        if (temp_pos.y >= get_y_min() && temp_pos.y <= get_y_max() && temp_pos.x >= get_x_min() && temp_pos.x <= get_x_max())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool collision_tri_plane(Vector3 _p1,Vector3 _p2,Vector3 _p3,int _deep)
        {
            //UnityEngine.Debug.Log("deep " + _deep);

            //先判断box是否和三角面的三条边相交，如果相交则肯定相交。
            /*
            if (collision_seg(_p1, _p2))
            {
                return true;
            }
            if (collision_seg(_p2, _p3))
            {
                return true;
            }
            if (collision_seg(_p3, _p1))
            {
                return true;
            }
             */ 
            //将三角面和包围盒分别在坐标系的三个平面上做投影，如果三个平面的投影的形状都相交，则相交，否则不想交
            //因为是标准平面，所以投影可以直接去掉对应一维即可。
            Phy2dABox temp_xy_box = new Phy2dABox();
            temp_xy_box.set_postion(new Vector2(m_center.x, m_center.y));
            temp_xy_box.set_size(new Vector2(m_size.x, m_size.y));

            Phy2dABox temp_zy_box = new Phy2dABox();
            temp_zy_box.set_postion(new Vector2(m_center.z, m_center.y));
            temp_zy_box.set_size(new Vector2(m_size.z, m_size.y));


            Phy2dABox temp_xz_box = new Phy2dABox();
            temp_xz_box.set_postion(new Vector2(m_center.x,m_center.z));
            temp_xz_box.set_size(new Vector2(m_size.x, m_size.z));


            Vector2 xy_p1 = new Vector2(_p1.x, _p1.y);
            Vector2 xy_p2 = new Vector2(_p2.x, _p2.y);
            Vector2 xy_p3 = new Vector2(_p3.x, _p3.y);

            Vector2 zy_p1 = new Vector2(_p1.z, _p1.y);
            Vector2 zy_p2 = new Vector2(_p2.z, _p2.y);
            Vector2 zy_p3 = new Vector2(_p3.z, _p3.y);

            Vector2 xz_p1 = new Vector2(_p1.x, _p1.z);
            Vector2 xz_p2 = new Vector2(_p2.x, _p2.z);
            Vector2 xz_p3 = new Vector2(_p3.x, _p3.z);

            //if (temp_xy_box.collision_tri(xy_p1, xy_p2, xy_p3, _deep))
            {
               //return true;
            }
           // if (temp_zy_box.collision_tri(zy_p1, zy_p2, zy_p3, _deep))
            {
                //return true;
            }
            //if (temp_xz_box.collision_tri(xz_p1, xz_p2, xz_p3, _deep))
            //{
              // return true;
            //}
            /*
        if (temp_zx_box.collision_tri(zx_p1, zx_p2, zx_p3, _deep) && temp_xy_box.collision_tri(xy_p1, xy_p2, xy_p3, _deep))
        {
            //return true;
        }

        if (temp_zx_box.collision_tri(zx_p1, zx_p2, zx_p3, _deep) || temp_yz_box.collision_tri(yz_p1, yz_p2, yz_p3, _deep))
        {
            //return true;
        }

        if (temp_xy_box.collision_tri(xy_p1, xy_p2, xy_p3, _deep) && temp_yz_box.collision_tri(yz_p1, yz_p2, yz_p3, _deep))
        {
            //return true;
        }
       // */
        if (temp_xz_box.collision_tri(xz_p1, xz_p2, xz_p3, _deep) && temp_xy_box.collision_tri(xy_p1, xy_p2, xy_p3, _deep) && temp_zy_box.collision_tri(zy_p1, zy_p2, zy_p3, _deep))
        {
            return true;
        }
            return false;
        }
        public Vector3 get_position(int _index) { return m_point_coll[_index]; }
        public Vector3 get_center()
        {
            return m_center;
        }
        public Vector3 get_relatively_position()
        {
            return m_relatively_position;
        }
        public bool test_point_in(Vector3 _pos)
        {
            if ((_pos.x >= flt().x) && (_pos.x <= frt().x) && (_pos.y >= flb().y) && (_pos.y <= flt().y)&&(_pos.z>=flt().z)&&(_pos.z<=blt().z))
            {
                return true;
            }
            else
                return false;
        }
        public override string ToString()
        {
            return "("+"xmin:"+get_x_min().ToString()+",xmax:"+get_x_max().ToString()+",ymin:"+get_y_min().ToString()
                +",ymax:"+get_y_max().ToString()+",zmin:"+get_z_min().ToString()+",zmax:"+get_z_max().ToString();
        }
		
		
        public float get_x_min() { return flt().x; }
        public float get_x_max() { return frt().x; }
        public float get_y_min() { return flb().y; }
        public float get_y_max() { return flt().y; }
        public float get_z_min() { return flt().z; }
        public float get_z_max() { return blt().z; }
        const int FLT = 0;
        const int FRT = 1;
        const int FRB = 2;
        const int FLB = 3;
        const int BLT = 4;
        const int BRT = 5;
        const int BRB = 6;
        const int BLB = 7;
        const int BOX_POINT_NUM = 8;
        Vector3 flt() { return m_point_coll[FLT]; }
        Vector3 frt() { return m_point_coll[FRT]; }
        Vector3 flb() { return m_point_coll[FLB]; }
        Vector3 frb() { return m_point_coll[FRB]; }
        Vector3 blt() { return m_point_coll[BLT]; }
        Vector3 brt() { return m_point_coll[BRT]; }
        Vector3 blb() { return m_point_coll[BLB]; }
        Vector3 brb() { return m_point_coll[BRB]; }
        Vector3[] m_point_coll = new Vector3[BOX_POINT_NUM];
        Vector3 m_position = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 m_relatively_position = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 m_center = new Vector3(0.0f, 0.0f, 0.0f);
        float m_volume = 0.0f;
        Vector3 m_size = new Vector3(1.0f, 1.0f, 1.0f);
        Vector3 m_half_size = new Vector3(0.5f, 0.5f, 0.5f);
        Phy2dABox m_2d_box = new Phy2dABox();
        const float m_seg_check_indev = 0.1f;
    }