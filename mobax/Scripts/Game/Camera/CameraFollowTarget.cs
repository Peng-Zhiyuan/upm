using System;
using UnityEngine;

public class CameraFollowTarget : MonoSingleton<CameraFollowTarget>
{
        private void Update()
        {
              SyncCamera();  
        }

        public void SyncCamera()
        {
                if(SceneObjectManager.Instance == null)
                        return;
                
                if(SceneObjectManager.Instance.CurSelectHero == null)
                        return;
                
                if(SceneObjectManager.Instance.CurSelectHero.mData.IsWaitSPSkillState)
                        return;

                transform.position = SceneObjectManager.Instance.CurSelectHero.CameraPoint.transform.position;
        }

        public void SetDirection(Vector3 dir)
        {
                transform.forward = dir;
        }
        
}