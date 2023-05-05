using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraFollowController
{
    public SceneObject _owner = null;

    private float _speed = 100f;
    private Vector3 _target = Vector3.zero;

    private bool _bUpdate = false;
    // Use this for initialization
    public void Init(SceneObject owner)
    {
        _owner = owner;
        _owner.objectEvent.AddListener(GameEvent.SyncPos, this, SyncPos);
    }

    public void Update(float param_deltaTime)
    {
        if (!_bUpdate)
            return;
        
        _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, _target, CameraManager.Instance.MoveSpeed * param_deltaTime);
    }

    public void MoveTo(Vector3 target, float time = 2f)
    {
        _bUpdate = false;
        _owner.transform.DOMove(target, time).SetEase(Ease.Linear).OnComplete(() =>
        {
            _target = target;
            _bUpdate = true;
        });
    }

    public void MoveImmediate(Vector3 target)
    {
        _target = target;
        _owner.transform.position = _target;
    }

    private void SyncPos(object[] eventData)
    {
        _target = (Vector3)eventData[0];
    }

    public void OnDestroy()
    {
        _owner.objectEvent.RemoveListener(GameEvent.SyncPos, this, SyncPos);
    }
}