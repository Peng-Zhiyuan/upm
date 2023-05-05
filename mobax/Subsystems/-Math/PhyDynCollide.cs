//ybzuo-ro
using UnityEngine;
using System.Collections.Generic;

namespace MathDL
{
    public class PhyDynCollide
    {
        public PhyDynCollide()
        {

        }

        public void set_dcis(DynCollInitStruct _dcis)
        {
            m_dcis = _dcis;
        }

        public List<DynCollResult> fetch_result(bool _only_land)
        {
            m_need_test_object.Clear();
            m_result_pcr_coll.Clear();
            Dictionary<int, PhyObject> m_obj_coll = PhyWorld.get_single().get_object_bank().get_object_coll();
            foreach (KeyValuePair<int, PhyObject> kv in m_obj_coll)
            {
                if (_only_land)
                {
                    if (kv.Value.get_phy_type() != PhyObjectType.LAND)
                    {
                        continue;
                    }
                }

                box_level_test(kv.Value);
            }

            return manage_result();
        }

        void box_level_test(PhyObject _obj)
        {
            PhyABox temp_box = _obj.get_box();
            //检测上面
            float rt1, rt2;
            if (PhyMath.GetDtFromAdsV1(m_dcis.acceleration.y, temp_box.get_y_max() - m_dcis.init_pos.y,
                m_dcis.init_speed.y, out rt1, out rt2))
            {

                test_in_from_time(_obj, temp_box, rt1, Vector3.up);
                if (rt2 != rt1)
                {
                    test_in_from_time(_obj, temp_box, rt2, Vector3.up);
                }
            }

            //与下面
            if (PhyMath.GetDtFromAdsV1(m_dcis.acceleration.y, temp_box.get_y_min() - m_dcis.init_pos.y,
                m_dcis.init_speed.y, out rt1, out rt2))
            {
                test_in_from_time(_obj, temp_box, rt1, Vector3.down);
                if (rt2 != rt1)
                {
                    test_in_from_time(_obj, temp_box, rt2, Vector3.down);
                }
            }

            //与左面
            if (PhyMath.GetDtFromAdsV1(m_dcis.acceleration.x, temp_box.get_x_min() - m_dcis.init_pos.x,
                m_dcis.init_speed.x, out rt1, out rt2))
            {
                test_in_from_time(_obj, temp_box, rt1, Vector3.left);
                if (rt2 != rt1)
                {
                    test_in_from_time(_obj, temp_box, rt2, Vector3.left);
                }
            }

            //右面
            if (PhyMath.GetDtFromAdsV1(m_dcis.acceleration.x, temp_box.get_x_max() - m_dcis.init_pos.x,
                m_dcis.init_speed.x, out rt1, out rt2))
            {
                test_in_from_time(_obj, temp_box, rt1, Vector3.right);
                if (rt2 != rt1)
                {
                    test_in_from_time(_obj, temp_box, rt2, Vector3.right);
                }
            }

            //前面
            if (PhyMath.GetDtFromAdsV1(m_dcis.acceleration.z, temp_box.get_z_min() - m_dcis.init_pos.z,
                m_dcis.init_speed.z, out rt1, out rt2))
            {
                test_in_from_time(_obj, temp_box, rt1, Vector3.forward);
                if (rt2 != rt1)
                {
                    test_in_from_time(_obj, temp_box, rt2, Vector3.forward);
                }
            }

            //后面
            if (PhyMath.GetDtFromAdsV1(m_dcis.acceleration.z, temp_box.get_z_max() - m_dcis.init_pos.z,
                m_dcis.init_speed.z, out rt1, out rt2))
            {
                test_in_from_time(_obj, temp_box, rt1, Vector3.back);
                if (rt2 != rt1)
                {
                    test_in_from_time(_obj, temp_box, rt2, Vector3.back);
                }
            }
        }

        void test_in_from_time(PhyObject _obj, PhyABox _box, float _time, Vector3 _normal)
        {
            if (_time > 0)
            {
                //Vector3 _v2;
                //_v=PhyMath.get_v2_from_v1_a_dt(m_dcis.init_speed, m_dcis.acceleration.y, _time);
                Vector3 temp_pos = PhyMath.Plerp(m_dcis, _time + m_time_precision);
                if (_box.test_point_in(temp_pos))
                {
                    float temp_time = _time - m_time_precision;
                    DynCollResult temp_pcr = new DynCollResult();
                    temp_pcr.time = temp_time;
                    temp_pcr.object_type = _obj.get_phy_type();
                    temp_pcr.position = temp_pos;
                    temp_pcr.speed = PhyMath.GetV2FromV1aDt(m_dcis.init_speed, temp_time);
                    Vector3 _lpos = PhyMath.Plerp(m_dcis, temp_time - m_time_precision);
                    temp_pcr.dir = temp_pcr.position - _lpos;
                    temp_pcr.dir.Normalize();
                    temp_pcr.reflex = Vector3.Reflect(temp_pcr.dir, _normal);
                    temp_pcr.reflex.Normalize();
                    temp_pcr.hit = true;
                    m_result_pcr_coll.Add(temp_pcr);
                }
            }
        }

        bool simple_test_in_from_time(PhyObject _obj, PhyABox _box, float _time)
        {
            if (_time > 0)
            {
                Vector3 temp_pos = PhyMath.Plerp(m_dcis, _time + m_time_precision);
                if (_box.test_point_in(temp_pos))
                {
                    return true;
                }
            }

            return false;
        }

        List<DynCollResult> manage_result()
        {
            if (m_result_pcr_coll.Count == 0)
            {
                float rt = get_out_time();
                DynCollResult temp_pcr = new DynCollResult();
                temp_pcr.object_type = PhyObjectType.LAND;
                temp_pcr.hit = false;
                Vector3 repos = PhyMath.Plerp(m_dcis, rt);
                temp_pcr.position = repos;
                temp_pcr.time = rt;
                temp_pcr.speed = PhyMath.GetV2FromV1aDt(m_dcis.init_speed, rt);
                Vector3 _lpos = PhyMath.Plerp(m_dcis, rt - m_time_precision);
                temp_pcr.dir = temp_pcr.position - _lpos;
                temp_pcr.dir.Normalize();
                m_result_pcr_coll.Add(temp_pcr);
            }

            return m_result_pcr_coll;
        }

        float get_out_time()
        {
            float r1, r2;
            float rt = 9999.9f;
            //ymin
            PhyMath.GetDtFromAdsV1(m_dcis.acceleration.y, -m_dcis.init_pos.y, m_dcis.init_speed.y, out r1, out r2);
            if (r1 >= 0.0f && r1 < rt)
            {
                rt = r1;
            }

            if (r2 >= 0.0f && r2 < rt)
            {
                rt = r2;
            }

            if (rt > 0 && rt != 9999.9f)
            {
                return rt;
            }
            else
            {
                return 0.0f;
            }
        }

        DynCollInitStruct m_dcis;
        const float m_extern_lenght = 2.0f;
        Dictionary<float, List<PhyObject>> m_need_test_object = new Dictionary<float, List<PhyObject>>();
        List<DynCollResult> m_result_pcr_coll = new List<DynCollResult>();
        const float m_time_precision = 0.0001f;
        Vector2 map_x_limits;
        Vector2 map_z_limits;
    }
}