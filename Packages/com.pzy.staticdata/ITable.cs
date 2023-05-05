using System.Collections;
using System.Collections.Generic;
using UnityEngine;
   
public interface ITable
{ 
    bool ContainsKey(string key);
    bool ContainsKey(int key);

    object GetValue(string key);
    List<string> GetKeys();
}
