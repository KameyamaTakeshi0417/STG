using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : HPBar_Base
{
    public GameObject playerObj;
    public delegate void HPChangedHandler();

    void OnEnable()
    {
        playerStatusManager_Alpha.OnPlayerHPChanged += HPUpdate;
    }

    void OnDisable()
    {
        playerStatusManager_Alpha.OnPlayerHPChanged -= HPUpdate;
    }

    // Start is called before the first frame update
    void Start()
    {
        hpSlider = HPBar.GetComponent<Slider>();
        
        // 蛻晄悄迥ｶ諷九・蜿肴丐・医・繝阪・繧ｸ繝｣繝ｼ縺九ｉ蜿門ｾ暦ｼ・
        GameObject managerObj = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
        if (managerObj != null)
        {
            var statusManager = managerObj.GetComponent<playerStatusManager_Alpha>();
            if (statusManager != null)
            {
                HPUpdate(statusManager.currentHP, statusManager.HP);
            }
        }
    }

    // Update is called once per frame
    void Update() { }

    private void HPUpdate(float current, float max)
    {
        HP = max;
        currentHP = current;
        
        if (hpSlider != null)
        {
            hpSlider.maxValue = HP;
            hpSlider.value = currentHP;
        }
    }

    public override void setSlideHPBar()
    {
        if (hpSlider != null)
        {
            if (hpSlider != null)
            {
                // HP繝舌・縺ｮ蛻晄悄險ｭ螳・
                hpSlider.maxValue = HP;
                hpSlider.value = (float)currentHP; // HP繝舌・縺ｮ譛蛻昴・蛟､繧堤樟蝨ｨ縺ｮHP縺ｫ險ｭ螳・
            }
        }
        else
        {
            Debug.LogWarning("Canvas or HPBar not found in the enemy object.");
        }
    }
}
