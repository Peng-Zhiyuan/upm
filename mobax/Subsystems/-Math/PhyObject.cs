//ybzuo-ro
using System.Collections.Generic;
using UnityEngine;
	public enum PhyObjectType
	{
		LAND,
		BAS,
		BOARD,
	}
    public  class PhyObject
    {
		public PhyObject(PhyObjectType _type)
		{
			m_type=_type;
		}
		public PhyObjectType get_phy_type()
		{
			return m_type;
		}
        public void set_id(int _id)
        {
            m_id = _id;
        }
        public int get_id()
        {
            return m_id;
        }
        public void set_position(Vector3 _pos)
        {
            m_position = _pos;
			m_abox.set_postion(m_position);
        }
        public Vector3 get_position()
        {
            return m_position;
        }
        public void set_abox(PhyABox _box)
        {
           m_abox=_box;
        }
        public PhyABox get_box()
        {
            return m_abox;
        }
		PhyObjectType m_type=PhyObjectType.LAND;
        int m_id;
        PhyABox m_abox = new PhyABox();
        Vector3 m_position;
        Vector3 m_size;
    }