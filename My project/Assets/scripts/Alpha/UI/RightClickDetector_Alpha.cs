using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Alpha.UI
{
    public class RightClickDetector_Alpha : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> onRightClick;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                PassThrough(eventData, ExecuteEvents.pointerDownHandler);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                PassThrough(eventData, ExecuteEvents.pointerUpHandler);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"[RightClickDetector] Right click detected on {gameObject.name}");
                onRightClick?.Invoke(eventData);
            }
            else
            {
                PassThrough(eventData, ExecuteEvents.pointerClickHandler);
            }
        }

        private void PassThrough<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                ExecuteEvents.ExecuteHierarchy(parent.gameObject, eventData, function);
            }
        }

        /// <summary>
        /// 対象のGameObjectおよびそのすべての子孫Graphicに RightClickDetector を付与し、コールバックを設定する。
        /// </summary>
        public static void Attach(GameObject target, Action<PointerEventData> callback, bool includeChildren = true)
        {
            if (target == null) return;
            var detector = target.GetComponent<RightClickDetector_Alpha>();
            if (detector == null) detector = target.AddComponent<RightClickDetector_Alpha>();
            detector.onRightClick = callback;

            if (includeChildren)
            {
                var graphics = target.GetComponentsInChildren<Graphic>(true);
                foreach (var g in graphics)
                {
                    if (g.gameObject == target) continue;
                    var childDetector = g.GetComponent<RightClickDetector_Alpha>();
                    if (childDetector == null) childDetector = g.gameObject.AddComponent<RightClickDetector_Alpha>();
                    childDetector.onRightClick = callback;
                }
            }
        }
    }
}
