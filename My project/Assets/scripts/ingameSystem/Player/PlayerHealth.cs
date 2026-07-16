using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : _Health_Base
{
    public bool isInvincible = false;
    
    // マネージャーの参�EをキャチE��ュ
    private playerStatusManager_Alpha statusManager;
    private int cachedMaxHPGauge = -1;
    public GameObject gaugeCanvas;
    public CircleHPBarManager circleHPBarManager;

    void Start()
    {
        // ManagerオブジェクトからスチE�Eタスマネージャーを取得すめE
        GameObject managerObj = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
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
            // まず最新の裁E��バフを確実に計算させる�E�実行頁E���Eズレ防止�E�E
            statusManager.UpdateEquipmentBuffs();

            HP = statusManager.HP;
            currentHP = statusManager.currentHP;

            // 初期化時にマネージャー側でバリアバフが掛かってぁE��ば適用する
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
        base.Update(); 

        if (statusManager != null)
        {
            HP = statusManager.HP;
            currentHP = statusManager.currentHP;
            
            if (cachedMaxHPGauge != statusManager.HPGauge)
            {
                cachedMaxHPGauge = statusManager.HPGauge;
                if (circleHPBarManager != null) circleHPBarManager.SetCircleBar(cachedMaxHPGauge);
                gaugeUpdate();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                TakeDamage(statusManager.currentHP);
            }
        }
    }
    public void gaugeUpdate() {
        circleHPBarManager.UpdateCircleBar(statusManager.nowHPGauge, (statusManager.currentHP / statusManager.HP));
    }

    // ボム発動時などの一定時間無敵E当たり判定消失EE琁E
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
        // 弱点倍率などローカル側の状態に基づく計算
        if (VulnerableFlg)
        {
            setDmg *= 1.5f;
        }
        
        // 被弾時演出（Juice）
        if (damage > 0 && Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
        {
            Alpha.Core.ProceduralJuiceManager_Alpha.Instance.TriggerPlayerDamageJuice();
        }

        // 実際のダメージ適用はすべて一元化してマネージャーへ委譲
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

    // コライダーを無効にせず、点滅演出付きで無敵時間を付与する
    public void MakeInvincible(float duration)
    {
        StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        isInvincible = true;
        
        // 簡単な点滅演出
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float blinkInterval = 0.1f;
        float elapsed = 0f;
        bool isVisible = true;
        
        while (elapsed < duration)
        {
            isVisible = !isVisible;
            foreach(var sr in renderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = isVisible ? 1f : 0.2f;
                    sr.color = c;
                }
            }
            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        foreach(var sr in renderers)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }
        
        isInvincible = false;
    }
}
