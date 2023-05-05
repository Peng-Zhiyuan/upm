using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class CircuitNaFlags : MonoBehaviour
{
    public Image naFlagPrefab;
    public Image overlayFlagPrefab;

    private Queue<Image> _naPool;
    private Queue<Image> _overlayPool;
    private Queue<Image> _naUsingPool;
    private Queue<Image> _overlayUsingPool;

    public void Clear()
    {
        if (null != _naUsingPool)
        {
            while (_naUsingPool.Count > 0)
            {
                var img = _naUsingPool.Dequeue();
                img.SetActive(false);
                _naPool.Enqueue(img);
            }
        }
        
        if (null != _overlayUsingPool)
        {
            while (_overlayUsingPool.Count > 0)
            {
                var img = _overlayUsingPool.Dequeue();
                img.SetActive(false);
                _overlayPool.Enqueue(img);
            }
        }
    }

    public void SetOverlay(int x, int y)
    {
        var img = _TakeOverlayBlock();
        _RecordOverlay(img);
        _Put(img, x, y);
    }

    public void SetNa(int x, int y)
    {
        var img = _TakeNaBlock();
        _RecordNa(img);
        _Put(img, x, y);
    }

    private void _Put(Image img, int x, int y)
    {
        img.SetAnchoredPosition(new Vector2(x * CircuitCellExt.UnitLength, y * CircuitCellExt.UnitLength));
        img.SetActive(true);
    }

    private Image _TakeOverlayBlock()
    {
        _overlayPool ??= new Queue<Image>();
        if (_overlayPool.Count > 0)
        {
            return _overlayPool.Dequeue();
        }

        return Instantiate(overlayFlagPrefab, transform);
    }

    private Image _TakeNaBlock()
    {
        _naPool ??= new Queue<Image>();
        if (_naPool.Count > 0)
        {
            return _naPool.Dequeue();
        }

        return Instantiate(naFlagPrefab, transform);
    }

    private void _RecordOverlay(Image img)
    {
        _overlayUsingPool ??= new Queue<Image>();
        _overlayUsingPool.Enqueue(img);
    }

    private void _RecordNa(Image img)
    {
        _naUsingPool ??= new Queue<Image>();
        _naUsingPool.Enqueue(img);
    }
    
    private void Awake()
    {
        // 默认要隐藏它们
        var sceneNa = naFlagPrefab.gameObject.scene;
        if (!string.IsNullOrEmpty(sceneNa.name))
        {
            naFlagPrefab.SetActive(false);
            _naPool = new Queue<Image>();
            _naPool.Enqueue(naFlagPrefab);
        }
        var sceneOverlay = overlayFlagPrefab.gameObject.scene;
        if (!string.IsNullOrEmpty(sceneOverlay.name))
        {
            overlayFlagPrefab.SetActive(false);
            _overlayPool = new Queue<Image>();
            _overlayPool.Enqueue(overlayFlagPrefab);
        }
    }
}