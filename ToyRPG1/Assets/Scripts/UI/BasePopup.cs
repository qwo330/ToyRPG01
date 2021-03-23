using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public abstract class BasePopup : MonoBehaviour
{
    public Action _ShowAction;
    public Action _HideAction;

    [SerializeField]
    RectTransform _ShowRect, _HideRect;
    Vector3 _showPosition, _hidePosition;

    void Start()
    {
        _showPosition = _ShowRect.position;    
        _hidePosition = _HideRect.position;    
    }

    public virtual void Show()
    {
        _ShowAction?.Invoke();
        gameObject.SetActive(true);
        transform.DOMove(_showPosition, 1f);
    }

    public virtual void Hide()
    {
        transform.DOMove(_hidePosition, 1f);
        gameObject.SetActive(false);
        _HideAction?.Invoke();
    }
}
