using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.View;
using UnityEngine;

public enum GuildPlayerType
{
        Player,
        LocalPlayer,
}
public class GuildPlayer
{
        public string Uid;
        public int ConfigID;
        

        private static Transform _root;
        public static Transform Root
        {
                get
                {
                        return GuildLobbyManager.Instance.Root;
                }
        }

        public bool IsLocalPlayer()
        {
                return GetType() == GuildPlayerType.LocalPlayer;
        }

        public virtual GuildPlayerType GetType()
        {
                return GuildPlayerType.Player;
        }

        public GuildPlayer(string uid, int ParamConfigID, bool isHigh = false, bool isSpring = false, bool isSelf = false)
        {
                Uid = uid;
                ConfigID = ParamConfigID;
                
                var go = new GameObject(ConfigID.ToString());
                go.transform.SetParent(Root);
                go.transform.localPosition = new Vector3(0, 3, 0);
                go.transform.localScale = Vector3.one;
                gameObject = go;
                transform = gameObject.transform;
                
                LoadBone(ConfigID, isHigh, isSpring, isSelf);
        }

        private Animator Animator;

        public GameObject gameObject;

        public Transform transform;

        public async Task LoadBone(int ConfigID, bool isHigh, bool isSpring, bool isSelf)
        {
                HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(ConfigID);
                if (heroInfo == null)
                {
                        Debug.LogError("没有找到该英雄");
                        return;
                }
                
                var address = $"{ConfigID}_int_low.prefab";
                if (isSelf)
                {
                        var modelPath = RoleHelper.GetAvatarName(heroInfo);
                        address = $"{modelPath}_int_low.prefab";
                        bool isYZ = heroInfo.AvatarInfo.Item1 > 0;
                        if (isSpring && !isYZ)
                        {
                                address = $"{modelPath}YP.prefab";    
                        }  
                }
                else
                {
                        if (isSpring)
                        {
                                address = $"{ConfigID}YP.prefab";    
                        }   
                }
                
                //Debug.LogError("address:"+ address);
                var model = await BattleResManager.Instance.LoadAvatarModel(address);


                model.transform.SetParent(transform);
                model.transform.localPosition = Vector3.zero;
                model.transform.localScale = Vector3.one;
                model.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

                Animator = model.GetComponentInChildren<Animator>();

                if (isSelf)
                {
                        var roleRender = model.gameObject.GetOrAddComponent<RoleRender>();
                        await roleRender.SwitchHeroSkin(heroInfo.AvatarInfo);   
                }
                
                SetState(State.STATE_IDLE);
                var attach = model.GetComponentInChildren<AttachBone>();
                if (attach != null)
                {
                     attach.AttachNode();
                }
                HideWeapon(transform.gameObject);
        }

        public void LookCamera()
        {
                if(CurState != State.STATE_IDLE && CurState != State.STATE_STOP)
                        return;
                
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        public State CurState = State.STATE_IDLE;
        
        private void SetState(State state, string anim = "")
        {
                if(Animator == null)
                        return;

                CurState = state;
                
                switch (state)
                {
                        case State.STATE_IDLE:
                        {
                                Animator.Play("stand");
                                break;
                        }
                        case State.STATE_MOVE:
                        {
                                Animator.Play("run");
                                //Animator.SetBool("moving", true);
                                break;
                        }
                        case State.STATE_STOP:
                        {
                                Animator.Play("stand");
                                //Animator.SetBool("moving", false);
                                break;
                        }

                        case State.STATE_SPRING:
                        {
                                Animator.Play(anim);
                                break;
                        }
                }
        }
        
        public Dictionary<string, Transform> m_Bones = new Dictionary<string, Transform>();
        
        private void HideWeapon(GameObject rootBone)
        {
                Transform tmp_boneRoot = rootBone.transform;
                m_Bones.Clear();
                var tmp_bones = ListPool.ListPoolN<Transform>.Get();
                tmp_boneRoot.GetComponentsInChildren(tmp_bones);
                m_Bones.Add("root", tmp_boneRoot);
                foreach (var tmp_bone in tmp_bones)
                {
                        var boneName = tmp_bone.name.ToLower();
                        if (boneName.StartsWith("weapon")
                            || boneName.Contains("weapon"))
                        {
                               tmp_bone.SetActive(false);
                                m_Bones.Add(boneName, tmp_bone);
                        }
                }
                ListPool.ListPoolN<Transform>.Release(tmp_bones);
        }


        private void PlayAnim(string anim)
        {
                if(Animator == null)
                        return;
                
                Animator.Play(anim, 0, 0);
        }
        
        public void MoveTo(float dir)
        {
                _movePath.Clear();
                
                var pos  = transform.position;
                pos.x -= 20f * Mathf.Sign(dir);
                _movePath.AddLast(pos);
                
                SetState(State.STATE_MOVE);
        }

        public void MoveTo(Vector3 pos)
        {
                _movePath.Clear();
                _movePath.AddLast(pos);
                SetState(State.STATE_MOVE);
        }

        public async Task EnterSpring(string anim)
        {
                
                while (true)
                {
                        if (Animator != null)
                        {
                                SetState(State.STATE_SPRING, anim);  
                                break;
                        }
                                
                        await Task.Delay(50);
                }
                
        }
        
        public async Task SayHello(string anim, string nextanim)
        {
                SetState(State.STATE_SPRING, anim);  
                
                float t = Animator.GetClipLength(anim);
                await Task.Delay((int) (1000f * t));
                
                SetState(State.STATE_SPRING, nextanim);
        }


        public virtual void Update()
        {
              UpdatePosition(Time.deltaTime);  
        }

        public Transform GetBone(string name)
        {
                var tmp_bones = ListPool.ListPoolN<Transform>.Get();
                transform.GetComponentsInChildren(tmp_bones);
                foreach (var tmp_bone in tmp_bones)
                {
                        var boneName = tmp_bone.name.ToLower();
                        if (boneName.Equals("body_hit"))
                        {
                                return tmp_bone;
                        }
                }

                return null;
        }
        
        public int SpringIndex
        {
                get;
                set;
        }



        private float speed = 5f;
        private LinkedList<Vector3> _movePath = new LinkedList<Vector3>();
        public void UpdatePosition(float param_delTime)
        {
                if(_movePath.Count == 0)
                    return;
                
                if(Animator == null)
                        return;
                
                float tmp_remaining = param_delTime;
                
                if(_movePath.First == null)
                    return;
                
                while (_movePath.First != null && tmp_remaining > 0)
                {
                    var tmp_pos = transform.position;
                    var tmp_target = _movePath.First.Value;
                    
                    //Owner.Speed = _movePath.First.Value * (1 - (Owner as ActorEntity).LowSpeed);

                    Vector3 tmp_diff = tmp_target - tmp_pos;

                    Animator.transform.forward = tmp_diff;

                    float tmp_distance = Vector3.Distance(tmp_pos, tmp_target);

                    float tmp_needtime = tmp_distance / speed;

                    var dir = tmp_diff.normalized;
                    if (tmp_remaining > tmp_needtime || (tmp_diff.x == 0.0f && tmp_diff.z == 0.0f))
                    {
                        
                        transform.position = tmp_target;
                        
                        if(_movePath.First != null)
                            _movePath.RemoveFirst();

                        tmp_remaining -= tmp_needtime;
                        
                        
                    }
                    else
                    {
                            transform.position = Vector3.MoveTowards(transform.position, tmp_target, speed * tmp_remaining);

                        tmp_remaining = 0;
                    }
                }

                if (_movePath.Count == 0)
                {
                        SetState(State.STATE_STOP);

                        if (IsLocalPlayer())
                        {
                                GuildLobbyManager.Instance.ArriveTarget();
                        }
                }
        }


}