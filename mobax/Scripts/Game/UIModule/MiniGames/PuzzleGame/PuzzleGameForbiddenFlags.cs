using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class PuzzleGameForbiddenFlags : MonoBehaviour
{
    public Image forbiddenPrefab;

    private Queue<Image> _forbiddenPool;
    private Queue<Image> _forbiddenUsingPool;

    public void Clear()
    {
        if (null != _forbiddenUsingPool)
        {
            while (_forbiddenUsingPool.Count > 0)
            {
                var img = _forbiddenUsingPool.Dequeue();
                img.SetActive(false);
                _forbiddenPool.Enqueue(img);
            }
        }
    }

    public void SetForbidden(int x, int y)
    {
        var img = _TakeForbiddenBlock();
        _RecordForbidden(img);
        _Put(img, x, y);
    }

    private void _Put(Image img, int x, int y)
    {
        img.SetAnchoredPosition(new Vector2((x + .5f) * PuzzleBrickCell.DefaultWidth, (y + .5f) * PuzzleBrickCell.DefaultHeight));
        img.SetActive(true);
    }

    private Image _TakeForbiddenBlock()
    {
        _forbiddenPool ??= new Queue<Image>();
        if (_forbiddenPool.Count > 0)
        {
            return _forbiddenPool.Dequeue();
        }

        return Instantiate(forbiddenPrefab, transform);
    }

    private void _RecordForbidden(Image img)
    {
        _forbiddenUsingPool ??= new Queue<Image>();
        _forbiddenUsingPool.Enqueue(img);
    }
    
    private void Awake()
    {
        // 默认要隐藏它们
        var scene = forbiddenPrefab.gameObject.scene;
        _forbiddenPool = new Queue<Image>();
        if (!string.IsNullOrEmpty(scene.name))
        {
            forbiddenPrefab.SetActive(false);
            _forbiddenPool.Enqueue(forbiddenPrefab);
        }
    }
}