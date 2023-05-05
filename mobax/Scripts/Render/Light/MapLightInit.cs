using System;
using UnityEngine;
public class MapLightInit : MonoBehaviour
{
        private void Start()
        {
        throw new Exception("猜测不使用的代码");
                //return;
                //SceneEventManager.CoreStuff.RegisterListener("godLight", GodLightDel);
        }
        void GodLightDel(int arg)
        {
                return;
                if (arg == 1)
                {
                        if (godLightGo != null)
                        {
                                godLightGo.transform.parent = SceneObjectManager.Instance.LocalPlayerCamera.transform;
                                godLightGo.transform.localPosition=Vector3.zero;
                                godLightGo.transform.localRotation=Quaternion.identity;
                                godLightGo.SetActive(true);
                        }
                }
                else
                {
                        if (godLightGo != null)
                        {
                                godLightGo.transform.parent = transform;
                                godLightGo.SetActive(false);
                        }
                }
        }

        public GameObject godLightGo=null;
}
