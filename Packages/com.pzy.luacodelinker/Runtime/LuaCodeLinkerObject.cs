﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[ExecuteInEditMode]
public class LuaCodeLinkerObject : MonoBehaviour 
{
    private LuaCodeLinkerObject _parentDesigner;
    public LuaCodeLinkerObject ParentDesigner
    {
        get
        {
            if(Application.isPlaying)
            {
                if(_parentDesigner == null)
                {
                    _parentDesigner = transform.parent?.GetComponentInParent<LuaCodeLinkerObject>();
                }
                return _parentDesigner;
            }
            else
            {
                return transform.parent?.GetComponentInParent<LuaCodeLinkerObject>();

            }

        }
    }

#if UNITY_EDITOR
    public string GetClassName()
    {
        var thisName = this.name;
        thisName = thisName.Trim('$');
        thisName = BigFirstChar(thisName);
        return thisName;

        //var parentDesigner = ParentDesigner;


        //var isPrefabInstance = UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(this.gameObject);
        //if (isPrefabInstance)
        //{
        //    return thisName;
        //}
        //else
        //{
        //    if (parentDesigner != null)
        //    {
        //        //return parentDesigner.GetClassName() + "_" + thisName;
        //        return parentDesigner.GetClassName() + thisName;
        //    }
        //    else
        //    {
        //        return thisName;
        //    }
        //}
    }

#endif

    private string BigFirstChar(string name)
    {
         return name.Substring(0,1).ToUpper() + name.Substring(1);
    }
  

}

