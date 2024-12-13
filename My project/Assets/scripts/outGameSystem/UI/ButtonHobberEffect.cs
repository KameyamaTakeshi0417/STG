using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverDistance = 20f; // ボタンが動く距離
    public Color glowColor = new Color(1f, 1f, 0.5f, 1f); // 光らせる色
    public float glowSpeed = 2f; // 光の強さが変わるスピード

    private Vector3 originalPosition; // ボタンの元の位置
    private Image buttonImage; // ボタンのImageコンポーネント
    private Color originalColor; // 元の色
    private bool isHovering = false; // マウスが乗っているかどうか

    void Start()
    {
        // 元の位置を保存
        originalPosition = transform.localPosition;

        // ボタンのImageコンポーネントを取得
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
    }

    void Update()
    {
        if (isHovering && buttonImage != null)
        {
            // 光らせる処理（サイン波で明滅させる）
            float glowFactor = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f; // 0〜1の範囲で変動
            buttonImage.color = Color.Lerp(originalColor, glowColor, glowFactor);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ボタンを右方向に移動
        transform.localPosition = originalPosition + new Vector3(hoverDistance, 0, 0);

        // マウスカーソルが乗っているフラグを立てる
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 元の位置に戻す
        transform.localPosition = originalPosition;

        // 色を元に戻す
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }

        // マウスカーソルが離れたフラグを下ろす
        isHovering = false;
    }
}
