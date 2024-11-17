using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : HPBar_Base
{
    public GameObject playerObj;
    void OnEnable()
    {
        Health.OnHPChanged += setSlideHPBar;
    }

    void OnDisable()
    {
        Health.OnHPChanged -= setSlideHPBar;
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
        HP = playerObj.GetComponent<Health>().HP;
        currentHP = playerObj.GetComponent<Health>().currentHP;
        // HPバーの初期設定
        hpSlider.maxValue = HP;
        hpSlider.value = (float)currentHP; // HPバーの最初の値を現在のHPに設定
    }

    public override void setSlideHPBar()
    {
        HPUpdate();
        // HPバー(Slider)を取得
        hpSlider = HPBar.transform.Find("PlayerHPBar").GetComponent<Slider>();

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
