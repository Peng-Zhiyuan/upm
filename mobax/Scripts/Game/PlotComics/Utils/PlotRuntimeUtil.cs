using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plot.Runtime
{
    public class PlotRuntimeUtil
    {
        public static void ClearAllChildren(GameObject obj, string exceptName = "")
        {
            if (obj == null)
            {
                return;
            }

            if (obj.transform.childCount <= 0) return;
            // 正序删除可能影响节点删不干净
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(exceptName))
                {
                    Object.DestroyImmediate(obj.transform.GetChild(i).gameObject);
                    continue;
                }

                if (obj.name == exceptName)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// 是否相交  z轴是相反的
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool JudgeOverlaps(Tuple<Vector3, Vector3> a, Tuple<Vector3, Vector3> b)
        {
            var min = Vector3.Max(a.Item1, b.Item1);
            var max = Vector3.Min(a.Item2, b.Item2);
            return min.x < max.x && min.z < max.z;
        }


        /// <summary>
        /// 运行模式下Texture转换成Texture2D
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D TextureToTexture2D(Texture texture)
        {
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture2D;
        }

        /// <summary>
        /// 将Texture2d转换为Sprite
        /// </summary>
        /// <param name="tex">参数是texture2d纹理</param>
        /// <returns></returns>
        public static Sprite TextureToSprite(Texture2D tex)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
    }
}