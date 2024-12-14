using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : HPBar_Base
{
    public GameObject playerObj;
    public delegate void HPChangedHandler();
    public static event HPChangedHandler OnPlayerHPChanged;

    void OnEnable()
    {
        PlayerHealth.OnPlayerHPChanged += HPUpdate;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerHPChanged -= HPUpdate;
    }

    // Start is called before the first frame update
    void Start()
    {
        hpSlider = HPBar.GetComponent<Slider>();
        HPUpdate();
    }

    // Update is called once per frame
    void Update() { }

    private void HPUpdate()
    {
        HP = playerObj.GetComponent<PlayerHealth>().HP;
        currentHP = playerObj.GetComponent<PlayerHealth>().currentHP;
        // HPバーの初期設定
        hpSlider.maxValue = HP;
        hpSlider.value = (float)currentHP; // HPバーの最初の値を現在のHPに設定
        // HPバー(Slider)を取得
        if (hpSlider != null)
        {
            setSlideHPBar();
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
