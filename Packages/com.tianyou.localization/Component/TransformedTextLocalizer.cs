using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformedTextLocalizer : MonoBehaviour
{
    [SerializeField]
    List<string> leftRotateLangList = new List<string>();

    [SerializeField]
    List<string> rightRotateLangList = new List<string>();

    Text _text;
    Text text{
        get{
            if(_text == null){
                _text = GetComponent<Text>();
            }
            return _text;
        }
    }

    bool right = false;

    bool left = false;

    public void SetLanguage(string langCode)
    {
        if(!text){
            return;
        }
        Reset();
        TryRightRotate(langCode);
        TryLeftRotate(langCode);
    }

    void TryRightRotate(string langCode){
        if(rightRotateLangList.Contains(langCode)){
            Rotate(true);
        }
    }
    
    void TryLeftRotate(string langCode){
        if(leftRotateLangList.Contains(langCode)){
            Rotate(false);
        }
    }

    void Reset(){        
        if(right || left){
            var trans = this.transform as RectTransform;

            var newZ = trans.localRotation.eulerAngles.z;
            if(left) newZ += 90;
            if(right) newZ -= 90;
            var newRotate = Quaternion.Euler(trans.localRotation.eulerAngles.x, trans.localRotation.eulerAngles.y, newZ);
            trans.localRotation = newRotate;

            var with = trans.sizeDelta.x;
            var hight = trans.sizeDelta.y;
            trans.sizeDelta = new Vector2(hight, with);

            this.right = false;
            this.left = false;
        }
    }

    void Rotate(bool turnRight){
        this.right = turnRight;
        this.left = !turnRight;

        var trans = this.transform as RectTransform;
        var newZ = trans.localRotation.eulerAngles.z;
        if(turnRight) newZ += 90;
        if(!turnRight) newZ -= 90;
        var newRotate = Quaternion.Euler(trans.localRotation.eulerAngles.x, trans.localRotation.eulerAngles.y, newZ);
        trans.localRotation = newRotate;

        var with = trans.sizeDelta.x;
        var hight = trans.sizeDelta.y;
        trans.sizeDelta = new Vector2(hight, with);
    }
}
