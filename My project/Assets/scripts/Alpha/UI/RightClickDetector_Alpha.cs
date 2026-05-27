using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Alpha.UI
{
    public class RightClickDetector_Alpha : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> onRightClick;

        public void OnPointerDown(PointerEventData eventData) { }
        public void OnPointerUp(PointerEventData eventData) { }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"[RightClickDetector] Right click detected on {gameObject.name}");
                onRightClick?.Invoke(eventData);
            }
        }
    }
}
