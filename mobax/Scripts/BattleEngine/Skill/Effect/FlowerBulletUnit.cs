/* Created:Loki Date:2023-02-18*/

using System;
using System.Threading.Tasks;
using BattleEngine.Logic;

namespace BattleEngine.View
{
    using UnityEngine;
    using DG.Tweening;

    public class FlowerBulletUnit : MonoBehaviour
    {
        public GameObject bulletModel;
        public ParticleSystemPlayCtr bulletFx;
        public ParticleSystemPlayCtr bulletHide;
        public ParticleSystemPlayCtr attackFx;
        public ParticleSystemPlayCtr attackTrail;

        

        public void InitPos(Vector3 initPos, Vector3 initRotation)
        {
            
            this.transform.position = initPos;
            this.transform.rotation = Quaternion.Euler(initRotation);
            ResetBullet();
        }

        public void FlyTarget(Vector3 targetPos, Transform lookAt, float duration)
        {
            bulletFx.Play();
            this.transform.DOKill();
            this.transform.LookAt(targetPos);
            Vector3 forward = (lookAt.position - targetPos).normalized;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.transform.DOMove(targetPos, duration).SetEase(Ease.OutQuart));
            sequence.Insert(0, this.transform.DORotate(Quaternion.LookRotation(forward).eulerAngles, duration).SetEase(Ease.Linear));
            sequence.Play();
        }

        public void OnAttack(Vector3 targetPos)
        {
            transform.LookAt(targetPos);
            attackFx.Play();
            attackTrail.transform.LookAt(targetPos);
            attackTrail.transform.DOMove(targetPos, 0.2f).SetEase(Ease.Linear).onComplete = () =>
            {
                attackTrail.Stop();
                attackTrail.transform.localPosition = Vector3.zero;
            };
        }

        public void ResetBullet()
        {
            bulletModel.SetActive(true);
            attackTrail.transform.localPosition = Vector3.zero;
            attackTrail.transform.localRotation = Quaternion.identity;
            bulletFx.Stop();
            attackFx.Stop();
            bulletHide.Stop();
            attackTrail.Stop();
        }

        public void OnHide()
        {
            bulletHide.Play();
        }
    }
}