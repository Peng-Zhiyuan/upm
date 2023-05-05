using UnityEngine;
using System.Collections;

public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);  //防止销毁自己
    }
}