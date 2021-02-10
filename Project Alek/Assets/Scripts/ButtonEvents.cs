using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonEvents : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onSelect;
    public UnityEvent onDeselect;
    public UnityEvent onPointerEnter;
    public UnityEvent onPointerExit;
    
    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        onDeselect?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit?.Invoke();
    }
}