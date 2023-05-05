/* Created:Loki Date:2023-02-18*/

using BattleEngine.Logic;
using BattleSystem.ProjectCore;

namespace BattleEngine.View
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using UnityEngine;

    public class FlowerBulletEffect : MonoBehaviour
    {
        public List<FlowerBulletUnit> bulletLst = new List<FlowerBulletUnit>();

        private GameObject targetActor;
        private Creature createActor;

        public void InitBulletTrans(List<Transform> targetTrans, GameObject targetObj, Creature _createActor)
        {
            createActor = _createActor;
            this.targetActor = targetObj;
            for (int i = 0; i < targetTrans.Count; i++)
            {
                if (i >= bulletLst.Count)
                {
                    break;
                }
                bulletLst[i].gameObject.SetActive(true);
                TransformUtil.InitTransformInfo(bulletLst[i].gameObject, targetTrans[i]);
                bulletLst[i].transform.parent = this.transform;
                bulletLst[i].transform.LookAt(targetObj.transform.position);
            }
        }

        private void Update()
        {
            if (createActor != null
                && createActor.mCurrentState != ACTOR_ACTION_STATE.ATK
                && createActor.mData.isOpenAITree)
            {
                createActor = null;
                DestroyImmediate(gameObject);
            }
            else if (Battle.Instance.mode.ModeType == BattleModeType.SkillView)
            {
                if (createActor.mData.CurrentSkillExecution == null)
                {
                    createActor = null;
                    DestroyImmediate(gameObject);
                }
            }
        }

        public void ResetBullet()
        {
            for (int i = 0; i < bulletLst.Count; i++)
            {
                bulletLst[i].ResetBullet();
            }
        }

        public void ChangePos(float durantion)
        {
            if (targetActor == null)
            {
                return;
            }
            Transform tempTrans = targetActor.transform;
            if (targetActor.GetComponent<Creature>() != null)
            {
                Creature creature = this.targetActor.GetComponent<Creature>();
                tempTrans = creature.GetBone("body_hit");
            }
            if (!Application.isPlaying)
            {
                for (int i = 0; i < bulletLst.Count; i++)
                {
                    Vector3 newPos = GetNewPos();
                    bulletLst[i].transform.position = newPos;
                    bulletLst[i].transform.LookAt(tempTrans);
                }
            }
            else
            {
                for (int i = 0; i < bulletLst.Count; i++)
                {
                    Vector3 newPos = GetNewPos();
                    bulletLst[i].FlyTarget(newPos, tempTrans, durantion);
                }
            }
        }

        public void OnAttack()
        {
            if (targetActor == null)
            {
                return;
            }
            Transform tempTrans = targetActor.transform;
            if (targetActor.GetComponent<Creature>() != null)
            {
                Creature creature = this.targetActor.GetComponent<Creature>();
                tempTrans = creature.GetBone("body_hit");
            }
            for (int i = 0; i < bulletLst.Count; i++)
            {
                bulletLst[i].OnAttack(tempTrans.position);
            }
        }

        public async void OnDestroyBullet()
        {
            for (int i = 0; i < bulletLst.Count; i++)
            {
                bulletLst[i].OnHide();
            }
            await Task.Delay(200);
            if (this != null)
            {
                for (int i = 0; i < bulletLst.Count; i++)
                {
                    bulletLst[i].bulletModel.SetActive(false);
                }
            }
            await Task.Delay(800);
            if (this != null)
            {
                DestroyImmediate(this.gameObject);
            }
        }

        private Vector3 GetNewPos()
        {
            if (targetActor == null)
            {
                return Vector3.zero;
            }
            float radius = 1.5f;
            float minY = 0.5f;
            if (targetActor.GetComponent<Creature>() != null)
            {
                Creature creature = targetActor.GetComponent<Creature>();
                radius = creature.mData.GetAiLocRadiu() + 1.0f;
                minY = creature.mData.GetCenter().y + 0.25f;
            }
            float angle = Random.Range(0, 360);
            Vector3 centerPos = targetActor.transform.position;
            float x = centerPos.x + radius * Mathf.Cos(angle);
            float y = Random.Range(minY, minY + 2);
            float z = centerPos.z + radius * Mathf.Sin(angle);
            return new Vector3(x, y, z);
        }
    }
}