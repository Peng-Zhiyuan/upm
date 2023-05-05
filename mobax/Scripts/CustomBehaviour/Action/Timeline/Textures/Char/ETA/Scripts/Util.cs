using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#region Util
public static class Matrix4x4Utils
{
    public enum E_AXIS
    {
        E_AXIS_X,
        E_AXIS_Y,
        E_AXIS_Z,
    }
    /// <summary>
    /// increasePos移动
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="increasePos"></param>
    static public void Matrix4x4Translate(this Transform transform, Vector3 increasePos)
    {
        Vector4 v = new Vector4(transform.position.x,
            transform.position.y,
            transform.position.z,
            1);


        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.m03 = increasePos.x;
        matrix.m13 = increasePos.y;
        matrix.m23 = increasePos.z;


        v = matrix * v;


        transform.position = new Vector3(v.x, v.y, v.z);
    }
    /// <summary>
    /// 通过矩阵基本功照顾物体的Rotate
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="euler"></param>
    public static void Matrix4x4Rotate(this Transform transform, Vector3 euler)
    {
        //Unity默认的旋转的Z-X-Y，计算出矩阵变换
        Matrix4x4 matrix = Matrix4x4Rotation(E_AXIS.E_AXIS_Y, euler.y) * Matrix4x4Rotation(E_AXIS.E_AXIS_X, euler.x) * Matrix4x4Rotation(E_AXIS.E_AXIS_Z, euler.z);
        //Debug.LogError(matrix.ToString());
        transform.rotation = GetRotation(matrix);
    }
    /// <summary>
    /// 欧拉角转矩阵变换
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    static private Matrix4x4 Matrix4x4Rotation(E_AXIS axis, float angle)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        //
        if (axis == E_AXIS.E_AXIS_X)
        {
            float rad = angle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);
            matrix = Matrix4x4.identity;
            matrix.m11 = cos;
            matrix.m12 = -sin;
            matrix.m21 = sin;
            matrix.m22 = cos;
        }
        else if (axis == E_AXIS.E_AXIS_Y)
        {
            float rad = angle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);
            matrix.m22 = cos;
            matrix.m20 = -sin;
            matrix.m02 = sin;
            matrix.m00 = cos;
        }
        else if (axis == E_AXIS.E_AXIS_Z)
        {
            float rad = angle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);
            matrix.m00 = cos;
            matrix.m01 = -sin;
            matrix.m10 = sin;
            matrix.m11 = cos;
        }
        return matrix;
    }
    /// <summary>
    /// 通过矩阵对物体进行缩放
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="MultipyScale"></param>
    static public void Matrix4x4Scale(this Transform transform, Vector3 MultipyScale)
    {
        Vector4 v = new Vector4(
           transform.localScale.x,
           transform.localScale.y,
           transform.localScale.z,
           1);

        Matrix4x4 matrix = Matrix4x4.identity;

        matrix.m00 = MultipyScale.x;
        matrix.m11 = MultipyScale.y;
        matrix.m22 = MultipyScale.z;

        v = matrix * v;

        transform.localScale = new Vector3(v.x, v.y, v.z);
    }
    /// <summary>
    /// 从矩阵中获取平移向量
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    static public Vector3 GetPostition(this Matrix4x4 matrix)
    {
        return new Vector3(matrix.m03, matrix.m13, matrix.m23);
    }
    /// <summary>
    /// 矩阵转四元数的算法
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    static public Quaternion GetRotation(this Matrix4x4 matrix)
    {
        float[] q = new float[4];
        int i, j;
        float QW, QX, QY, QZ;

        q[0] = +matrix.m00 + matrix.m11 + matrix.m22;
        q[1] = +matrix.m00 - matrix.m11 - matrix.m22;
        q[2] = -matrix.m00 + matrix.m11 - matrix.m22;
        q[3] = -matrix.m00 - matrix.m11 + matrix.m22;

        j = 0;//q[0]是最大的
        for (i = 1; i < 4; i++) j = (q[i] > q[j]) ? i : j;

        float biggest = (float)(Mathf.Sqrt(q[j] + 1) * 0.5f);
        float mult = 0.25f / biggest;
        //谁大取谁，结合一下公式
        Quaternion quaternion = Quaternion.identity;
        if (j == 0)
        {
            QW = biggest;
            QX = (matrix.m12 - matrix.m21) * mult;
            QY = (matrix.m20 - matrix.m02) * mult;
            QZ = (matrix.m01 - matrix.m10) * mult;
        }
        else if (j == 1)
        {
            QW = (matrix.m12 - matrix.m21) * mult;
            QX = biggest;
            QY = (matrix.m01 + matrix.m10) * mult;
            QZ = (matrix.m20 + matrix.m02) * mult;
        }
        else if (j == 2)
        {
            QW = (matrix.m20 - matrix.m02) * mult;
            QX = (matrix.m01 + matrix.m10) * mult;
            QY = biggest;
            QZ = (matrix.m12 + matrix.m21) * mult;
        }
        else
        {
            QW = (matrix.m01 - matrix.m10) * mult;
            QX = (matrix.m20 + matrix.m02) * mult;
            QY = (matrix.m12 + matrix.m21) * mult;
            QZ = biggest;
        }
        //四元数转矩阵的底层算法：
        //public Quaternion MatrixToQuaternion(Matrix4x4 matrix)
        //{
        //    float qw = Mathf.Sqrt(1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
        //    float w = 4 * qw;
        //    float qx = (matrix.m21 - matrix.m12) / w;
        //    float qy = (matrix.m02 - matrix.m20) / w;
        //    float qz = (matrix.m10 - matrix.m01) / w;
        //    return new Quaternion(qx, qy, qz, qw);
        //}
        return new Quaternion(-QX, -QY, -QZ, QW);
    }
    /// <summary>
    /// 矩阵获取
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    static public Vector3 GetScale(this Matrix4x4 matrix)
    {
        float x = Mathf.Sqrt(matrix.m00 * matrix.m00 + matrix.m01 * matrix.m01 + matrix.m02 * matrix.m02);
        float y = Mathf.Sqrt(matrix.m10 * matrix.m10 + matrix.m11 * matrix.m11 + matrix.m12 * matrix.m12);
        float z = Mathf.Sqrt(matrix.m20 * matrix.m20 + matrix.m21 * matrix.m21 + matrix.m22 * matrix.m22);
        return new Vector3(x, y, z);
    }
}

public static class QuaternionUtils
{
    /// <summary>
    /// 计算从A到B的旋转差异
    /// Quaternion.identity就是指Quaternion(0,0,0,0),就是每旋转前的初始角度,是一个确切的值,而transform.rotation是指本物体的角度,值是不确定的,比如可以这么设置transform.rotation = Quaternion.identity;
    /// 一个是值类型,一个是属性变量
    /// 利用四元数的差
    /// </summary>
    /// <param name="B">四元数B</param>
    /// <param name="A">四元素A</param>
    /// <returns></returns>
    public static Quaternion SubtractRotation(Quaternion B, Quaternion A)
    {
        //Inverse 取反
        Quaternion C = Quaternion.Inverse(A) * B;
        return C;
    }
}
#endregion

#region E3D_Material_Animation

/// <summary> shader 属性类型 </summary>
public enum EShaderPropertyType
{
    Range = 0,
    Float,
    //贴图Tiling/Offset   
    TexEnv,
    Color,
}
public struct TexST
{
    public string tiling_X;
    public string tiling_Y;
    public string offset_X;
    public string offset_Y;

    public TexST(string a, string b, string c, string d)
    {
        this.tiling_X = a;
        this.tiling_Y = b;
        this.offset_X = c;
        this.offset_Y = d;
    }
};

public struct EMatInfo
{
    public Renderer render;
    public Material currentMat;
    public Material defualtMat;
};

public class MaterialUpdatingPropertyValue
{
    Renderer selfRender;
    Renderer[] childRenderers;
    Material[] materials;
    Dictionary<Renderer, Material[]> excuteDic = new Dictionary<Renderer, Material[]>();

    public MaterialUpdatingPropertyValue() { }

    public MaterialUpdatingPropertyValue(Transform target, bool isEditor)
    {
        excuteDic.Clear();
        //===================//
        this.selfRender = target.GetComponent<Renderer>();
        //===================//
        if (selfRender)
        {
            if (isEditor)
                this.materials = this.selfRender.sharedMaterials;
            else
                this.materials = this.selfRender.materials;
            //===================//
            if (!excuteDic.ContainsKey(selfRender))
            {
                if (isEditor)
                    excuteDic.Add(selfRender, selfRender.sharedMaterials);
                else
                    excuteDic.Add(selfRender, selfRender.materials);
            }
        }
        //===================//  
        this.childRenderers = target.GetComponentsInChildren<Renderer>();
        //===================//
        if (childRenderers.Length > 0)
        {
            for (int i = 0; i < childRenderers.Length; i++)
            {
                Renderer r = childRenderers[i];
                if (!excuteDic.ContainsKey(r))
                {
                    if (isEditor)
                        excuteDic.Add(r, r.sharedMaterials);
                    else
                        excuteDic.Add(r, r.materials);
                }
            }
        }
        //===================//
    }

    #region ----\\ Material ==> Float 属性修改 //----
    public void UpdatingFloatValue(string property, float value)
    {
        foreach (var mat in materials)
        {
            if (mat.HasProperty(property))
            {
                mat.SetFloat(property, value);
            }
        }
    }

    public void UpdatingFloatValueApplyChild(string property, float value)
    {
        foreach (var matGroup in excuteDic.Values)
        {
            foreach (var mat in matGroup)
            {
                if (mat.HasProperty(property))
                {
                    mat.SetFloat(property, value);
                }
            }
        }
    }
    #endregion

    #region ----\\ Material ==> Color 属性修改 //----
    public void UpdatingColorValue(string property, Color value)
    {
        foreach (var mat in materials)
        {
            if (mat.HasProperty(property))
            {
                mat.SetColor(property, value);
            }
        }
    }

    public void UpdatingColorValueApplyChild(string property, Color value)
    {
        foreach (var matGroup in excuteDic.Values)
        {
            foreach (var mat in matGroup)
            {
                if (mat.HasProperty(property))
                {
                    mat.SetColor(property, value);
                }
            }
        }
    }
    #endregion

    #region ----\\ Material ==> TextureST 属性修改 //----
    public void UpdatingTexSTValue(string property, string specificStr, string axis, float value)
    {
        foreach (var mat in materials)
        {
            if (mat.HasProperty(property))
            {
                SetShedrtTexST(mat, property, specificStr, axis, value);
            }
        }
    }

    public void UpdatingTexSTValueApplyChild(string property, string specificStr, string axis, float value)
    {
        foreach (var matGroup in excuteDic.Values)
        {
            foreach (var mat in matGroup)
            {
                if (mat.HasProperty(property))
                {
                    SetShedrtTexST(mat, property, specificStr, axis, value);
                }
            }
        }
    }

    #region TextureST
    private const string tiling = "Tiling";
    private const string offset = "Offset";
    private const string x = "X";
    private const string y = "Y";
    private Vector2 _tempST = Vector2.zero;
    public void SetShedrtTexST(Material mat, string propertyName, string specificStr, string axis, float value)
    {
        if (string.Equals(specificStr, tiling))
        {
            _tempST = mat.GetTextureScale(propertyName);
            if (axis == x)
            {
                mat.SetTextureScale(propertyName, new Vector2(value, _tempST.y));
            }
            else if (axis == y)
            {
                mat.SetTextureScale(propertyName, new Vector2(_tempST.x, value));
            }
        }
        else if (string.Equals(specificStr, offset))
        {
            _tempST = mat.GetTextureOffset(propertyName);

            if (axis == x)
            {
                mat.SetTextureOffset(propertyName, new Vector2(value, _tempST.y));
            }
            else if (axis == y)
            {
                mat.SetTextureOffset(propertyName, new Vector2(_tempST.x, value));
            }
        }
    }
    #endregion

    #endregion

}



#region Editor面板扩展类
/// <summary>
/// 材质属性基类
/// </summary>
[System.Serializable]
public class CurveAnimShaderProBase
{
    /// <summary>
    /// 属性名
    /// </summary>
    public string propertyName;
    /// <summary>
    /// 是否应用到所有子对象
    /// </summary>
    public bool applyChild = true;

    /// <summary>
    /// 曲线倍率
    /// </summary>
    public float scaleCurve = 1;

    /// <summary>
    /// 默认是否使用自己的曲线
    /// </summary>
    public bool useSelfAnimCurve = false;
    /// <summary>
    /// 动画曲线
    /// </summary>
    public AnimationCurve animCurve = new AnimationCurve();
}

/// <summary>
/// 材质Color类型数据
/// </summary>
[System.Serializable]
public class CurveAnimColorShaderPro : CurveAnimShaderProBase
{
    [SerializeField]
    /// <summary>
    /// 颜色渐变
    /// </summary>
    public Gradient colorGradient = new Gradient();

    public CurveAnimColorShaderPro() { }
    public CurveAnimColorShaderPro(Gradient gradient, string colorProperty, bool applyChild = true)
    {
        this.colorGradient = gradient;
        this.applyChild = applyChild;
        this.propertyName = colorProperty;
    }

}

/// <summary>
/// 材质Range类型数据
/// </summary>
[System.Serializable]
public class CurveAnimRangeShaderPro : CurveAnimShaderProBase
{
    public float rangeResult = 0;
    public float rangeInputMin = 0;
    public float rangeInputMax = 1;
    public CurveAnimRangeShaderPro() { }
    public CurveAnimRangeShaderPro(float range, float rangIdleMin, float rangeIdleMax)
    {
        this.rangeResult = range;
        this.rangeInputMin = rangIdleMin;
        this.rangeInputMax = rangeIdleMax;
    }
}

/// <summary>
/// 材质Float类型数据
/// </summary>
[System.Serializable]
public class CurveAnimFloatShaderPro : CurveAnimShaderProBase
{
    public float floatResult = 0;

    public CurveAnimFloatShaderPro() { }
    public CurveAnimFloatShaderPro(float result)
    {
        this.floatResult = result;
    }
}

/// <summary>
/// 材质TextureST类型数据
/// </summary>
[System.Serializable]
public class CurveAnimTexSTShaderPro : CurveAnimShaderProBase
{
    public Vector2 textureScale;
    public Vector2 textureOffset;
    public float floatResult = 0;
    /// <summary>
    /// shader 贴图对应的属性名
    /// </summary>
    public string targetPropertyName;
    /// <summary>
    /// tiling / Offset x或者y分量
    /// </summary>
    public string axisName;
    /// <summary>
    /// 具体是Tiling / Offset
    /// </summary>
    public string specificStr;
    public CurveAnimTexSTShaderPro() { }
    public CurveAnimTexSTShaderPro(Vector2 tiling, Vector2 offset)
    {
        this.textureScale = tiling;
        this.textureOffset = offset;
    }
}



#endregion
#endregion

#region E3D_GetTransformToShader
public enum E3DGetTransformType
{
    Position = 0,
    Rotation,
    Scale,
}

[System.Serializable]
public class E3DGetTransformToShaderInfo
{
    public GameObject gameObject;
    public E3DGetTransformType transformType = E3DGetTransformType.Position;
    public int transformTypeIndex;
    public string propertyName;
    public bool applyChild = false;
}
#endregion

#region E3D_GetTransformXYZ
public enum E3D_Position_Axis
{
    X = 0,
    Y,
    Z
}

[System.Serializable]
public class E3DGetTransformXYZInfo
{
    public GameObject gameObject;
    public E3D_Position_Axis positionType = E3D_Position_Axis.X;
    public int positionTypeIndex;
}
#endregion

#region E3D_Rotate_Self
public enum E3D_Rotate_Axis
{
    X = 0,
    Y,
    Z
}
#endregion

#region E3D_BB
public enum E3D_BillBoard
{
    YLock = 0,
    Free
}
#endregion

#region Common
public struct OldTransformInfo
{
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public Vector3 localScale;
};

public struct CacheTransformInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
};
#endregion

#region E3D_Move_Direction
public enum E3D_Move_Enum
{
    right = 0,
    up,
    forward,
    left,
    down,
    back,
}
#endregion

