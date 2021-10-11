using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;

[RequireComponent(typeof(TextFontLocalizer))]
public class TextLocalizer : MonoBehaviour
{
    public string key;

    //[SerializeField, ReadOnly]
    private string lastLocalizeLanguage;
    private string[] _parameters = null;
    public string[] parameters{
        set{
            _parameters = value;
        }
        private get{
            return _parameters;
        }
    }

    Text text;

    TextMesh textMesh;

//    TextMeshProUGUI textMeshProUGUI;

    TransformedTextLocalizer transformedTextLocalizer;

    void Awake()
    {
        var text = this.GetComponent<Text>();
        this.text = text;
//        var textMesh = this.GetComponent<TextMesh>();
//        this.textMesh = textMesh;
//        var textMeshProUGUI = this.GetComponent<TextMeshProUGUI>();
//        this.textMeshProUGUI = textMeshProUGUI;

        transformedTextLocalizer = this.GetComponent<TransformedTextLocalizer>();
    }

    void OnEnable()
    {
        RefreshIfNeed(false);
        LanguageManager.LanguageChanged += OnLanguageChange;
    }

    void OnDisable(){
        LanguageManager.LanguageChanged -= OnLanguageChange;
    }

    void OnLanguageChange(string lang){
        RefreshIfNeed(false);
    }

    bool HasTextComponent
    {
        get
        {
            if(text != null)
            {
                return true;
            }

            //if(textMesh != null)
            //{
            //    return true;
            //}

            //if(textMeshProUGUI != null)
            //{
            //    return true;
            //}

            return false;
        }
    }

    public void RefreshIfNeed(bool force = false)
    {
        var hasTextComponent = this.HasTextComponent;
        if (!hasTextComponent)
        {
            return;
        }
        var currentLanguage = LocalizationManager.Stuff.Language;
        if(!force && lastLocalizeLanguage == currentLanguage)
        {
            return;
        }

        Refresh();

        this.lastLocalizeLanguage = currentLanguage;
    }

    void Refresh(){
        SetTransform();
        Localize();
    }

    // 应用文本框的变形，如旋转九十度
    // 这个组件需要单独配置在Text组件同Gameobject上
    void SetTransform(){
        if(transformedTextLocalizer){
            var currentLanguage = LocalizationManager.Stuff.Language;
            transformedTextLocalizer.SetLanguage(currentLanguage);
        }
    }

    private void Localize()
    {
        string msg = null;
        if(parameters != null){
            msg = LocalizationManager.Stuff.GetText(this.key, parameters);
        }
        else{
            msg = LocalizationManager.Stuff.GetText(this.key);
        }
        //// 如果没有 textMesh 组件，不需要本地化
        //if (this.textMesh != null)
        //{
        //    this.textMesh.text = msg;
        //}

        // 如果没有 text 组件，不需要本地化
        if (this.text != null)
        {
            this.text.text = msg;
        }

        //// 如果没有 text 组件，不需要本地化
        //if (this.textMeshProUGUI != null)
        //{
        //    this.textMeshProUGUI.text = msg;
        //}
    }
}
