using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class staminaManager : MonoBehaviour
{
    public GameObject staminaGauge;
    public GameObject playerManagerObj;
    public playerStatusManager_Alpha playerStatusManager;

    

        Image targetGauge;
    // Start is called before the first frame update
    void Awake()
    {
        targetGauge=staminaGauge.GetComponent<Image>();
        playerStatusManager = playerStatusManager_Alpha.Instance;
        targetGauge.fillAmount = 1;
        if (playerStatusManager != null)
        {
            UpdateFill();
        }
    }
    private void Update()
    {
        if (playerStatusManager == null)
            playerStatusManager = playerStatusManager_Alpha.Instance;

        if (playerStatusManager != null)
        {
            UpdateFill();
        }
    }
    // Update is called once per frame
    void UpdateFill()
    {
        float max = playerStatusManager.maxStamina;
        float cur = playerStatusManager.currentStamina;

        float ratio = cur / max;
        targetGauge.fillAmount = ratio;

    }
}
