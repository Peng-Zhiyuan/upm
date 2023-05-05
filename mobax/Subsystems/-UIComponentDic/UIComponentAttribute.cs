using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class UIComponentAttribute : Attribute
{
    public enum E_KeyType
    {
        Name,
        Hierarchy
    }

    public UIComponentAttribute(string key, E_KeyType keyType = E_KeyType.Name, bool isRequired = true)
    {
        Key = key;
        KeyType = keyType;
        IsRequired = isRequired;
    }

    public UIComponentAttribute(string key, bool isRequired)
    {
        Key = key;
        KeyType = E_KeyType.Name;
        IsRequired = isRequired;
    }

    public string Key
    {
        get;
        private set;
    }

    public E_KeyType KeyType
    {
        get;
        private set;
    }

    public bool IsRequired
    {
        get;
        private set;
    }

    public static bool InitComponents(GameObject gameObj, object target)
    {
        if (gameObj == null || target == null) return false;
        UIComponentDict comDict = gameObj.GetComponent<UIComponentDict>();
        if (comDict == null) return true;

        bool allFieldsReady = true;
        Type controllerType = target.GetType();
        var fields = controllerType.GetFields(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Instance
			| System.Reflection.BindingFlags.FlattenHierarchy);
        foreach (var field in fields)
        {
            var attrs = field.GetCustomAttributes(typeof(UIComponentAttribute), false);
            if (attrs == null || attrs.Length != 1) continue;
            var attr = attrs[0] as UIComponentAttribute;
            Transform trans = attr.KeyType == E_KeyType.Name
                ? comDict.GetTransByName(attr.Key)
                : comDict.GetTransByHierarchy(attr.Key);
            if (trans == null)
            {
                if (attr.IsRequired)
                {
                    //Debug.LogWarning(attr.Key + ":" + attr.KeyType + " not found.");
                    allFieldsReady = false;
                }
                continue;
            }
            Type fieldType = field.FieldType;
            if (fieldType.Equals(typeof(Transform)))
            {
                field.SetValue(target, trans);
            }
            else if (fieldType.Equals(typeof(GameObject)))
            {
                field.SetValue(target, trans.gameObject);
            }
            else if (fieldType.IsSubclassOf(typeof(Component)))
            {
                Component comp = trans.GetComponent(fieldType);
                if (comp != null) field.SetValue(target, comp);
                else
                {
					if (attr.IsRequired) {
						Debug.LogWarning(attr.Key + ":" + attr.KeyType + " not found.");
						allFieldsReady = false;
					}
                }
            }
            else
            {
				if (attr.IsRequired) {
					Debug.LogWarning(attr.Key + ":" + attr.KeyType + " not found.");
					allFieldsReady = false;
				}
            }
        }
        return allFieldsReady;
    }
}
