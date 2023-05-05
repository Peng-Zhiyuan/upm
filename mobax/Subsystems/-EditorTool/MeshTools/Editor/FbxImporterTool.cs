using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
public class FbxImporterTool 
{
    public struct CollectNormalJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> normals, vertrx;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<UnsafeHashMap<Vector3, Vector3>.ParallelWriter> result;

        public CollectNormalJob(NativeArray<Vector3> normals, NativeArray<Vector3> vertrx, NativeArray<UnsafeHashMap<Vector3, Vector3>.ParallelWriter> result)
        {
            this.normals = normals;
            this.vertrx = vertrx;
            this.result = result;
        }

        void IJobParallelFor.Execute(int index)
        {
            for (int i = 0; i < result.Length + 1; i++)
            {
                if (i == result.Length)
                {
                   // Debug.LogError("important log,keep use!");//important log,keep use!
                    break;
                }

                Debug.Log($">>>>>>>>>>>>>>>>>>>>>>>>>>>{result[i]}");//important log,keep use!

                if (result[i].TryAdd(vertrx[index], normals[index]))
                {
                    break;
                }
            }
        }
    }

    public struct BakeNormalToUV2Job : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> vertrx, normals;
        [ReadOnly] public NativeArray<Vector4> tangents;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<UnsafeHashMap<Vector3, Vector3>> result;
        public NativeArray<Vector2> uv2s;

        public BakeNormalToUV2Job(NativeArray<Vector3> vertrx, NativeArray<Vector3> normals, NativeArray<Vector4> tangents, NativeArray<UnsafeHashMap<Vector3, Vector3>> result, NativeArray<Vector2> uv2s)
        {
            this.vertrx = vertrx;
            this.normals = normals;
            this.tangents = tangents;
            this.result = result;
            this.uv2s = uv2s;
        }

        void IJobParallelFor.Execute(int index)
        {
            Vector3 smoothedNormals = Vector3.zero;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i][vertrx[index]] != Vector3.zero)
                    smoothedNormals += result[i][vertrx[index]];
                else
                    break;
            }
            smoothedNormals = smoothedNormals.normalized;

            var binormal = (Vector3.Cross(normals[index], tangents[index]) * tangents[index].w).normalized;

            var tbn = new Matrix4x4(
                tangents[index],
                binormal,
                normals[index],
                Vector4.zero);
            tbn = tbn.transpose;

            var bakedNormal = tbn.MultiplyVector(smoothedNormals).normalized;

            Vector2 uv2 = new Vector2();
            var x = bakedNormal.x * 0.5f + 0.5f;
            var y = bakedNormal.y * 0.5f + 0.5f;
            var z = bakedNormal.z * 0.5f + 0.5f;

            //pack x,y to uv2.x
            x = Mathf.Round(x * 15);
            y = Mathf.Round(y * 15);
            var packed = Vector2.Dot(new Vector2(x, y), new Vector2((float)(1.0 / (255.0 / 16.0)), (float)(1.0 / 255.0)));

            //store to UV2
            uv2.x = packed;
            uv2.y = z;
            this.uv2s[index] = uv2;
        }
    }

    public struct BakeNormalToColorJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> vertrx, normals;
        [ReadOnly] public NativeArray<Vector4> tangents;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<UnsafeHashMap<Vector3, Vector3>> result;
        [ReadOnly] public bool existColors;
        public NativeArray<Color> colors;

        public BakeNormalToColorJob(NativeArray<Vector3> vertrx, NativeArray<Vector3> normals, NativeArray<Vector4> tangents, NativeArray<UnsafeHashMap<Vector3, Vector3>> result, bool existColors, NativeArray<Color> colors)
        {
            this.vertrx = vertrx;
            this.normals = normals;
            this.tangents = tangents;
            this.result = result;
            this.existColors = existColors;
            this.colors = colors;
        }

        void IJobParallelFor.Execute(int index)
        {
            Vector3 smoothedNormals = Vector3.zero;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i][vertrx[index]] != Vector3.zero)
                    smoothedNormals += result[i][vertrx[index]];
                else
                    break;
            }
            smoothedNormals = smoothedNormals.normalized;

            var binormal = (Vector3.Cross(normals[index], tangents[index]) * tangents[index].w).normalized;

            var tbn = new Matrix4x4(
                tangents[index],
                binormal,
                normals[index],
                Vector4.zero);
            tbn = tbn.transpose;

            var bakedNormal = tbn.MultiplyVector(smoothedNormals).normalized;

            Color color = new Color();
            color.r = (bakedNormal.x * 0.5f) + 0.5f;
            color.g = (bakedNormal.y * 0.5f) + 0.5f;
            color.b = existColors ? colors[index].b : 1;
            color.a = existColors ? colors[index].a : 1;
            colors[index] = color;
        }

    }
    private static void SmoothModel(string dst, string src)
    {
        var g = AssetDatabase.LoadAssetAtPath<GameObject>(src);
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(dst);
        Dictionary<string, Mesh> originalMesh = GetMesh(g);
        Dictionary<string, Mesh> smoothedMesh = GetMesh(go);
        if (go == null)
        {
            Debug.LogError(dst+" is null");
        }
        if (smoothedMesh != null)
        {
            /*
            foreach (var item in smoothedMesh)
            {
                Debug.LogError("smoothedMesh key:"+item.Key);
            }
            foreach (var item in originalMesh)
            {
                Debug.LogError("originalMesh key:" + item.Key);
            }
            */
            foreach (var item in originalMesh)
            {
                var m = item.Value;
                if (smoothedMesh.ContainsKey(item.Key))
                {
                    Debug.Log(src + " ComputeSmoothedNormalToUV2 Suc:" + item.Key);
                    m.uv2 = ComputeSmoothedNormalToUV2ByJob(smoothedMesh[item.Key], m);
                }
                else
                {
                    throw new System.Exception("key not find:" + item.Key);
                    //Debug.LogError("key not find:" + item.Key);
                }
            }
        }
        AssetDatabase.DeleteAsset(dst);
        Debug.LogError("smooth success: "+ src);
    }
    private static Dictionary<string, Mesh> GetMesh(GameObject go)
    {
        Dictionary<string, Mesh> dic = new Dictionary<string, Mesh>();
        foreach (var item in go.GetComponentsInChildren<MeshFilter>(true))
            dic.Add(item.name.Replace("@@@",""), item.sharedMesh);
        if (dic.Count == 0)
            foreach (var item in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                dic.Add(item.name.Replace("@@@", ""), item.sharedMesh);
        return dic;
    }
    private static Vector2[] ComputeSmoothedNormalToUV2ByJob(Mesh smoothedMesh, Mesh originalMesh, int maxOverlapvertices = 10)
    {
        int svc = smoothedMesh.vertexCount, ovc = originalMesh.vertexCount;
        // CollectNormalJob Data
        NativeArray<Vector3> normals = new NativeArray<Vector3>(smoothedMesh.normals, Allocator.Persistent),
            vertrx = new NativeArray<Vector3>(smoothedMesh.vertices, Allocator.Persistent),
            smoothedNormals = new NativeArray<Vector3>(svc, Allocator.Persistent);
        var result = new NativeArray<UnsafeHashMap<Vector3, Vector3>>(maxOverlapvertices, Allocator.Persistent);
        var resultParallel = new NativeArray<UnsafeHashMap<Vector3, Vector3>.ParallelWriter>(result.Length, Allocator.Persistent);
        // NormalBakeJob Data
        NativeArray<Vector3> normalsO = new NativeArray<Vector3>(originalMesh.normals, Allocator.Persistent),
            vertrxO = new NativeArray<Vector3>(originalMesh.vertices, Allocator.Persistent);
        var tangents = new NativeArray<Vector4>(originalMesh.tangents, Allocator.Persistent);
        var uv2s = new NativeArray<Vector2>(ovc, Allocator.Persistent);

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new UnsafeHashMap<Vector3, Vector3>(svc, Allocator.Persistent);
            resultParallel[i] = result[i].AsParallelWriter();
        }

        bool existUV2 = originalMesh.uv2.Length == ovc;
        if (existUV2)
            uv2s.CopyFrom(originalMesh.uv2);

        CollectNormalJob collectNormalJob = new CollectNormalJob(normals, vertrx, resultParallel);
        BakeNormalToUV2Job normalBakeJob = new BakeNormalToUV2Job(vertrxO, normalsO, tangents, result, uv2s);

        normalBakeJob.Schedule(ovc, 100, collectNormalJob.Schedule(svc, 100)).Complete();

        Vector2[] resultUV2 = new Vector2[uv2s.Length];
        uv2s.CopyTo(resultUV2);

        normals.Dispose();
        vertrx.Dispose();
        result.Dispose();
        smoothedNormals.Dispose();
        resultParallel.Dispose();
        normalsO.Dispose();
        vertrxO.Dispose();
        tangents.Dispose();
        uv2s.Dispose();

        return resultUV2;
    }

    Color[] ComputeSmoothedNormalToColorByJob(Mesh smoothedMesh, Mesh originalMesh, int maxOverlapvertices = 10)
    {
        int svc = smoothedMesh.vertexCount, ovc = originalMesh.vertexCount;
        // CollectNormalJob Data
        NativeArray<Vector3> normals = new NativeArray<Vector3>(smoothedMesh.normals, Allocator.Persistent),
            vertrx = new NativeArray<Vector3>(smoothedMesh.vertices, Allocator.Persistent),
            smoothedNormals = new NativeArray<Vector3>(svc, Allocator.Persistent);
        var result = new NativeArray<UnsafeHashMap<Vector3, Vector3>>(maxOverlapvertices, Allocator.Persistent);
        var resultParallel = new NativeArray<UnsafeHashMap<Vector3, Vector3>.ParallelWriter>(result.Length, Allocator.Persistent);
        // NormalBakeJob Data
        NativeArray<Vector3> normalsO = new NativeArray<Vector3>(originalMesh.normals, Allocator.Persistent),
            vertrxO = new NativeArray<Vector3>(originalMesh.vertices, Allocator.Persistent);
        var tangents = new NativeArray<Vector4>(originalMesh.tangents, Allocator.Persistent);
        var colors = new NativeArray<Color>(ovc, Allocator.Persistent);

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new UnsafeHashMap<Vector3, Vector3>(svc, Allocator.Persistent);
            resultParallel[i] = result[i].AsParallelWriter();
        }

        bool existColors = originalMesh.colors.Length == ovc;
        if (existColors)
            colors.CopyFrom(originalMesh.colors);

        CollectNormalJob collectNormalJob = new CollectNormalJob(normals, vertrx, resultParallel);
        BakeNormalToColorJob normalBakeJob = new BakeNormalToColorJob(vertrxO, normalsO, tangents, result, existColors, colors);

        normalBakeJob.Schedule(ovc, 100, collectNormalJob.Schedule(svc, 100)).Complete();

        Color[] resultColors = new Color[colors.Length];
        colors.CopyTo(resultColors);

        normals.Dispose();
        vertrx.Dispose();
        result.Dispose();
        smoothedNormals.Dispose();
        resultParallel.Dispose();
        normalsO.Dispose();
        vertrxO.Dispose();
        tangents.Dispose();
        colors.Dispose();
        return resultColors;
    }

    public static async void PostProcessFBX(string assetPath)
    {
        if (assetPath.Contains("@") || assetPath.Contains("@@@") || !assetPath.ToUpper().EndsWith(".FBX"))
        {
            return;
        }
        Debug.Log("try smooth:"+assetPath);
        ModelImporter originModelImporter = (ModelImporter)AssetImporter.GetAtPath(assetPath);
        originModelImporter.isReadable = true;
        EditorUtility.SetDirty(originModelImporter);
        AssetDatabase.Refresh();

        string src = assetPath;
        string dst = Path.GetDirectoryName(src) + "/@@@" + Path.GetFileName(src);

        var full_path = Application.dataPath + "/" + dst.Substring(7);
        if (!File.Exists(full_path))
        {
            AssetDatabase.CopyAsset(src, dst);
            AssetDatabase.ImportAsset(dst);
        }
        //await Task.Delay(1000);
        ModelImporter smoothModelImporter = (ModelImporter)AssetImporter.GetAtPath(dst);
        smoothModelImporter.importBlendShapeNormals = ModelImporterNormals.None;
        smoothModelImporter.importNormals = ModelImporterNormals.Calculate;
        smoothModelImporter.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
        smoothModelImporter.normalSmoothingAngle = 180.0f;
        smoothModelImporter.isReadable = true;

        SmoothModel(dst.Replace("\\", "/"), src);
        //await Task.Delay(1000);
        originModelImporter.importBlendShapeNormals = ModelImporterNormals.None;
        originModelImporter.importNormals = ModelImporterNormals.Import;
        originModelImporter.isReadable = false;
        EditorUtility.SetDirty(originModelImporter);
        AssetDatabase.Refresh();

    }

    private static List<string> GetAllAssetsAtPath(string path)
    {
        List<string> list = new List<string>();
        path = path.Replace("Assets", "");
        string[] spritesPath = Directory.GetFiles(Application.dataPath + path, "*", SearchOption.AllDirectories);
        //循环遍历每一个路径，单独加P
        foreach (string spritePath in spritesPath)
        {     //替换路径中的反斜杠为正斜杠       
            string tempPath = spritePath.Replace(@"\", "/");     //截取我们需要的路径
            tempPath = tempPath.Substring(tempPath.IndexOf("Assets"));
            //根据路径加载资源
            if (tempPath != null) list.Add(tempPath);
        }
        return list;
    }

    [MenuItem("Tools/AutoSmoothNormalToUV2")]
    public static void AutoSmoothNormalToUV2()
    {
        List<string> pathList = GetAllAssetsAtPath("Assets/Arts/Models/");
        if (pathList.Count == 0) return;
        for (int i = 0; i < pathList.Count; i++)
        {
            PostProcessFBX(pathList[i]);
        }
    }

    [MenuItem("Assets/CustomBrushTools/FbxSmoothNormalToUV2")]
	public static void SmoothNormalToUV2()
	{
		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (UnityEngine.Object activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
            PostProcessFBX(resPath);
        }
	}


}
