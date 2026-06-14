using UnityEngine;
using UnityEngine.UI;

public class StaminaUI_Alpha : MonoBehaviour
{
    [Header("References")]
    public playerStatusManager_Alpha statusManager;
    public Slider staminaSlider;
    
    [Header("Color Settings")]
    public Image fillImage;
    public Color normalColor = new Color(0f, 1f, 0.5f, 1f); // 少し青みがかった緑
    public Color exhaustedColor = new Color(1f, 0.2f, 0.2f, 1f); // 赤

    void Start()
    {
        if (statusManager == null)
        {
            GameObject managerObj = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
            if (managerObj != null)
            {
                statusManager = managerObj.GetComponent<playerStatusManager_Alpha>();
            }
        }

        // FillImageの自動取得
        if (fillImage == null && staminaSlider != null)
        {
            Transform fillRect = staminaSlider.fillRect;
            if (fillRect != null)
            {
                fillImage = fillRect.GetComponent<Image>();
                if (fillImage != null) normalColor = fillImage.color; // 元の色を記憶
            }
        }
    }

    void Update()
    {
        if (statusManager != null && staminaSlider != null)
        {
            staminaSlider.maxValue = statusManager.maxStamina;
            staminaSlider.value = statusManager.currentStamina;

            if (fillImage != null)
            {
                fillImage.color = statusManager.isStaminaExhausted ? exhaustedColor : normalColor;
            }
        }
    }
}
