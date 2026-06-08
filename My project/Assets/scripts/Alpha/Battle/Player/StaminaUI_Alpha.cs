using UnityEngine;
using UnityEngine.UI;

public class StaminaUI_Alpha : MonoBehaviour
{
    [Header("References")]
    public playerStatusManager_Alpha statusManager;
    public Slider staminaSlider;

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
    }

    void Update()
    {
        if (statusManager != null && staminaSlider != null)
        {
            staminaSlider.maxValue = statusManager.maxStamina;
            staminaSlider.value = statusManager.currentStamina;
        }
    }
}
