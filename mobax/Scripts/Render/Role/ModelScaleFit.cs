using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ScreenFit
{
    public class ModelScaleFit : MonoBehaviour
    {
        private RectTransform rectTransform = null;
        private Vector3 cacheScale;
        public float adjustScale = 1f;
        public void Awake()
        {
            this.cacheScale = this.transform.localScale;
            FitSize();
            UIEngine.Stuff.CanvasSizeChnaged += FitSize;
        }

        void OnDestroy()
        {
            UIEngine.Stuff.CanvasSizeChnaged -= FitSize;
        }


        public float CanvasScale
        {
            get
            {
                if (rectTransform == null) rectTransform = UIEngine.Stuff.Canvas.GetComponent<RectTransform>();
                return rectTransform.localScale.x;
            }
        }

        [Sirenix.OdinInspector.ShowInInspector]
        void FitSize()
        {
            var scaler = UIEngine.Stuff.Canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
   
                Debug.LogError("scaler is null");
                return;
            }
      /*      if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                Debug.LogError("未适配类型 uiScaleMode：" + scaler.uiScaleMode);
                return;
            }

            if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand)
            {
                Debug.LogError("未适配类型 screenMatchMode：" + scaler.screenMatchMode);
                return;
            }*/
            this.transform.localScale =  this.adjustScale * this.cacheScale/ CanvasScale;

        }
    }

}