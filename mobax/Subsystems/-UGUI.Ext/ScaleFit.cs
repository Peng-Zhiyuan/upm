using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crystal;
namespace ScreenFit
{
    public enum FillType
    {
        FillByWidth,
        FillByHeight,
        FillInSize,
        FillOutSize,
        KeepSize
    }
    public class ScaleFit : MonoBehaviour
    {
        public FillType filType = FillType.FillOutSize;
        private int design_height = 2340;
        private int design_width = 1080;
        private RectTransform rectTransform = null;
        private Vector3 cacheScale;
        private RectTransform safeArea;
        public bool playOnLoad = true;

        public void Awake()
        {
            this.cacheScale = this.transform.localScale;
            var rectTransForm = this.GetComponent<RectTransform>();
            if (rectTransForm != null)
            {
                design_width = (int)rectTransForm.sizeDelta.x;
                design_height = (int)rectTransForm.sizeDelta.y;
            }
            if (playOnLoad)
            {
                this.FitSize();
            }
            UIEngine.Stuff.CanvasSizeChnaged += FitSize;


        }
        void OnDestroy()
        {
            UIEngine.Stuff.CanvasSizeChnaged -= FitSize;
        }
        
        //[Sirenix.OdinInspector.ShowInInspector]
        public void FitSize()
        {
           
            float y_uv_offset = 0.5f - (SafeArea.anchorMax.y + SafeArea.anchorMin.y) / 2;
            float y_offset = y_uv_offset * Screen.height / CanvasScale;
            this.transform.localPosition = Vector3.up * y_offset;
            //Debug.LogError("y_uv_offset:"+ y_uv_offset+ "  y_offset:"+ y_offset);
           /* Debug.LogError("this.cacheScale :" + this.cacheScale);
            Debug.LogError("filType:" + filType);*/
            switch (filType)
            {
                case FillType.FillByWidth: this.transform.localScale = this.cacheScale  *  this.FixWidthScale; break;
                case FillType.FillByHeight: this.transform.localScale = this.cacheScale * this.FixHeightScale; break;
                case FillType.FillInSize: this.transform.localScale = this.cacheScale * this.FixInScale; break;
                case FillType.FillOutSize: this.transform.localScale = this.cacheScale * this.FixOutScale; break;
                case FillType.KeepSize:this.transform.localScale = this.cacheScale;break;
                default: break;
            }
        }

        public RectTransform SafeArea
        {
            get
            {
                if (safeArea == null) safeArea = UIEngine.Stuff.transofrm_safeArea;
                return safeArea;
            }
        }

        public Vector2 CanvasSize
        {
            get
            {
                if (rectTransform == null) rectTransform = UIEngine.Stuff.Canvas.GetComponent<RectTransform>();
                return rectTransform.sizeDelta;
            }
        }

        public float CanvasScale
        {
            get
            {
                if (rectTransform == null) rectTransform = UIEngine.Stuff.Canvas.GetComponent<RectTransform>();
                return  rectTransform.localScale.x;
            }
        }

        public float FixOutScale
        {
            get
            {
                return Mathf.Max(FixHeightScale, FixWidthScale);
            }
        }

        public float FixInScale
        {
            get
            {
                return Mathf.Min(FixHeightScale, FixWidthScale);
            }
        }

        public float FixWidthScale
        {
            get
            {
                return Screen.width  / (design_width * CanvasScale);
            }
        }

        public float FixHeightScale
        {
            get
            {
                return Screen.height /(design_height * CanvasScale);
            }
        }
    }

}
