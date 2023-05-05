using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class KeyRegister : StuffObject<KeyRegister>
{
    [ShowInInspector]
    Dictionary<KeyCode, Action> keyCodeToHandlerDic = new Dictionary<KeyCode, Action>();

    public void Register(KeyCode key, Action handler)
    {
        if(keyCodeToHandlerDic.ContainsKey(key))
        {
            throw new Exception($"[KeyRegister] keyCode: " + key + " has already been registed");
        }
        keyCodeToHandlerDic[key] = handler;
    }

    public void Unregister(KeyCode key)
    {
        keyCodeToHandlerDic.Remove(key);
    }

    void Update()
    {
        var isAnyKeyPressed = Input.anyKey;
        if(isAnyKeyPressed)
        {
            foreach(var kv in keyCodeToHandlerDic)
            {
                var key = kv.Key;
                var handler = kv.Value;
                var b = Input.GetKeyDown(key);
                if(b)
                {
                    handler?.Invoke();
                }
            }
        }
    }
}
