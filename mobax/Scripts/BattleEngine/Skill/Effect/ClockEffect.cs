namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using Logic;

    public class ClockEffect : MonoBehaviour
    {
        public float Offset;
        public float HOffset;

        public void Awake()
        {
#if !SERVER
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            GameObject tmp_instance = this.transform.Find("Clock").gameObject;
            Vector2 xz1 = UnityEngine.Random.insideUnitCircle.normalized * Offset;
            var rotdir = new Vector3(xz1.x, 0, xz1.y);
            rotdir = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * rotdir;
            var headpos = transform.position;
            List<Vector3> poses = new List<Vector3>();
            for (int i = 0; i < 3; i++)
            {
                float hoffset = UnityEngine.Random.Range(-HOffset, HOffset);
                if (MathHelper.DistanceVec3(CameraManager.Instance.MainCamera.transform.position, transform.position + new Vector3(rotdir.x, 0, rotdir.z)) > MathHelper.DistanceVec3(CameraManager.Instance.MainCamera.transform.position, headpos))
                {
                    hoffset = UnityEngine.Random.Range(HOffset * 0.5f, HOffset);
                }
                var pos = new Vector3(rotdir.x, hoffset, rotdir.z);
                poses.Add(transform.position + pos);
                rotdir = Quaternion.AngleAxis(120, Vector3.up) * rotdir;
            }
            for (int i = 0; i < 3; i++)
            {
                var index = UnityEngine.Random.Range(0, poses.Count);
                var pos = poses[index];
                poses.RemoveAt(index);
                BattleTimer.Instance.DelayCall(0.8f * i, delegate(object[] objects)
                                {
                                    tmp_instance.SetActive(false);
                                    tmp_instance.SetActive(true);
                                    float offset = Offset;
                                    tmp_instance.transform.localPosition = pos; //+ new Vector3(UnityEngine.Random.Range(-offset, offset), UnityEngine.Random.Range(-offset, offset), UnityEngine.Random.Range(-offset, offset));
                                    tmp_instance.transform.Find("shunyi").LookAt(this.transform);
                                }
                );
            }
#endif
        }
    }
}