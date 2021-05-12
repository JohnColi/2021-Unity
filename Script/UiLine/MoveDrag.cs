using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveDrag : MonoBehaviour, IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    public Action pointDownEvent;
    public Action pointUpEvent;

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointDownEvent?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointUpEvent?.Invoke();
    }
}
