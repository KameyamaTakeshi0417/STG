using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Health_Base : MonoBehaviour
{
    public float HP = 100f; // 初期HP

    public float currentHP;
    public int Exp;
    protected int moneyCount;
    protected Slider hpSlider; //HPバー（スライダー）
    protected HPBar_Base m_handler;
    public float VulnerableTime = 0f;
    public bool VulnerableFlg = false;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (VulnerableFlg)
        {
            VulnerableTime -= 0.1f;
            if (VulnerableTime <= 0f)
            {
                VulnerableFlg = false;
            }
        }
    }

    public void SliderUpdate()
    {
        hpSlider.value = currentHP; //スライダは０〜1.0で表現するため最大HPで割って少数点数字に変換
    }

    public virtual void TakeDamage(float damage) { }

    public void setExp(int exp)
    {
        Exp = exp;
    }

    public float getHP()
    {
        return HP;
    }

    public void setHP(float hp)
    {
        HP = hp;
        return;
    }

    public float getCurrentHP()
    {
        return currentHP;
    }

    public void setCurrentHP(float set)
    {
        currentHP = set;
    }

    public void addHP(float hp)
    {
        HP += hp;
        return;
    }

    public void AddCurrentHP(float set)
    {
        float ret = set;
        if (currentHP + set > HP)
            set = HP;
        currentHP += set;
    }
}
