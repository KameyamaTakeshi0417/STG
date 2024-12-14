using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar_Base : MonoBehaviour
{
    public float HP = 100f; // 初期HP

    public float currentHP;
    public GameObject HPBar;
    protected Slider hpSlider; //HPバー（スライダー）
    public _Health_Base objectHealth;

    // Start is called before the first frame update
    void Start()
    {
        objectHealth = gameObject.GetComponent<_Health_Base>();
        currentHP = objectHealth.currentHP;
        HP = objectHealth.HP;
    }

    // Update is called once per frame
    void Update()
    {
        currentHP = objectHealth.currentHP;
        HP = objectHealth.HP;
    }

    public virtual void setSlideHPBar()
    {
        // HPバー(Slider)を取得
        hpSlider = HPBar.transform.Find("HPBar").GetComponent<Slider>();

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

    public void SliderUpdate()
    {
        hpSlider.value = currentHP; //スライダは０〜1.0で表現するため最大HPで割って少数点数字に変換
    }
}
