using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Health_Base : MonoBehaviour
{
    public float HP = 100f; // 初期HP

    public float currentHP;
    protected int Exp;
    protected int moneyCount;
    protected Slider hpSlider; //HPバー（スライダー）
    protected HPBar_Base m_handler;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

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
        currentHP += set;
    }
}
