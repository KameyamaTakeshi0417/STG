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
        
        // 初期状態の反映（マネージャーから取得）
        GameObject managerObj = GameObject.Find("manager");
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
                // HPバーの初期設定
                hpSlider.maxValue = HP;
                hpSlider.value = (float)currentHP; // HPバーの最初の値を現在のHPに設定
            }
        }
        else
        {
            Debug.LogWarning("Canvas or HPBar not found in the enemy object.");
        }
    }
}
