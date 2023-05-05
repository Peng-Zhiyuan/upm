using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crystal;
namespace ScreenFit
{

    public class Spine3DScaleFit : MonoBehaviour
    {
        private int design_height = 2340;
        private int design_width = 1080;
        private RectTransform rectTransform = null;
        private Vector3 cacheScale;
        private RectTransform safeArea;

        public void Awake()
        {
            this.cacheScale = this.transform.localScale;
            this.FitSize();
            UIEngine.Stuff.CanvasSizeChnaged += FitSize;

        }
        void OnDestroy()
        {
            UIEngine.Stuff.CanvasSizeChnaged -= FitSize;
        }
        public void FitSize()
        {
            float ratio = Screen.width / (float)Screen.height;
            float designRatio = this.design_width / (float)this.design_height;
            if(ratio > designRatio)
            {
                this.transform.localScale = this.cacheScale * (Screen.width * (float)this.design_height) / (this.design_width * Screen.height);
            }
            else this.transform.localScale = this.cacheScale;
        }

        public float CanvasScale
        {
            get
            {
                if (rectTransform == null) rectTransform = UIEngine.Stuff.Canvas.GetComponent<RectTransform>();
                return  rectTransform.localScale.x;
            }
        }
     
    }

}
