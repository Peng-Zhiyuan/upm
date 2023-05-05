using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class TransportItem: MonoBehaviour
{
    public static bool Pause;
    public static Vector2 AimLeft;
    public static Vector2 AimRight;
    public static int Speed = 500;
    private static int Diff = 100;
    private static readonly Vector2[] Points =
    {
        new Vector2(0, -220),
        new Vector2(-290, -220),
        new Vector2(-290, -410),
        new Vector2(300, -410),
        new Vector2(300, -600),
        new Vector2(0, -600),
        new Vector2(0, -1460),
    };
    private static readonly Vector2 AimPoint = new Vector2(0, -1134);
    private static readonly Vector2 DirLeft = new Vector2(-1, 0);
    private static readonly Vector2 DirRight = new Vector2(1, 0);

    public Action<TransportItem> OnEnd { get; set; }
    public Action<TransportItem> OnDest { get; set; }
    public float Passed { get; private set; }
    public string Icon { get; private set; }
    public bool IsBomb { get; private set; }
    public TransportDirection DestDir { get; private set; }
    public TransportItem Next { get; set; }
    public Image Image => GetComponent<Image>();
    public bool ReachTarget => Vector2.Distance(_pos, AimPoint) <= Diff;
    public bool Surpass => _pos.y <= AimPoint.y;  // 已经超过了目标点
    public bool Vanished => _vanished;

    private int _pointIndex;
    private Vector2 _nextPoint;
    private Vector2 _prevPoint;
    private Vector2 _direction;
    private Vector2 _pos;
    private Vector2 _dest;
    private bool _gaming;
    private bool _vanished;
    private float _prevPassed;
    private Sprite _bombSprite;

    public void SetStart(float offLength = 0)
    {
        _Reset();
        
        _pos = _prevPoint + _direction * offLength;
        this.GetComponent<RectTransform>().anchoredPosition = _pos;
        Passed = _prevPassed + offLength;
    }

    public void MoveTo(TransportDirection dir)
    {
        DestDir = dir;
        _direction = dir == TransportDirection.Left ? DirLeft : DirRight;
        _dest = dir == TransportDirection.Left ? AimLeft : AimRight;
    }

    public async void Explode()
    {
        _gaming = false;
        OnEnd?.Invoke(this);
        
        // 爆炸效果
        var bucket = BucketManager.Stuff.Main;
        var effect = await bucket.GetOrAquireAsync<GameObject>("fx_ui_cobattle_break.prefab");
        var effectInstance = bucket.Pool.Reuse<RecycledGameObject>(effect);
        effectInstance.transform.SetParent(transform.parent);
        effectInstance.SetPosition(transform.position);
        // 到时间就销毁
        await Task.Delay(1000);
        effectInstance.Recycle();
    }

    public void SetIcon(string icon, bool isBomb)
    {
        Icon = icon;
        IsBomb = isBomb;
        if (isBomb)
        {
            if (null != _bombSprite)
            {
                Image.sprite = _bombSprite;
            }
        }
        else
        {
            if (null == _bombSprite)
            {
                _bombSprite = Image.sprite;
            }
            UiUtil.SetSpriteInBackground(Image, () => $"{icon}.png");
        }
        UiUtil.SetAlpha(Image, 1);
    }

    public void Vanish(float duration = .2f, Action onComplete = null)
    {
        _vanished = true;

        Image.DOFade(0, duration).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    private void Update()
    {
        if (Pause) return;
        if (!_gaming) return;

        _pos += _direction * Speed * Time.deltaTime;
        var walkedOn = Vector2.Distance(_pos, _prevPoint);
        Passed = _prevPassed + walkedOn;
        // Debug.Log($"Passed: 当前已经行走了{Passed}");
        if (DestDir != TransportDirection.None)
        {
            if (!_SameDirection(_dest - _pos, _direction))
            {
                _gaming = false;
                this.GetComponent<RectTransform>().anchoredPosition = _dest;
                OnDest?.Invoke(this);
            }
            else
            {
                _pos.y = Mathf.Lerp(_pos.y, _dest.y, .1f);
            }
        }
        else if (!_SameDirection(_nextPoint - _pos, _direction))
        {
            ++_pointIndex;
            if (_pointIndex >= Points.Length)
            {
                _gaming = false;
                this.GetComponent<RectTransform>().anchoredPosition = Points[Points.Length - 1];
                OnEnd?.Invoke(this);
                return;
            }

            _SetNewStage();
            // 把超出的内容加到下一段中，以达到精确计算
            var over = _pos - _prevPoint;
            _pos = _prevPoint + _direction * over.magnitude;
        }

        this.GetComponent<RectTransform>().anchoredPosition = _pos;
    }

    private void _Reset()
    {
        _gaming = true;
        _vanished = false;
        _pointIndex = 0;
        _nextPoint = Vector2.zero;
        DestDir = TransportDirection.None;
        Passed = 0;
        _SetNewStage();
    }

    private void _SetNewStage()
    {
        if (_nextPoint != Vector2.zero)
        {
            _prevPassed += Vector2.Distance(_prevPoint, _nextPoint);
        }
        else
        {
            _prevPassed = 0;
        }
        
        _prevPoint = _nextPoint;
        _nextPoint = Points[_pointIndex];
        _direction = (_nextPoint - _prevPoint).normalized;
    }

    private bool _SameDirection(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.x >= 0 && v1.y * v2.y >= 0;
    }
}


public enum TransportDirection
{
    None = 0,
    Left,
    Right
}