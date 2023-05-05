//ybzuo-ro
//物理世界
using System.Collections.Generic;
using UnityEngine;
namespace MathDL
{
    public struct PhyObjectAddStruct
    {
		public PhyObjectType phy_type;
        public int id;
        public Vector3 postion;
        public PhyABox box;
    }
    public class PhyWorld
    {
        static PhyWorld m_single = null;
        public static PhyWorld get_single()
        {
            if (m_single == null)
            {
                m_single = new PhyWorld();
            }
            return m_single;
        }
        PhyWorld()
        {
            m_dyn_collider = new PhyDynCollide();
            m_round_collider = new PhySphereCollide();
			
			
//			//right lanban
//			PhyObjectAddStruct _poas=new PhyObjectAddStruct();
//			_poas.phy_type=PhyObjectType.BOARD;
//			_poas.id=1;
//			_poas.postion=NField.right_board_pos;			
//			_poas.box=new PhyABox();
//			_poas.box.set_postion(NField.right_board_pos);
//			_poas.box.set_size(NField.board_size);
//			add_object(_poas);
//			
//			_poas=new PhyObjectAddStruct();
//			_poas.phy_type=PhyObjectType.BAS;
//			_poas.id=2;
//			_poas.postion=NField.right_basketry_pos;			
//			_poas.box=new PhyABox();
//			_poas.box.set_postion(NField.right_basketry_pos);
//			_poas.box.set_size(NField.basketry_size);
//			add_object(_poas);
//			
//			
//			
//			//left lanban
//			_poas=new PhyObjectAddStruct();
//			_poas.phy_type=PhyObjectType.BOARD;
//			_poas.id=3;
//			_poas.postion=NField.left_board_pos;
//			_poas.box=new PhyABox();
//			_poas.box.set_postion(NField.left_board_pos);
//			_poas.box.set_size(NField.board_size);
//			add_object(_poas);
//			
//			_poas=new PhyObjectAddStruct();
//			_poas.phy_type=PhyObjectType.BAS;
//			_poas.id=4;
//			_poas.postion=NField.left_basketry_pos;			
//			_poas.box=new PhyABox();
//			_poas.box.set_postion(NField.left_basketry_pos);
//			_poas.box.set_size(NField.basketry_size);
//			add_object(_poas);
//			
			//diban
			PhyObjectAddStruct _poas=new PhyObjectAddStruct();
			_poas.phy_type=PhyObjectType.LAND;
			_poas.id=0;
			_poas.postion=new Vector3(0.0f,-0.5f,0.0f);
			_poas.box=new PhyABox();
			_poas.box.set_postion(_poas.postion);
			_poas.box.set_size(new Vector3(100000.0f,1.0f,100000.0f));
			add_object(_poas);



			
			
				
        }
        public PhyObjectBank get_object_bank()
        {
            return m_object_bank;
        }
        void add_object(PhyObjectAddStruct _poas)
        {
            PhyObject _object = new PhyObject(_poas.phy_type);
            _object.set_id(_poas.id);
			_object.set_abox(_poas.box.clone());
            _object.set_position(_poas.postion);
            m_object_bank.push_object(_object);
        }
        public List<DynCollResult> dyn_detect(DynCollInitStruct _dcis,bool _only_land)
        {
            m_dyn_collider.set_dcis(_dcis);
            return  m_dyn_collider.fetch_result(_only_land);
        }
        public List<SphereCollResult> round_detect(Vector3 _pos, float _radius)
        {
            m_round_collider.set_info(_pos, _radius);
            return m_round_collider.fetch_result();
        }
        public void release()
        {
            m_object_bank.release_res();
        }
		public DynCollInitStruct fetch_a_shot(DynCollInitStruct _dcis)
		{
			//List<DynCollInitStruct> _list=new List<DynCollInitStruct>();
			List<DynCollResult> _rlist=dyn_detect(_dcis,false);
			_rlist.Sort(DynCollResult._copmass);
			_dcis.finish_time=_rlist[0].time;
			_dcis.object_type=_rlist[0].object_type;
			_dcis.hit_pos=_rlist[0].position;
			return _dcis;
//			_list.Add(_dcis);
//			if(_hit)
//			{
//				//drop_down;
//				if(Random.value>0.5f)
//				{
//					_dcis=PhyMath.make_dcis(Vector3.down);
//				}
//				else
//				{
//					//_dcis=PhyMath.make_dcis(PhyMath.get_v2_from_v1_a_dt(_dcis.init_speed,_rlist[0].time));
//					_dcis=PhyMath.make_dcis(_rlist[0].speed);
//				}
//				_dcis.init_pos=_rlist[0].position;
//				_rlist=dyn_detect(_dcis,_hit);
//				_dcis.finish_time=_rlist[0].time;
//				_dcis.object_type=_rlist[0].object_type;
//				_list.Add(_dcis);
//				while(_rlist[0].speed.magnitude>0.1f)
//				{
//					_dcis=PhyMath.make_dcis(_rlist[0].reflex*_rlist[0].speed.magnitude*0.6f);
//					_dcis.init_pos=_rlist[0].position;
//					_rlist=dyn_detect(_dcis,_hit);
//					_dcis.finish_time=_rlist[0].time;
//					_dcis.object_type=_rlist[0].object_type;
//					_list.Add(_dcis);
//				}				
//			}
//			return _list;
		}
		public List<DynCollInitStruct> free_fall(Vector3 _pos)
		{
			List<DynCollInitStruct> _list=new List<DynCollInitStruct>();
			DynCollInitStruct _dcis=PhyMath.MakeDCIS(Vector3.zero);
			_dcis.init_pos=_pos;
            List<DynCollResult> _rlist = dyn_detect(_dcis, true);
			_dcis.finish_time=_rlist[0].time;
			_dcis.object_type=_rlist[0].object_type;
			_list.Add(_dcis);
            //UnityEngine.Debug.Log(_rlist[0].time+" "+_rlist[0].hit+" "+_rlist[0].object_type);
			while(_rlist[0].speed.magnitude>0.1f)
			{
				_dcis=PhyMath.MakeDCIS(_rlist[0].reflex*_rlist[0].speed.magnitude*0.6f);
				_dcis.init_pos=_rlist[0].position;
				_rlist=dyn_detect(_dcis,true);
				_dcis.finish_time=_rlist[0].time;
				_dcis.object_type=_rlist[0].object_type;
				_list.Add(_dcis);  
			}	
			return _list;
		}
		public List<DynCollInitStruct> normal_fall(DynCollInitStruct _dcis)
		{
			List<DynCollInitStruct> _list=new List<DynCollInitStruct>();
			List<DynCollResult> _rlist=dyn_detect(_dcis,true);
			_dcis.finish_time=_rlist[0].time;
			_dcis.object_type=_rlist[0].object_type;
			_list.Add(_dcis);
			while(_rlist[0].speed.magnitude>0.1f)
			{
				_dcis=PhyMath.MakeDCIS(_rlist[0].reflex*_rlist[0].speed.magnitude*0.6f);
				_dcis.init_pos=_rlist[0].position;
				_rlist=dyn_detect(_dcis,true);
				_dcis.finish_time=_rlist[0].time;
				_dcis.object_type=_rlist[0].object_type;
				_list.Add(_dcis);
			}	
			return _list;
		}
        PhyObjectBank m_object_bank = new PhyObjectBank();
        PhyDynCollide m_dyn_collider = null;
        PhySphereCollide m_round_collider = null;
    }
}