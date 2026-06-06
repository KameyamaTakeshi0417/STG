using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : _Health_Base
{
    public bool isInvincible = false;
    
    // マネージャーの参照をキャッシュ
    private playerStatusManager_Alpha statusManager;
    public GameObject gaugeCanvas;
    public CircleHPBarManager circleHPBarManager;

    void Start()
    {
        // Managerオブジェクトからステータスマネージャーを取得する
        GameObject managerObj = GameObject.Find("manager");
        if (managerObj != null)
        {
            statusManager = managerObj.GetComponent<playerStatusManager_Alpha>();
        }
        else
        {
            Debug.LogError("[PlayerHealth] 'manager' GameObject not found in the scene!");
        }

        // 初期化時にマネージャー側のHPでローカル変数を同期しておく
        if (statusManager != null)
        {
            // まず最新の装備バフを確実に計算させる（実行順序のズレ防止）
            statusManager.UpdateEquipmentBuffs();

            HP = statusManager.HP;
            currentHP = statusManager.currentHP;

            // 初期化時にマネージャー側でバリアバフが掛かっていれば適用する
            if (statusManager.hasBarrierBuff)
            {
                Debug.Log($"[PlayerHealth] Start: statusManager has barrier buff. Setting isBarrierActive to true.");
                isBarrierActive = true;
                barrierEndurableDamage = statusManager.barrierEndurableDamage;
                barrierBaseRespawnTime = statusManager.barrierRespawnTime;
                
                if (barrierVisualObject != null)
                {
                    barrierVisualObject.SetActive(true);
                }
            }
            else
            {
                Debug.Log($"[PlayerHealth] Start: statusManager DOES NOT have barrier buff. Setting isBarrierActive to false.");
                isBarrierActive = false;
                barrierEndurableDamage = 0f;
                
                if (barrierVisualObject != null)
                {
                    barrierVisualObject.SetActive(false);
                }
            }
        }
        circleHPBarManager = gaugeCanvas.GetComponent<CircleHPBarManager>();
        circleHPBarManager.SetCircleBar(statusManager.HPGauge);
    }

    protected override void Update()
    {
        base.Update(); // _Health_Base側の無敵時間などのカウントダウン処理を実行

        // マネージャー側の実HPと常に同期させておく
        // （他スクリプトが_Health_Baseとして現在のHPを参照した場合の齟齬防止）
        if (statusManager != null)
        {
            HP = statusManager.HP;
            currentHP = statusManager.currentHP;
            
            // デバッグ機能: F3キーで現在HP分のダメージを受ける
            if (Input.GetKeyDown(KeyCode.F3))
            {
                TakeDamage(statusManager.currentHP);
            }
        }
    }
    public void gaugeUpdate() {
        circleHPBarManager.UpdateCircleBar(statusManager.nowHPGauge, (statusManager.currentHP / statusManager.HP));
    }

    // ボム発動時などの一定時間無敵（当たり判定消失）処理
    public void MakeInvincibleWithColliders(float duration)
    {
        StartCoroutine(InvincibleWithCollidersRoutine(duration));
    }

    private IEnumerator InvincibleWithCollidersRoutine(float duration)
    {
        isInvincible = true;
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        List<Collider2D> activeColliders = new List<Collider2D>();
        
        foreach (var col in colliders)
        {
            if (col != null && col.enabled)
            {
                activeColliders.Add(col);
                col.enabled = false;
            }
        }

        yield return new WaitForSeconds(duration);

        foreach (var col in activeColliders)
        {
            if (col != null) col.enabled = true;
        }
        isInvincible = false;
    }

    public override void TakeDamage(float damage)
    {
        if (isInvincible) return; // 無敵中ならダメージを無視する

        float setDmg = damage;
        // 弱点倍率などローカル（自身）の状態に基づく計算
        if (VulnerableFlg)
        {
            setDmg *= 1.5f;
        }
        
        // 実際のダメージ適用はすべて一元管理しているマネージャーへ委譲
        if (statusManager != null)
        {
            statusManager.ApplyDamage(setDmg);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] StatusManager is missing. Cannot apply damage.");
        }
        gaugeUpdate();
    }
}
