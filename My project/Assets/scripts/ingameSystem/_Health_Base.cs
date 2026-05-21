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

    [Header("Pierce Settings")]
    [Tooltip("このエネミーに対して許容される貫通の最大回数")]
    public int PierceVolume = 1;

    [Header("Status Effects")]
    [Tooltip("感電の蓄積値。0より大きい場合帯電状態")]
    public int VoltCount = 0;

    [Header("Stun Settings")]
    [Tooltip("現在のスタン耐性値（この値以上のスタン秒数でないとスタンしない）")]
    public float StunResistance = 0f;
    [Tooltip("スタンを受けた際に増加するスタン耐性値")]
    public float BaseStunResistance = 0.5f;
    
    [HideInInspector] public bool isStunned = false;
    protected float currentStunTime = 0f;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (VulnerableFlg)
        {
            VulnerableTime -= Time.deltaTime; // 0.1f から Time.deltaTime に修正
            if (VulnerableTime <= 0f)
            {
                VulnerableFlg = false;
            }
        }

        if (isStunned)
        {
            currentStunTime -= Time.deltaTime;
            if (currentStunTime <= 0f)
            {
                isStunned = false;
            }
        }
    }

    /// <summary>
    /// スタンを付与する。耐性値によって軽減され、0以下になれば無効化される。
    /// 一度スタンを受けると、耐性値が加算される。
    /// </summary>
    public virtual void ApplyStun(float stunDuration)
    {
        float effectiveStun = stunDuration - StunResistance;
        
        if (effectiveStun > 0f)
        {
            isStunned = true;
            // すでにスタン中で、より長いスタンを受けた場合は上書き
            if (currentStunTime < effectiveStun)
            {
                currentStunTime = effectiveStun;
            }
            
            // スタン耐性を上昇させる
            StunResistance += BaseStunResistance;
            Debug.Log($"[{gameObject.name}] Stunned for {effectiveStun}s. Next Resistance: {StunResistance}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Resisted Stun! (Resistance: {StunResistance} >= Duration: {stunDuration})");
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
