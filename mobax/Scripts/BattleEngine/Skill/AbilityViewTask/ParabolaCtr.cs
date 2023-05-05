namespace BattleEngine.View
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 抛物线
    /// </summary>
    public class ParabolaCtr : MonoBehaviour
    {
        public Vector3 beignPoint = Vector3.zero;
        public Vector3 endPoint = Vector3.zero;
        public float height = 2.0f;
        public int resolution = 10;
        private List<Vector3> _path = new List<Vector3>();
        public LineRenderer lineRenderer;

        public System.Action delegateAnimFinish;

        int currentIndex = 1;

        public void InitParabola()
        {
            currentIndex = 1;
        }

        [ContextMenu("RefreshParabola")]
        public void RefreshParabola()
        {
            _path.Clear();
            Vector3 bezierControlPoint = (beignPoint + endPoint) * 0.5f + (Vector3.up * height);
            _path.Add(beignPoint);
            Vector3 calculatePoint = Vector3.zero;
            for (int i = 1; i < resolution; i++)
            {
                var t = (i + 1) / (float)resolution;
                calculatePoint = GetBezierPoint(t, beignPoint, bezierControlPoint, endPoint);
                _path.Add(calculatePoint);
            }
            if (currentIndex == resolution)
            {
                lineRenderer.positionCount = _path.Count;
                lineRenderer.SetPositions(_path.ToArray());
                if (delegateAnimFinish != null)
                {
                    delegateAnimFinish();
                }
            }
            else
            {
                lineRenderer.positionCount = currentIndex;
                lineRenderer.SetPositions(_path.GetRange(0, currentIndex).ToArray());
                currentIndex += 1;
            }
        }

        public void PrewRefreshParabola()
        {
            _path.Clear();
            Vector3 bezierControlPoint = (beignPoint + endPoint) * 0.5f + (Vector3.up * height);
            _path.Add(beignPoint);
            Vector3 calculatePoint = GetBezierPoint(1, beignPoint, bezierControlPoint, endPoint);
            _path.Add(calculatePoint);
            lineRenderer.positionCount = _path.Count;
            lineRenderer.SetPositions(_path.ToArray());
            delegateAnimFinish?.Invoke();
        }

        /// <param name="t">0到1的值，0获取曲线的起点，1获得曲线的终点</param>
        /// <param name="start">曲线的起始位置</param>
        /// <param name="center">决定曲线形状的控制点</param>
        /// <param name="end">曲线的终点</param>
        public static Vector3 GetBezierPoint(float t, Vector3 start, Vector3 center, Vector3 end)
        {
            return (1 - t) * (1 - t) * start + 2 * t * (1 - t) * center + t * t * end;
        }
    }
}