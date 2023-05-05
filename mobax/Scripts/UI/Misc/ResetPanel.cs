using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ResetPanel : MonoBehaviour
{
    private const string panelName = "Reset";
    public Image maskImage;
    public float maskFadeTime = 0.3f;
    public float blackPauseTime = 1.0f;

    public GameObject root;
    
    public void CancelB()
    {
        //UIManager.Instance.UnloadUIFromName(panelName);
    }

    public void ConfirmB()
    {
        StartCoroutine(Finish());
    }

    void OnEnabled()
    {
        maskImage.enabled = false;
        root.SetActive(true);
    }

    IEnumerator Finish()
    {
        maskImage.enabled = true;
        StartCoroutine(UIUtility.MaskFade(maskImage, 0f, 1f, maskFadeTime));
        yield return new WaitForSeconds(maskFadeTime);
        root.SetActive(false);
        yield return new WaitForSeconds(blackPauseTime);
        StartCoroutine(UIUtility.MaskFade(maskImage, 1f, 0f, maskFadeTime));
        yield return new WaitForSeconds(maskFadeTime);
        //UIManager.Instance.UnloadUIFromName(panelName);
    }
}
