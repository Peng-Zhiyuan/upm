using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SearchRequest
{
    public int id;
    public string needField;

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + id.GetHashCode();
            if(!string.IsNullOrEmpty(needField))
            {
                hash = hash * 23 + needField.GetHashCode();
            }
            return hash;
        }
    }
}
