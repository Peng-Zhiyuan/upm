using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    [System.Serializable]
    public class LoopScrollPrefabSource 
    {
        public GameObject prefab;
        public string prefabName;
        public int poolSize = 5;

        private bool inited = false;
        public virtual GameObject GetObject()
        {
            if(!inited)
            {
                inited = true;
                if(prefab != null){
                    SG.ResourceManager.Instance.InitPoolWithGameObject(prefab,poolSize);
                    return SG.ResourceManager.Instance.GetObjectFromPool(prefab.name);
                }
                else{
                    SG.ResourceManager.Instance.InitPool(prefabName, poolSize);
                    return SG.ResourceManager.Instance.GetObjectFromPool(prefabName);
                }
            }else{
                  if(prefab != null){
                    return SG.ResourceManager.Instance.GetObjectFromPool(prefab.name);
                }
                else{
                    return SG.ResourceManager.Instance.GetObjectFromPool(prefabName);
                }
            }
        }

        public virtual void ReturnObject(Transform go)
        {
            go.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            SG.ResourceManager.Instance.ReturnObjectToPool(go.gameObject);
        }
    }
}
