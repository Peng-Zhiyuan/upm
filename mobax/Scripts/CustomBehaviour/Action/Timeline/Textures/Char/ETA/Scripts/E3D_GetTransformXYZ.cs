using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
public class E3D_GetTransformXYZ : MonoBehaviour
{
    [HideInInspector]
    public List<E3DGetTransformXYZInfo> infoLists = new List<E3DGetTransformXYZInfo>();
    [HideInInspector]
    public Dictionary<GameObject, CacheTransformInfo> cacheInfoDic = new Dictionary<GameObject, CacheTransformInfo>();

    Vector3 oldPosition;
    Quaternion oldRotation;
    Vector3 oldScale;
    //保存初始设置
    public void SaveOldTransform()
    {
        oldPosition = transform.position;
        oldRotation = transform.rotation;
        oldScale = transform.localScale;
    }

    //设置初始值
    public void SetOldTransform()
    {
        transform.position = oldPosition;
        transform.rotation = oldRotation;
        transform.localScale = oldScale;
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            SaveOldTransform();
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying)
        {
            SetOldTransform();
        }
    }

    void Start()
    {
        SaveOldTransform();
        SetCacheTransformInfo();//2020-10-27
    }

    void Update()
    {
        //TransformInfoChange(Time.deltaTime);//2020-10-27
        TransformInfoChangeExtension(Time.deltaTime);
    }

    #region Simple
    public void TransformInfoChange(float deltaTime)
    {
        Vector3 ret = Vector3.zero;
        foreach (var info in infoLists)
        {
            if (info == null || info.gameObject == null) continue;
            ret += GetPositionTypeInfo(info.gameObject, info.positionType);
        }
        //transform.position = ret;
        //===============================================================//
        Vector3 target = oldPosition + ret;
        transform.position = target;
        //===============================================================//
    }

    Vector3 GetPositionTypeInfo(GameObject obj, E3D_Position_Axis type)
    {
        Vector3 retValue = Vector3.zero;
        if (obj)
        {
            switch (type)
            {
                case E3D_Position_Axis.X:
                    retValue = Vector3.right * obj.transform.position.x;
                    break;
                case E3D_Position_Axis.Y:
                    retValue = Vector3.up * obj.transform.position.y;
                    break;
                case E3D_Position_Axis.Z:
                    retValue = Vector3.forward * obj.transform.position.z;
                    break;
                default:
                    break;
            }
        }
        return retValue;
    }
    #endregion

    #region Extension
    //===============================================================================//

    public void TransformInfoChangeExtension(float deltaTime)
    {
        //===============================================================//
        Vector3 ret = Vector3.zero;
        foreach (var info in infoLists)
        {
            if (info == null || info.gameObject == null) continue;
            GameObject temp = info.gameObject;
            if (cacheInfoDic.ContainsKey(temp))
            {
                ret += GetPositionTypeInfoExtension(info.gameObject, info.positionType, cacheInfoDic[temp].position);
            }
        }
        //transform.position = ret;
        //===============================================================//
        Vector3 target = oldPosition + ret;
        transform.position = target;
        //===============================================================//
    }

    public void SetCacheTransformInfo()
    {
        cacheInfoDic.Clear();
        foreach (var info in infoLists)
        {
            if (info == null || info.gameObject == null) continue;
            GameObject temp = info.gameObject;
            CacheTransformInfo tempInfo = new CacheTransformInfo();
            tempInfo.position = temp.transform.position;
            tempInfo.rotation = temp.transform.rotation;
            tempInfo.scale = temp.transform.localScale;
            if (!cacheInfoDic.ContainsKey(temp))
            {
                cacheInfoDic.Add(temp, tempInfo);
            }
        }
    }

    Vector3 GetPositionTypeInfoExtension(GameObject obj, E3D_Position_Axis type, Vector3 old)
    {
        Vector3 retValue = Vector3.zero;
        if (obj)
        {
            switch (type)
            {
                case E3D_Position_Axis.X:
                    retValue = Vector3.right * (obj.transform.position - old).x;
                    break;
                case E3D_Position_Axis.Y:
                    retValue = Vector3.up * (obj.transform.position - old).y;
                    break;
                case E3D_Position_Axis.Z:
                    retValue = Vector3.forward * (obj.transform.position - old).z;
                    break;
                default:
                    break;
            }
        }
        return retValue;
    }

    //===============================================================================//
    #endregion
}
