using System;
using BattleSystem.Core;
//using BehaviorDesigner.Runtime.Tasks.Unity.UnityInput;
using DigitalRubyShared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Skill
{
    public class SkillEditController : MonoBehaviour
    {
        private Creature m_SelectedRole = null;

        public Creature SelRole
        {
            get { return m_SelectedRole; }
            set { m_SelectedRole = value; }
        }
        private int index = 0;
        public Vector3 RandomPos
        {
            get 
            {
                float angle = index * 36f * 3.14f / 180f;
                var r = 1 + index * 0.1f;
                var v3 = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;
                index++;
                return v3;
            }
        }

        private void Start()
        {
            GameObject.Find("FingersScriptPrefab").GetComponent<FingersScript>().enabled = false;
            BattleStateManager.Instance.ChangeState(eBattleState.Play);
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Role")))
                {
                    //Debug.Log(hit.collider.gameObject.name);

                    Creature tmp_creature = SceneObjectManager.Instance.Find(hit.collider.gameObject);
                    if (tmp_creature != null)
                    {
                        if (m_SelectedRole != null)
                        {
                            m_SelectedRole.OnUnSelected();
                            m_SelectedRole = null;
                            return;
                        }
                            
                        m_SelectedRole = tmp_creature as Creature;
                        SceneObjectManager.Instance.LocalPlayerCamera.SetFollowTarget(m_SelectedRole);
                        m_SelectedRole.OnSelected();
                        return;
                    }
                }
            }
            
        }

        /*private void OnGUI()
        {
            GUI.skin.button.fontSize = 30;

            if (GUI.Button(new Rect(40, 40, 200, 100), "创建怪物"))
            {
                LocalPlayer localPlayer = SceneObjectManager.Instance.GetLocalPlayer();
                if(localPlayer == null)
                {
                    Debug.LogError("先创建主角");
                    return;
                }
                
                var data = ClientEngine.Instance.GetInputObject();
                data.cmdType = CmdType.CreateHero;
                data._idList.Add("50001");
                data.pos = Vector3.one;
                data.param = "MAX_HP";
                ClientEngine.Instance.AddInputData(data);
                //ulong id = (ulong)SceneObjectManager.Instance.GetCreatureCount();

                //var creature = SceneObjectManager.Instance.CreateMonster(id, "怪物" + id);

                //Vector3 pos = localPlayer.transform.position;
                //Vector2 vec = Random.insideUnitCircle * 2;
                //pos.x += vec.x;
                //pos.z += vec.y;
                //creature.SetPosition(pos);

                //for(int i =  11; i <20;i++)
                //{
                //    var creature = SceneObjectManager.Instance.CreateMonster("monster" + i, "怪物" + i);

                //    Vector3 pos = new Vector3((i - 10) * 10, 0, 0);
                //    Vector2 vec = Random.insideUnitCircle * 2;
                //    pos.x += vec.x;
                //    pos.z += vec.y;
                //    creature.SetPosition(pos);
                //}

            }
            


          

        }
        */
        
        

    }
}