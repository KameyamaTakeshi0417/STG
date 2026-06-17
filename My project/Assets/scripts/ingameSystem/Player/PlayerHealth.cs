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

    // ボム発動時などの一定時間無敵�E�当たり判定消失�E��E琁E
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
        if (isInvincible) return; // 無敵中ならダメージを無視すめE

        float setDmg = damage;
        // 弱点倍率などローカル�E��E身�E��E状態に基づく計箁E
        if (VulnerableFlg)
        {
            setDmg *= 1.5f;
        }
        
        // 実際のダメージ適用はすべて一允E��琁E��てぁE��マネージャーへ委譲
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
