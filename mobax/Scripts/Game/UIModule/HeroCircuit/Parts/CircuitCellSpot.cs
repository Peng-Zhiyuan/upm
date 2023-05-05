using UnityEngine;
using UnityEngine.UI;

public class CircuitCellSpot : MonoBehaviour
{
    public Image cellLock;

    public void SetLocked(bool locked)
    {
        cellLock.gameObject.SetActive(locked);
    }
}