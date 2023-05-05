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
    // ��ģ�͵���ǰ����
   void OnPreprocessModel()
    {
       if (!assetPath.Contains("Arts/Models") || assetPath.Contains("@@@")) return;
        ModelImporter model = assetImporter as ModelImporter;
        model.importBlendShapeNormals = ModelImporterNormals.None;
        model.importNormals = ModelImporterNormals.Import;
        model.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
        model.normalSmoothingAngle = 60.0f;
    }

    // ��GameObject���ɺ���ã���GameObject���޸Ļ�Ӱ�����ɽ���������ò��ᱣ��
/*   void OnPostprocessModel(GameObject g)
    {
        if (!assetPath.Contains("Arts/Models") || g.name.Contains("@@@")) return;
        ModelImporter model = assetImporter as ModelImporter;
        FbxImporterTool.PostProcessFBX(model.assetPath);

    }*/
}