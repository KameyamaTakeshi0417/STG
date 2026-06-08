using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : _Health_Base
{
    public bool isInvincible = false;
    
    // 繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｮ蜿ら・繧偵く繝｣繝・す繝･
    private playerStatusManager_Alpha statusManager;
    public GameObject gaugeCanvas;
    public CircleHPBarManager circleHPBarManager;

    void Start()
    {
        // Manager繧ｪ繝悶ず繧ｧ繧ｯ繝医°繧峨せ繝・・繧ｿ繧ｹ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ繧貞叙蠕励☆繧・
        GameObject managerObj = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
        if (managerObj != null)
        {
            statusManager = managerObj.GetComponent<playerStatusManager_Alpha>();
        }
        else
        {
            Debug.LogError("[PlayerHealth] 'manager' GameObject not found in the scene!");
        }

        // 蛻晄悄蛹匁凾縺ｫ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ蛛ｴ縺ｮHP縺ｧ繝ｭ繝ｼ繧ｫ繝ｫ螟画焚繧貞酔譛溘＠縺ｦ縺翫￥
        if (statusManager != null)
        {
            // 縺ｾ縺壽怙譁ｰ縺ｮ陬・ｙ繝舌ヵ繧堤｢ｺ螳溘↓險育ｮ励＆縺帙ｋ・亥ｮ溯｡碁・ｺ上・繧ｺ繝ｬ髦ｲ豁｢・・
            statusManager.UpdateEquipmentBuffs();

            HP = statusManager.HP;
            currentHP = statusManager.currentHP;

            // 蛻晄悄蛹匁凾縺ｫ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ蛛ｴ縺ｧ繝舌Μ繧｢繝舌ヵ縺梧寺縺九▲縺ｦ縺・ｌ縺ｰ驕ｩ逕ｨ縺吶ｋ
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
        base.Update(); // _Health_Base蛛ｴ縺ｮ辟｡謨ｵ譎る俣縺ｪ縺ｩ縺ｮ繧ｫ繧ｦ繝ｳ繝医ム繧ｦ繝ｳ蜃ｦ逅・ｒ螳溯｡・

        // 繝槭ロ繝ｼ繧ｸ繝｣繝ｼ蛛ｴ縺ｮ螳櫞P縺ｨ蟶ｸ縺ｫ蜷梧悄縺輔○縺ｦ縺翫￥
        // ・井ｻ悶せ繧ｯ繝ｪ繝励ヨ縺契Health_Base縺ｨ縺励※迴ｾ蝨ｨ縺ｮHP繧貞盾辣ｧ縺励◆蝣ｴ蜷医・鮨滄ｽｬ髦ｲ豁｢・・
        if (statusManager != null)
        {
            HP = statusManager.HP;
            currentHP = statusManager.currentHP;
            
            // 繝・ヰ繝・げ讖溯・: F3繧ｭ繝ｼ縺ｧ迴ｾ蝨ｨHP蛻・・繝繝｡繝ｼ繧ｸ繧貞女縺代ｋ
            if (Input.GetKeyDown(KeyCode.F3))
            {
                TakeDamage(statusManager.currentHP);
            }
        }
    }
    public void gaugeUpdate() {
        circleHPBarManager.UpdateCircleBar(statusManager.nowHPGauge, (statusManager.currentHP / statusManager.HP));
    }

    // 繝懊Β逋ｺ蜍墓凾縺ｪ縺ｩ縺ｮ荳螳壽凾髢鍋┌謨ｵ・亥ｽ薙◆繧雁愛螳壽ｶ亥､ｱ・牙・逅・
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
        if (isInvincible) return; // 辟｡謨ｵ荳ｭ縺ｪ繧峨ム繝｡繝ｼ繧ｸ繧堤┌隕悶☆繧・

        float setDmg = damage;
        // 蠑ｱ轤ｹ蛟咲紫縺ｪ縺ｩ繝ｭ繝ｼ繧ｫ繝ｫ・郁・霄ｫ・峨・迥ｶ諷九↓蝓ｺ縺･縺剰ｨ育ｮ・
        if (VulnerableFlg)
        {
            setDmg *= 1.5f;
        }
        
        // 螳滄圀縺ｮ繝繝｡繝ｼ繧ｸ驕ｩ逕ｨ縺ｯ縺吶∋縺ｦ荳蜈・ｮ｡逅・＠縺ｦ縺・ｋ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｸ蟋碑ｭｲ
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
