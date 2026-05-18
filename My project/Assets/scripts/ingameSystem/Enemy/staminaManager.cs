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
        playerStatusManager=playerManagerObj.GetComponent<playerStatusManager_Alpha>();
        targetGauge.fillAmount = 1;
       UpdateFill();
    }
    private void Update()
    {
        UpdateFill();
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
