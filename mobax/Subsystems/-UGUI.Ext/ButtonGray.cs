using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGray : MonoBehaviour
{
    public Image[] btnImg;
    public Text[] btnText;
    public Color[] textColor;
    // Start is called before the first frame update
    public void gray(){
        for(int i=0;i<btnImg.Length;i++){
            UIGray.SetUIGray (btnImg[i]);
        }
        for(int i=0;i<btnText.Length;i++){
            btnText[i].color = new Color(0.78f,0.78f,0.78f,1);
        }
    }
    public void Recovery()
    {
        for(int i=0;i<btnImg.Length;i++){
            UIGray.Recovery (btnImg[i]);
        }
        for(int i=0;i<btnText.Length;i++){
            btnText[i].color = textColor[i];
        }
    }
}
