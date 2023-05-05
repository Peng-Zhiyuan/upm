using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;


public class SmoothProgressBar : MonoBehaviour
{
    [HideInInspector] [Range(0, 1)] public float targetValue;

    public void ResetProgress()
    {
        this.DisplayValue = 0;
    }

    public float DisplayValue
    {
        get { return this.progressBar.value; }
        set
        {
            if (this.progressBar == null)
            {
                return;
            }

            this.progressBar.value = value;
        }
    }

    private bool _openTween = true;

    public bool OpenTween
    {
        get => this._openTween;
        set
        {
            if (this._openTween.Equals(value)) return;

            this._openTween = value;
        }
    }

    Slider progressBar;

    private void OnEnable()
    {
        this.progressBar = this.GetComponent<Slider>();
        this.DisplayValue = 0f;
    }

    void Update()
    {
        if (this._openTween)
        {
            if(this.DisplayValue >= this.targetValue)
            {
                return;
            }

            var vector = this.targetValue - this.DisplayValue;
            var distance = Mathf.Abs(vector);
            var step = Time.deltaTime * 1;
            var delta = Mathf.Min(distance, step);
            var finalVector = SetVectorMode(vector, delta);
            this.DisplayValue += finalVector;
        }
        else
        {
            this.DisplayValue = this.targetValue;
        }
    }

    /**
     * 设置一纬向量的模
     * @param origin 向量
     * @param mode 模
     */
    static float SetVectorMode(float origin, float mode)
    {
        if (origin > 0)
        {
            return mode;
        }
        else if (origin < 0)
        {
            return -mode;
        }

        return 0;
    }

    public async Task WaitDisplayFullAsync()
    {
        while (this.DisplayValue < 1)
        {
            await Task.Delay(1);
        }
    }
}