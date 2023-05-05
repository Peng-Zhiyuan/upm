using UnityEngine;
using UnityEngine.UI;

public class RawImageUV : MonoBehaviour {
    public RawImage target;

    public float SpeedX = 0f;
    public float SpeedY = 0f;

    public float UV_W = 0.0f;
    public float UV_H = 0.0f;

    private float ox;
    private float oy;
    private float wc;
    private float hc;

    private void Start () {
        if (target == null) {
            target = GetComponent<RawImage> ();
        }
        if (UV_W == 0.0f) {
            wc = target.rectTransform.rect.width / target.mainTexture.width;
        } else {
            wc = UV_W;
        }
        if (UV_H == 0.0f) {
            hc = target.rectTransform.rect.height / target.mainTexture.height;
        } else {
            hc = UV_H;
        }
    }

    void Update () {
        if (target != null) {
            ox += Time.deltaTime * SpeedX;
            oy += Time.deltaTime * SpeedY;
            ox = ox % 1;
            oy = oy % 1;
            target.GetComponent<RawImage> ().uvRect = new Rect (ox, oy, wc, hc);
        }
    }
}