using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
public class ModelOutlineImporter : AssetPostprocessor
{
    // 在模型导入前调用
   void OnPreprocessModel()
    {
       if (!assetPath.Contains("Arts/Models") || assetPath.Contains("@@@")) return;
        ModelImporter model = assetImporter as ModelImporter;
        model.importBlendShapeNormals = ModelImporterNormals.None;
        model.importNormals = ModelImporterNormals.Import;
        model.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
        model.normalSmoothingAngle = 60.0f;
    }

    // 在GameObject生成后调用，对GameObject的修改会影响生成结果，但引用不会保留
/*   void OnPostprocessModel(GameObject g)
    {
        if (!assetPath.Contains("Arts/Models") || g.name.Contains("@@@")) return;
        ModelImporter model = assetImporter as ModelImporter;
        FbxImporterTool.PostProcessFBX(model.assetPath);

    }*/
}