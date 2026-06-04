using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

namespace Alpha.UI
{
    public class InventorySlotDragHandler_Alpha : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public int slotIndex;
        public Action<int, int> onSlotDropped; // fromIndex, toIndex

        private static GameObject draggedVisual;
        private static int dragSourceIndex = -1;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            dragSourceIndex = slotIndex;

            // "Icon"という名前の子オブジェクトから画像をコピーしてドラッグ中の見た目を作成
            Transform iconTrans = transform.Find("Icon");
            if (iconTrans != null)
            {
                Image iconImg = iconTrans.GetComponent<Image>();
                if (iconImg != null && iconImg.sprite != null && iconImg.color.a > 0)
                {
                    draggedVisual = new GameObject("DraggedIcon");
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        draggedVisual.transform.SetParent(canvas.transform, false);
                        draggedVisual.transform.SetAsLastSibling(); // 最前面へ
                    }

                    Image newImg = draggedVisual.AddComponent<Image>();
                    newImg.sprite = iconImg.sprite;
                    newImg.color = iconImg.color;
                    newImg.raycastTarget = false; // ドロップの判定を邪魔しないようにする

                    RectTransform rect = draggedVisual.GetComponent<RectTransform>();
                    rect.sizeDelta = iconTrans.GetComponent<RectTransform>().sizeDelta;
                    rect.position = eventData.position;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggedVisual != null && eventData.button == PointerEventData.InputButton.Left)
            {
                draggedVisual.GetComponent<RectTransform>().position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggedVisual != null)
            {
                Destroy(draggedVisual);
                draggedVisual = null;
            }
            dragSourceIndex = -1;
        }

        public void OnDrop(PointerEventData eventData)
        {
            // 同じスロットでなければ入れ替え処理を発火
            if (dragSourceIndex != -1 && dragSourceIndex != slotIndex)
            {
                onSlotDropped?.Invoke(dragSourceIndex, slotIndex);
            }
        }
    }
}
