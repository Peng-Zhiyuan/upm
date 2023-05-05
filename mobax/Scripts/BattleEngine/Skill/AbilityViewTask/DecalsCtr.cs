namespace BattleEngine.View
{
    using UnityEngine;

    /// <summary>
    /// 贴花表现层
    /// </summary>
    public class DecalsCtr : MonoBehaviour
    {
        public Renderer[] renders;
        public Renderer spreadRender;
        public Transform bk;
        public GameObject outDecals;

        public void SetAsEnemy()
        {
            for (int i = 0; i < renders.Length; i++)
            {
                renders[i].material.SetColor("_Main_Color", Color.red);
                renders[i].material.SetColor("_MainColor", Color.red);
                renders[i].material.SetColor("_AddColor", Color.red);
                renders[i].material.SetColor("_Add2Cololr", Color.red);
            }
        }

        public void SetAsTeam()
        {
            for (int i = 0; i < renders.Length; i++)
            {
                renders[i].material.SetColor("_Main_Color", new Color(0, 0.533f, 0.749f, 1));
                renders[i].material.SetColor("_MainColor", new Color(0, 0.533f, 0.749f, 1));
                renders[i].material.SetColor("_AddColor", new Color(0, 0.533f, 0.749f, 1));
                renders[i].material.SetColor("_Add2Cololr", new Color(0, 0.533f, 0.749f, 1));
            }
        }

        public void HideOutDecals(bool hide)
        {
            if (outDecals == null)
            {
                return;
            }
            outDecals.SetActive(!hide);
        }

        public void SetCircleScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public void SetCircleSpeed(float speed)
        {
            ParticleSystem ps = outDecals.transform.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startLifetime = speed;
        }

        public void SetRayScale(Vector3 scale, Vector3 bklocalPos, Vector3 bkLocalScale)
        {
            transform.localScale = scale;
            bk.transform.localPosition = bklocalPos;
            bk.transform.localScale = bkLocalScale;
        }

        private float rayAddOffset = 0;

        public void SetRaySpeed(float speed)
        {
            rayYieldToFull(speed);
        }

        private void rayYieldToFull(float speed)
        {
            TimerMgr.Instance.BattleSchedulerTimer(0.034f, () =>
                            {
                                if (rayAddOffset >= 1.0f)
                                {
                                    TimerMgr.Instance.Remove("rayYieldToFull");
                                    rayAddOffset = 0;
                                    return;
                                }
                                rayAddOffset += speed * 0.034f;
                                rayAddOffset = Mathf.Min(1f, rayAddOffset);
                                if (spreadRender != null)
                                    spreadRender.material.SetFloat("_AddOffsetY", rayAddOffset);
                            }, true, "rayYieldToFull"
            );
            rayAddOffset = 0;
        }

        public void SetRetangleScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        private float retangleAddOffset = 0;

        public void SetRetangleSpeed(float speed)
        {
            retangleYieldToFull(speed);
        }

        private void retangleYieldToFull(float speed)
        {
            TimerMgr.Instance.BattleSchedulerTimer(0.034f, () =>
                            {
                                if (retangleAddOffset >= 1.0f)
                                {
                                    TimerMgr.Instance.Remove("retangleYieldToFull");
                                    retangleAddOffset = 0;
                                    return;
                                }
                                retangleAddOffset += speed * 0.034f;
                                retangleAddOffset = Mathf.Min(1f, retangleAddOffset);
                                if (spreadRender != null)
                                    spreadRender.material.SetFloat("_AddOffsetY", retangleAddOffset);
                            }, true, "retangleYieldToFull"
            );
        }
    }
}