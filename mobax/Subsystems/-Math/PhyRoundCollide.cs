//ybzuo-ro
//球体碰撞检测
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Detectors;

namespace MathDL
{
    public class PhySphereCollide
    {
        public void set_info(Vector3 _pos, float _radius)
        {
            m_center = _pos;
            m_radius = _radius;
        }

        public List<SphereCollResult> fetch_result()
        {
            m_result_coll.Clear();
            foreach (KeyValuePair<int, PhyObject> _kv in PhyWorld.get_single().get_object_bank().get_object_coll())
            {
                box_in_round_test(_kv.Key, _kv.Value.get_box());
            }

            return m_result_coll;
        }

        void execute_pos(Vector3 _pos)
        {
            float _lenght = Vector3.Distance(m_center, _pos);
            if (_lenght <= m_radius)
            {
                if (_lenght < m_distance_record)
                {
                    m_distance_record = _lenght;
                }
            }
        }

        void box_in_round_test(int _id, PhyABox _box)
        {
            //先检测碰撞点是否在盒内
            if (_box.test_point_in(m_center))
            {
                SphereCollResult temp_rcr;
                temp_rcr.distance = 0.0f;
                temp_rcr.id = _id;
                m_result_coll.Add(temp_rcr);
            }
            else
            {
                m_distance_record = 9999.0f;
                for (int i = 0; i < 8; ++i)
                {
                    execute_pos(_box.get_position(i));
                }

                execute_pos(_box.get_center());
                if (m_distance_record != 9999.0f)
                {
                    SphereCollResult temp_rcr;
                    temp_rcr.distance = m_distance_record;
                    temp_rcr.id = _id;
                    m_result_coll.Add(temp_rcr);
                }
            }
        }

        Vector3 m_center;
        ObscuredFloat m_radius;
        List<SphereCollResult> m_result_coll = new List<SphereCollResult>();
        ObscuredFloat m_distance_record;
    }
}