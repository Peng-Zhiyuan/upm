using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crystal;
namespace ScreenFit
{
    public class UIScaleFit : MonoBehaviour
    {
        private int design_height = 2340;
        private int design_width = 1080;
        private int max_height = 2340;
        private int max_width = 1440;
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
            float maxDesignRatio = this.max_width / (float)this.design_height;
            float minDesignRatio = this.design_width / (float)this.max_height;
            //Debug.LogError("maxDesignRatio��" + maxDesignRatio+ "minDesignRatio��" + minDesignRatio+ "   ratio:"+ ratio);
            if (ratio < minDesignRatio)
            {
                this.transform.localScale = this.cacheScale * Screen.height/(this.max_height * CanvasScale);
                Debug.LogError(" this.transform.localScale��" + this.transform.localScale);
            }
            else if (ratio > maxDesignRatio)
            {
                this.transform.localScale = this.cacheScale * Screen.width / (this.max_width * CanvasScale);
                Debug.LogError(" this.transform.localScale��" + this.transform.localScale);
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
