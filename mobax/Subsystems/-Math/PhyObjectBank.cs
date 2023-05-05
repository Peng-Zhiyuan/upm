//ybzuo-ro
using System.Collections.Generic;
    public class PhyObjectBank
    {
        public void push_object(PhyObject _obj)
        {
            if ((_obj != null) && (!m_obj_coll.ContainsKey(_obj.get_id())))
            {
                m_obj_coll.Add(_obj.get_id(), _obj);
            }
        }
        public void remove_object(int _id)
        {
            if (m_obj_coll.ContainsKey(_id))
            {
                m_obj_coll.Remove(_id);
            }
        }
        public PhyObject get_object(int _id)
        {
            return m_obj_coll[_id];
        }
        public Dictionary<int,PhyObject> get_object_coll()
        {
            return m_obj_coll;
        }
        public void release_res()
        {
            m_obj_coll.Clear();
        }
        Dictionary<int, PhyObject> m_obj_coll = new Dictionary<int, PhyObject>();
    }