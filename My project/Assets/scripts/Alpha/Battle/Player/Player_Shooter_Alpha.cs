using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shooter_Alpha : MonoBehaviour
{
    public bool onCoolTime;
    public AudioSource shootAudioSource; // 髯滓汚・ｽ・ｾ驍ｵ・ｺ繝ｻ・ｮ鬨ｾ蜈ｷ・ｽ・ｺ髯昴・繝ｻ雎ｬ・ｹ鬨ｾ蛹・ｽｽ・ｨ驍ｵ・ｺ繝ｻ・ｮAudioSource
    public float moveRadius = 2f; // 驛｢譎丞ｹｲ・取ｨ抵ｽｹ・ｧ繝ｻ・､驛｢譎｢・ｽ・､驛｢譎｢・ｽ・ｼ驛｢・ｧ陷代・・ｽ・ｸ繝ｻ・ｭ髯滂ｽ｢郢晢ｽｻ遶雁､・ｸ・ｺ陷ｷ・ｶ繝ｻ邇匁ｺ鬮ｮ繝ｻ・ｽ・ｾ郢晢ｽｻ

    [Header("Weapon Settings")]
    public BASE_WeaponData_Alpha equippedWeaponData; // 髴托ｽｴ繝ｻ・ｾ髯懶ｽｨ繝ｻ・ｨ鬮ｯ・ｬ郢晢ｽｻ繝ｻ蜥擾ｽｸ・ｺ陷会ｽｱ遯ｶ・ｻ驍ｵ・ｺ郢晢ｽｻ繝ｻ邇厄ｽｱ繝ｻ・ｽ・ｦ髯懆ｶ｣・ｽ・ｨ驛｢譏ｴ繝ｻ郢晢ｽｻ驛｢・ｧ繝ｻ・ｿ郢晢ｽｻ郢晢ｽｻnspector驍ｵ・ｺ闕ｵ譎｢・ｽ閾･・ｹ・ｧ繝ｻ・｢驛｢・ｧ繝ｻ・ｿ驛｢譏ｴ繝ｻ郢晢ｽ｡髯ｷ・ｿ繝ｻ・ｯ鬮｢・ｭ繝ｻ・ｽ郢晢ｽｻ郢晢ｽｻ

    [Header("Spawn Pattern Settings")]
    public float shotIntervalSec = 0.05f;
    public float lateralSpacingWorld = 0.5f;
    public float spreadRangeDeg = 20f;
    public float radialStepDeg = 5f;
    public float reverseTravelTimeSec = 1.0f;

    [Header("Bouquet Settings")]
    public GameObject bouquetBulletPrefab;

    private Vector3 watch;
    private bool isPaused = false;
    private Transform playerTransform;
    GameObject PlayerObj;
    playerStatusManager_Alpha playerStatusScript;
    InventoryManager_Alpha inventoryManager;
    Alpha.PointerLineSystem pointerSystem;
    PlayerBulletManager_Alpha cachedBulletManager;
    bool hasSearchedBulletManager = false;

    [Header("Weapon Groups")]
    public int currentWeaponGroup = 0; // 0, 1, 2 (rows in inventory)

    void Awake()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        PlayerObj = playerTransform.gameObject;
        if (playerTransform != null)
        {
            playerStatusScript = playerStatusManager_Alpha.Instance;
            inventoryManager = GameObject.FindObjectOfType<InventoryManager_Alpha>();
        }
        pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        cachedBulletManager = Object.FindAnyObjectByType<PlayerBulletManager_Alpha>();
    }

    void Start()
    {
        onCoolTime = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f || isPaused)
            return;

        // 雎・ｽｦ陜趣ｽｨ郢ｧ・ｰ郢晢ｽｫ郢晢ｽｼ郢晏干繝ｻ郢晢ｽｫ郢晢ｽｼ郢晄懊・郢ｧ鬆大ｴ帷ｸｺ繝ｻ
        if (Alpha.Managers.PlayerInputManager_Alpha.Instance.WasWeaponPrevPressed || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            currentWeaponGroup--;
            if (currentWeaponGroup < 0) currentWeaponGroup = 2;
            Debug.Log("Switched weapon group.");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }
        else if (Alpha.Managers.PlayerInputManager_Alpha.Instance.WasWeaponNextPressed || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            currentWeaponGroup++;
            if (currentWeaponGroup > 2) currentWeaponGroup = 0;
            Debug.Log("Switched weapon group.");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }

        // --- 繧｢繧ｯ繝・ぅ繝悶せ繧ｭ繝ｫ縺ｮ逋ｺ蜍募愛螳・---
        if (playerStatusScript != null && playerStatusScript.HasActiveSkill)
        {
            if (Alpha.Managers.PlayerInputManager_Alpha.Instance.WasSpecialPressed)
            {
                TriggerActiveSkill();
            }
        }

        // 郢ｧ・ｨ郢ｧ・､郢晁汞・ｾ髮趣ｽ｡邵ｺ・ｮ陟趣ｽｧ隶灘生・定愾髢・ｾ繝ｻ
        Vector3 aimPos = Alpha.Managers.PlayerInputManager_Alpha.Instance.GetWorldAimPosition(transform.position);

        // 驛｢・ｧ繝ｻ・ｿ驛｢譎｢・ｽ・ｼ驛｢・ｧ繝ｻ・ｲ驛｢譏ｴ繝ｻ郢晢ｽｨ驛｢譎｢・ｽ・ｭ驛｢譏ｴ繝ｻ邵ｺ驢搾ｽｹ・ｧ繝ｻ・ｪ驛｢譎｢・ｽ・ｳ髫ｴ蠑ｱ・・ｹ晢ｽｻ髯ｷ繝ｻ・ｽ・ｦ鬨ｾ繝ｻ繝ｻ繝ｻ蟶晄≧繝ｻ・ｽ髯ｷ繝ｻ
        if (pointerSystem == null) 
        {
            pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        }
        
        Vector3 direction;
        Vector3 pPos = playerTransform.position;
        pPos.z = 0; // Z髯溯ｶ｣・ｽ・ｧ髫ｶ轣倡函郢晢ｽｻ0驍ｵ・ｺ繝ｻ・ｫ髯懈圜・ｽ・ｺ髯橸ｽｳ郢晢ｽｻ

        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            // 驛｢譎｢・ｽ・ｭ驛｢譏ｴ繝ｻ邵ｺ驢搾ｽｹ・ｧ繝ｻ・ｪ驛｢譎｢・ｽ・ｳ驍ｵ・ｺ陷会ｽｱ遯ｶ・ｻ驍ｵ・ｺ郢晢ｽｻ繝ｻ邇匁ｱ槭・・ｾ鬮ｮ雜｣・ｽ・｡驍ｵ・ｺ陟暮ｯ会ｽｼ讓抵ｽｹ・ｧ陟募ｾ後・驍ｵ・ｲ遶丞｣ｺ關ｽ驍ｵ・ｺ繝ｻ・ｮ髯昴・・ｽ・ｾ鬮ｮ雜｣・ｽ・｡驍ｵ・ｺ繝ｻ・ｮ髫ｴ繝ｻ・ｽ・ｹ髯ｷ・ｷ闔会ｽ｣繝ｻ螳壽・闔会ｽ｣繝ｻ・･
            Vector3 targetPos = pointerSystem.CurrentTarget.position;
            targetPos.z = 0; 
            direction = (targetPos - pPos).normalized;
        }
        else
        {
            // 驍ｵ・ｺ郢晢ｽｻ遶企・・ｸ・ｺ闔会ｽ｣繝ｻ讙趣ｽｸ・ｺ繝ｻ・ｰ髣碑・・ｿ・ｫ遶擾ｽｪ驍ｵ・ｺ繝ｻ・ｧ鬯ｨ・ｾ陞｢・ｹ繝ｻ鬘費ｽｹ譎・ｽｧ・ｭ邵ｺ閧ｲ・ｹ・ｧ繝ｻ・ｹ驍ｵ・ｺ繝ｻ・ｮ髫ｴ繝ｻ・ｽ・ｹ髯ｷ・ｷ闔会ｽ｣繝ｻ螳壽・闔会ｽ｣繝ｻ・･
            direction = (aimPos - pPos).normalized;
        }

        // 騾具ｽｺ陝・・蟀ｿ陷ｷ莉｣繝ｻ髫ｪ閧ｲ・ｮ證ｦ・ｼ蛹ｻ繝ｻ郢ｧ・ｦ郢ｧ・ｹ隴・ｽｹ陷ｷ莉｣竏ｪ邵ｺ貅倥・陷ｿ・ｳ郢ｧ・ｹ郢昴・縺・ｹ昴・縺題ｭ・ｽｹ陷ｷ謇假ｽｼ繝ｻ
        watch = direction;
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;

        // 陟托ｽｾ邵ｺ・ｮ騾具ｽｺ陝・・諢幄楜繝ｻ
        if (Time.timeScale != 0f && (Alpha.Managers.PlayerInputManager_Alpha.Instance.IsFiring && !onCoolTime))
        {
            onCoolTime = true;
            StartCoroutine(ShootAndCooldownRoutine());
        }
    }

    private IEnumerator ShootAndCooldownRoutine()
    {
        if (playerStatusScript == null) playerStatusScript = playerStatusManager_Alpha.Instance;
        int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
        float burstInterval = 0.05f; // 驛｢譎｢・ｽ・ｦ驛｢譎｢・ｽ・ｼ驛｢・ｧ繝ｻ・ｶ驛｢譎｢・ｽ・ｼ鬮ｫ陬懈桶隰泌調・ｸ・ｺ繝ｻ・ｫ驛｢・ｧ陋ｹ・ｻ繝ｻ繝ｻ.1鬩穂ｼ懊・遶擾ｽｪ驍ｵ・ｺ雋・･繝ｻ2-3驛｢譎・ｽｼ驥・ｨ抵ｽｹ譎｢・ｽ・ｼ驛｢譎｢・ｿ・ｽ鬩墓ｧｫ蜚ｱ繝ｻ・ｺ繝ｻ・ｦ驍ｵ・ｺ繝ｻ・ｮ鬩墓得・ｽ・ｭ驍ｵ・ｺ郢晢ｽｻ闖ｫ・｣鬯ｮ・ｫ郢晢ｽｻ

        for (int i = 0; i < burstCount; i++)
        {
            Shoot(); // 逋ｺ蟆・・逅・ｒ蜻ｼ縺ｳ蜃ｺ縺・
            
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // 驛｢譎√・郢晢ｽｻ驛｢・ｧ繝ｻ・ｹ驛｢譎√＃陋ｹ・ｱ髯昴・繝ｻ繝ｻ・ｵ郢ｧ繝ｻ・ｽ・ｺ郢晢ｽｻ繝ｻ・ｾ陟募ｨｯ繝ｻ驛｢・ｧ繝ｻ・ｯ驛｢譎｢・ｽ・ｼ驛｢譎｢・ｽ・ｫ驛｢・ｧ繝ｻ・ｿ驛｢・ｧ繝ｻ・､驛｢譎｢・ｿ・ｽ郢晢ｽｻ陋ｹ・ｻ・取㏍・ｹ譎｢・ｽ・ｭ驛｢譎｢・ｽ・ｼ驛｢譏懶ｽｼ螟ｲ・ｽ・ｼ陝ｲ・ｨ繝ｻ蟶晢ｽｫ・｢陷ｿ・･繝ｻ・ｧ闕ｵ譏ｶ繝ｻ驛｢・ｧ郢晢ｽｻ
        // 髯憺屮・ｽ・ｺ髮九・縺倡ｹ晢ｽｻ鬨ｾ蜈ｷ・ｽ・ｺ髯昴・繝ｻ闖ｫ・｣鬯ｮ・ｫ隴∵腸・ｽ繝ｻ.8鬩穂ｼ懊・遶企ｦｴ蝮弱・・ｭ髯橸ｽｳ郢晢ｽｻ
        float baseInterval = 0.8f;
        // 鬯ｮ・｢繝ｻ・｢髫ｰ・ｨ繝ｻ・ｰ驍ｵ・ｺ繝ｻ・ｮ髯区ｺｷ隱ｿ驍擾ｽｫ驛｢・ｧ陝ｶ譏ｶ繝ｻ鬨ｾ蛹・ｽｽ・ｨ (髣懆侭繝ｻ BulletSpanMag驍ｵ・ｺ郢晢ｽｻ00驍ｵ・ｺ繝ｻ・ｮ髯懶ｽ｣繝ｻ・ｴ髯ｷ・ｷ陋ｹ・ｻ郢晢ｽｻ1髯区ｻ・ｺゑｾやぎ郢晢ｽｻ0驍ｵ・ｺ繝ｻ・ｮ髯懶ｽ｣繝ｻ・ｴ髯ｷ・ｷ陋ｹ・ｻ郢晢ｽｻ0.5髯区ｺ倥・
        float targetInterval = baseInterval * (playerStatusScript != null ? playerStatusScript.BulletSpanMag * 0.01f : 1f);
        
        yield return new WaitForSecondsRealtime(targetInterval);
        onCoolTime = false;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void SetWatchDirection(Vector3 direction)
    {
        watch = direction;
    }

    public void Shoot()
    {
        if (playerStatusScript == null) playerStatusScript = playerStatusManager_Alpha.Instance;
        if (playerStatusScript == null) return;
        ShootFromData(false);
    }

    private void TriggerActiveSkill()
    {
        if (playerStatusScript == null) playerStatusScript = playerStatusManager_Alpha.Instance;
        if (playerStatusScript == null || !playerStatusScript.HasActiveSkill) return;

        string effectClass = playerStatusScript.currentActiveEffectClassName;
        int pos = playerStatusScript.currentActiveEffectEquipPosition;
        int rarity = playerStatusScript.currentActiveEffectRarity;

        Alpha_Effect_Base activeEffect = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(effectClass, pos, rarity);
        if (activeEffect != null)
        {
            activeEffect.OnActiveSkill(this);
        }
    }

    private void ShootFromData(bool isBouquet)
    {
        // InventoryManager邵ｺ荵晢ｽ芽ｿｴ・ｾ陜ｨ・ｨ邵ｺ・ｮ雎・ｽｦ陜趣ｽｨ(y = currentWeaponGroup)邵ｺ・ｮ3邵ｺ・､邵ｺ・ｮ雎・ｽｦ陜趣ｽｨ郢昴・繝ｻ郢ｧ・ｿ郢ｧ雋槫徐陟輔・
        Alpha.Data.WeaponSeriesData_Alpha series1 = null;
        Alpha.Data.WeaponSeriesData_Alpha series2 = null;
        Alpha.Data.WeaponSeriesData_Alpha series3 = null;
        
        if (inventoryManager != null)
        {
            var inst1 = inventoryManager.Get(0, currentWeaponGroup);
            var inst2 = inventoryManager.Get(1, currentWeaponGroup);
            var inst3 = inventoryManager.Get(2, currentWeaponGroup);
            
            series1 = inst1.series;
            series2 = inst2.series;
            series3 = inst3.series;
        }

        bool isBouquetLocal = isBouquet;
        if (inventoryManager != null && !isBouquetLocal) isBouquetLocal = inventoryManager.IsBouquetActive();

        List<GameObject> prefabsToInstantiate = new List<GameObject>();
        float bulletChangeMultiplier = 0f;

        if (isBouquetLocal)
        {
            if (inventoryManager != null)
            {
                var instA = inventoryManager.Get(0, 0);
                var instB = inventoryManager.Get(0, 1);
                var instC = inventoryManager.Get(0, 2);
                if (instA.series != null && instA.series.bulletPrefab != null) prefabsToInstantiate.Add(instA.series.bulletPrefab);
                if (instB.series != null && instB.series.bulletPrefab != null) prefabsToInstantiate.Add(instB.series.bulletPrefab);
                if (instC.series != null && instC.series.bulletPrefab != null) prefabsToInstantiate.Add(instC.series.bulletPrefab);
            }
            if (prefabsToInstantiate.Count == 0 && bouquetBulletPrefab != null)
            {
                prefabsToInstantiate.Add(bouquetBulletPrefab);
            }
            if (prefabsToInstantiate.Count == 0)
            {
                prefabsToInstantiate.Add(Resources.Load<GameObject>("Objects/Bullet/NormalBullet"));
            }
        }
        else
        {
            GameObject singlePrefab = Resources.Load<GameObject>("Objects/Bullet/NormalBullet");
            // --- 隴・ｽｰ邵ｺ蜉ｱ・櫁托ｽｾ陷・ｽｪ陷井ｺ･・ｺ・ｦ郢晢ｽｭ郢ｧ・ｸ郢昴・縺・---
            Alpha.Data.BulletChangeWeaponEffectSO_Alpha bestEffect = null;
            int bestSlotIndex = -1;
            bool bestHasAllEq = false;
            float bestScore = -1f;
            int bestRarity = 1;

            if (inventoryManager != null)
            {
                for (int slot = 0; slot < 3; slot++)
                {
                    var inst = inventoryManager.Get(slot, currentWeaponGroup);
                    if (inst.series == null) continue;

                    bool hasAllEq = false;
                    if (inst.currentEffects != null)
                    {
                        foreach (var eff in inst.currentEffects)
                        {
                            if (eff != null && eff.effectType == Alpha.Data.WeaponEffectType_Alpha.AllEquipable) hasAllEq = true;
                        }
                    }

                    Alpha.Data.BulletChangeWeaponEffectSO_Alpha bcEffect = null;
                    if (inst.currentEffects != null)
                    {
                        foreach (var eff in inst.currentEffects)
                        {
                            if (eff is Alpha.Data.BulletChangeWeaponEffectSO_Alpha bce) bcEffect = bce;
                        }
                    }
                    if (bcEffect == null) continue;

                    float currentScore = (int)bcEffect.seriesTier;
                    if (bcEffect.bulletPrefab != null && bcEffect.bulletPrefab.GetComponent<CircularObject>() != null)
                    {
                        currentScore = 2.5f;
                    }

                    if (currentScore > bestScore)
                    {
                        bestEffect = bcEffect; bestSlotIndex = slot; bestScore = currentScore; bestHasAllEq = hasAllEq; bestRarity = inst.rarity > 0 ? inst.rarity : 1;
                    }
                    else if (UnityEngine.Mathf.Approximately(currentScore, bestScore))
                    {
                        if (hasAllEq && !bestHasAllEq)
                        {
                            bestEffect = bcEffect; bestSlotIndex = slot; bestHasAllEq = hasAllEq; bestRarity = inst.rarity > 0 ? inst.rarity : 1;
                        }
                        else if (hasAllEq == bestHasAllEq)
                        {
                            if (slot > bestSlotIndex)
                            {
                                bestEffect = bcEffect; bestSlotIndex = slot; bestHasAllEq = hasAllEq; bestRarity = inst.rarity > 0 ? inst.rarity : 1;
                            }
                        }
                    }
                }
            }

            if (bestEffect != null && bestEffect.bulletPrefab != null)
            {
                singlePrefab = bestEffect.bulletPrefab;
                bulletChangeMultiplier = bestEffect.GetValue(bestRarity);
            }
            else
            {
                if (series3 != null && series3.bulletPrefab != null) singlePrefab = series3.bulletPrefab;
                else if (series2 != null && series2.bulletPrefab != null) singlePrefab = series2.bulletPrefab;
                else if (series1 != null && series1.bulletPrefab != null) singlePrefab = series1.bulletPrefab;
                else UnityEngine.Debug.LogWarning("[Player_Shooter] Weapon bullet prefab is not set. Using default.");
            }
            prefabsToInstantiate.Add(singlePrefab);
        }
        float baseWeaponDamage = 0f;

        if (prefabsToInstantiate.Count > 0 && prefabsToInstantiate[0] != null)
        {
            Bullet_Base baseBullet = prefabsToInstantiate[0].GetComponent<Bullet_Base>();
            if (baseBullet != null)
            {
                baseWeaponDamage = baseBullet.dmg;
            }
        }

        // --- 鬨ｾ蜈ｷ・ｽ・ｺ髯昴・繝ｻ遶頑･｢・｢郢晢ｽｻ繝ｻ・ｦ遶丞｣ｺ繝ｻ髯ｷ闌ｨ・ｽ・ｱ鬯ｨ・ｾ陞｢・ｹ郢晢ｽｱ驛｢譎｢・ｽ・ｩ驛｢譎｢・ｽ・｡驛｢譎｢・ｽ・ｼ驛｢・ｧ繝ｻ・ｿ驍ｵ・ｺ繝ｻ・ｮ鬮ｫ・ｪ髢ｧ・ｲ繝ｻ・ｮ郢晢ｽｻ---
        if (playerStatusScript == null) 
        {
            playerStatusScript = playerStatusManager_Alpha.Instance;
            if (playerStatusScript == null) 
            {
                Debug.LogError("[Player_Shooter] playerStatusManager_Alpha.Instance is null! Cannot shoot.");
                return;
            }
        }
        float finalDamage = playerStatusScript.GetFinalDamage(baseWeaponDamage);
        finalDamage *= (1f + bulletChangeMultiplier);
        int totalShotCount = 1 + playerStatusScript.extraShotCount;
        var pattern = playerStatusScript.currentSpawnPattern;

        int localCircularSubShots = 0;
        int localVoltTickReduce = 0;
        float localSecondaryDamageUp = 0f;

        if (inventoryManager != null)
        {
            int loopCount = isBouquet ? 9 : 3;
            for (int n = 0; n < loopCount; n++)
            {
                int x = isBouquet ? (n % 3) : n;
                int y = isBouquet ? (n / 3) : currentWeaponGroup;
                var inst = inventoryManager.Get(x, y);
                if (inst.series != null && inst.currentEffects != null)
                {
                    foreach (var eff in inst.currentEffects)
                    {
                        if (eff != null)
                        {
                            int rarity = inst.rarity > 0 ? inst.rarity : 1;
                            if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.CircularSubShotPlus)
                                localCircularSubShots += (int)eff.GetValue(rarity);
                            else if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.VoltTickReduce)
                                localVoltTickReduce += (int)eff.GetValue(rarity);
                        }
                    }
                }
            }
        }

        int bulletExtraShots = 0;
        if (prefabsToInstantiate.Count > 0 && prefabsToInstantiate[0] != null && prefabsToInstantiate[0].GetComponent<CircularObject>() != null)
        {
            bulletExtraShots = localCircularSubShots; // 陝・ｉ逡醍ｹｧ・ｨ郢晁ｼ斐♂郢ｧ・ｯ郢晏現繝ｻ陝・ｻ呻ｽｼ・ｾ陟・懷・

            int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
            if (burstCount > 1 || totalShotCount > 1)
            {
                finalDamage *= 0.3f; // 髫包ｽｪ騾具ｽｺ陝・・辟夂ｸｺ謔滂ｽ｢蜉ｱ竏ｴ邵ｺ貅ｷ・ｽ・ｴ陷ｷ蛹ｻ繝ｻ0.3陋溘・
            }
        }
        else
        {
            // do nothing
        }
        Vector3 aimPoint = Alpha.Managers.PlayerInputManager_Alpha.Instance.GetWorldAimPosition(transform.position);
        aimPoint.z = 0;
        bool isTargetLocked = false;
        Transform lockedTarget = null;
        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            aimPoint = pointerSystem.CurrentTarget.position;
            aimPoint.z = 0;
            isTargetLocked = true;
            lockedTarget = pointerSystem.CurrentTarget;
        }

        Vector3 muzzlePos = playerTransform.position + (watch * moveRadius);
        Vector3 aimDirection = watch; // 驛｢譎・ｽｧ・ｭ邵ｺ閧ｲ・ｹ・ｧ繝ｻ・ｹ驍ｵ・ｺ陟募ｾ後・驛｢譎｢・ｽ・ｬ驛｢・ｧ繝ｻ・､驛｢譎｢・ｽ・､驛｢譎｢・ｽ・ｼ驍ｵ・ｺ繝ｻ・ｫ鬮ｴ蜿ｰ・ｻ・｣隨倥・・ｸ・ｺ陟托ｽｱ繝ｻ迢暦ｽｸ・ｺ繝ｻ・ｨ (aimPoint - muzzlePos) 驍ｵ・ｺ驕偵・ﾂ郢晢ｽｻ繝ｻ・ｻ繝ｻ・｢驍ｵ・ｺ陷ｷ・ｶ繝ｻ迢暦ｽｹ譎√・邵ｺ蝣､・ｹ・ｧ陝ｶ譏懶ｽｺ貅ｽ・ｸ・ｺ髣雁ｨｯ陞ｺ驛｢・ｧ遶丞仰遶乗劼・ｽ・ｸ繝ｻ・ｸ驍ｵ・ｺ繝ｻ・ｫwatch髫ｴ繝ｻ・ｽ・ｹ髯ｷ・ｷ闔会ｽ｣繝ｻ螳壽割繝ｻ・ｿ鬨ｾ蛹・ｽｽ・ｨ

        TriggerWeaponFireEffects(muzzlePos, aimDirection, finalDamage, isBouquet);

        StartCoroutine(SpawnBulletRoutine(prefabsToInstantiate, muzzlePos, aimDirection, aimPoint, totalShotCount, pattern, finalDamage, isTargetLocked, lockedTarget, isBouquet, bulletExtraShots, 0, bulletChangeMultiplier, localVoltTickReduce, localSecondaryDamageUp));
    }

    private float GetExplosionScaleByRarity(int rarity)
    {
        if (rarity <= 1) return 0.8f;
        if (rarity == 2) return 1.0f;
        if (rarity == 3) return 1.2f;
        return 2.0f;
    }

    private void SpawnExplosionArea(Vector3 position, float dmg, float scaleMultiplier = 1.0f)
    {
        GameObject prefab = Resources.Load<GameObject>("Objects/Effect_Explosion");
        if (prefab != null)
        {
            GameObject obj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                obj = Alpha_ObjectPoolManager.Instance.Rent(prefab, position, Quaternion.identity);
            }
            else
            {
                obj = Instantiate(prefab, position, Quaternion.identity);
            }

            if (obj != null)
            {
                obj.transform.localScale = prefab.transform.localScale * scaleMultiplier;
            }

            Alpha_ExplosionArea areaScript = obj.GetComponent<Alpha_ExplosionArea>();
            if (areaScript != null)
            {
                areaScript.sourcePrefab = prefab;
                areaScript.lifetime = 1.0f; // 1遘偵〒豸医∴繧・
                areaScript.ActivateExplosionArea(dmg);
            }
            else
            {
                Effect_Explosion oldScript = obj.GetComponent<Effect_Explosion>();
                if (oldScript != null)
                {
                    oldScript.sourcePrefab = prefab;
                    oldScript.startExplosion(dmg, 10);
                }
            }
        }
    }

    private void TriggerWeaponFireEffects(Vector3 muzzlePos, Vector3 aimDir, float finalDamage, bool isBouquet)
    {
        if (inventoryManager == null) return;
        
        System.Action<Alpha.Data.WeaponEffectSO_Alpha, int> triggerEffect = null;
        triggerEffect = (effSO, rarity) =>
        {
            if (effSO == null) return;
            if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
            {
                var comp = effSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                if (comp != null && comp.subEffects != null)
                {
                    foreach (var sub in comp.subEffects) triggerEffect(sub, rarity);
                }
                return;
            }

            

            effSO.OnWeaponFire(this.gameObject, muzzlePos, aimDir, finalDamage, rarity);
        };

        System.Action<Alpha.Data.WeaponSeriesData_Alpha, int> processSeries = (series, rarity) =>
        {
            if (series == null) return;
            foreach (var eff in series.bulletSpecificEffects) triggerEffect(eff, rarity);
            foreach (var eff in series.casingSpecificEffects) triggerEffect(eff, rarity);
            foreach (var eff in series.primerSpecificEffects) triggerEffect(eff, rarity);
        };

        if (isBouquet)
        {
            var instA = inventoryManager.Get(0, 0);
            var instB = inventoryManager.Get(0, 1);
            var instC = inventoryManager.Get(0, 2);
            processSeries(instA.series, instA.rarity);
            processSeries(instB.series, instB.rarity);
            processSeries(instC.series, instC.rarity);
        }
        else
        {
            var inst1 = inventoryManager.Get(0, currentWeaponGroup);
            var inst2 = inventoryManager.Get(1, currentWeaponGroup);
            var inst3 = inventoryManager.Get(2, currentWeaponGroup);
            processSeries(inst1.series, inst1.rarity);
            processSeries(inst2.series, inst2.rarity);
            processSeries(inst3.series, inst3.rarity);
        }
    }

    private IEnumerator SpawnBulletRoutine(List<GameObject> prefabs, Vector3 muzzlePos, Vector3 aimDir, Vector3 aimPoint, int shotCount, playerStatusManager_Alpha.SpawnPattern pattern, float finalDmg, bool isTargetLocked, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0, float bulletChangeMultiplier = 0f, int voltTickReduceForBullet = 0, float secondaryDamageUpForBullet = 0f)
    {
        for (int i = 0; i < shotCount; i++)
        {
            Vector3 spawnPos = muzzlePos;
            Vector3 spawnDir = aimDir;
            

            // 髯ｷ・ｷ郢晢ｽｻ繝ｻ・ｼ繝ｻ・ｾ驍ｵ・ｺ隴∫ｵｶ繝ｻ驍ｵ・ｺ繝ｻ・ｫ驛｢・ｧ繝ｻ・ｨ驛｢譎・ｽｼ譁絶凾驛｢・ｧ繝ｻ・ｯ驛｢譎冗樟邵ｺ繝ｻ・ｹ譎｢・ｽ・ｳ驛｢・ｧ繝ｻ・ｹ驛｢・ｧ繝ｻ・ｿ驛｢譎｢・ｽ・ｳ驛｢・ｧ繝ｻ・ｹ驛｢・ｧ陜｣・､陷・ｽｽ髫ｰ迹壹・隨倥・・ｹ・ｧ郢晢ｽｻ

            if (pattern == playerStatusManager_Alpha.SpawnPattern.Straight)
            {
                Vector3 rightDir = new Vector3(aimDir.y, -aimDir.x, 0).normalized;
                float offset = 0f;
                if (shotCount % 2 == 1)
                {
                    int step = (i + 1) / 2;
                    float sign = (i % 2 == 1) ? 1f : -1f;
                    if (i == 0) step = 0;
                    offset = step * lateralSpacingWorld * sign;
                }
                else
                {
                    int step = i / 2;
                    float sign = (i % 2 == 0) ? 1f : -1f;
                    offset = (step + 0.5f) * lateralSpacingWorld * sign;
                }
                spawnPos += rightDir * offset;
            }
            else if (pattern == playerStatusManager_Alpha.SpawnPattern.Radial)
            {
                float offsetDeg = 0f;
                if (shotCount % 2 == 1)
                {
                    int step = (i + 1) / 2;
                    float sign = (i % 2 == 1) ? 1f : -1f;
                    if (i == 0) step = 0;
                    offsetDeg = step * radialStepDeg * sign;
                }
                else
                {
                    int step = i / 2;
                    float sign = (i % 2 == 0) ? 1f : -1f;
                    offsetDeg = (step + 0.5f) * radialStepDeg * sign;
                }
                spawnDir = Quaternion.Euler(0, 0, offsetDeg) * aimDir;
            }
                        GameObject prefabToUse = Alpha_BulletPrototypeBuilder.GetOrBuildPrototype(isBouquet ? -1 : currentWeaponGroup, inventoryManager);
            if (prefabToUse == null) { prefabToUse = prefabs[i % prefabs.Count]; }
            CreateSingleBullet(prefabToUse, spawnPos, spawnDir, aimDir, finalDmg, lockedTarget, isBouquet, extraShotsForBullet, extraPierceForBullet, bulletChangeMultiplier, voltTickReduceForBullet, secondaryDamageUpForBullet);

            // 郢ｧ・ｵ郢ｧ・ｦ郢晢ｽｳ郢晏ｳｨ縺顔ｹ晁ｼ斐♂郢ｧ・ｯ郢晏現繝ｻ陷蜥ｲ蜃ｽ繝ｻ莠･・ｿ繝ｻ・ｦ竏壺・陟｢諛環ｧ邵ｺ・ｦ繝ｻ繝ｻ
            // if (shootAudioSource != null) shootAudioSource.Play();

            if (pattern == playerStatusManager_Alpha.SpawnPattern.Barrage )
            {
                yield return new WaitForSeconds(shotIntervalSec);
            }
        }
    }

    private void CreateSingleBullet(GameObject prefabToInstantiate, Vector3 spawnPos, Vector3 spawnDir, Vector3 originalAimDir, float finalDamage, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0, float bulletChangeMultiplier = 0f, int voltTickReduceForBullet = 0, float secondaryDamageUpForBullet = 0f)
    {
        GameObject bulletPrefab;
        if (Alpha_ObjectPoolManager.Instance != null)
        {
            bulletPrefab = Alpha_ObjectPoolManager.Instance.Rent(prefabToInstantiate, spawnPos, Quaternion.identity);
        }
        else
        {
            bulletPrefab = Instantiate(prefabToInstantiate, spawnPos, Quaternion.identity);
            bulletPrefab.SetActive(true);
        }

        float rotationAngle = Mathf.Atan2(spawnDir.y, spawnDir.x) * Mathf.Rad2Deg;
        bulletPrefab.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();
        
        if (bulletScript != null)
        {
            bulletScript.sourcePrefab = prefabToInstantiate;
            
            
            

            Bullet_Base prefabScript = prefabToInstantiate.GetComponent<Bullet_Base>();
            float originalSpeed = prefabScript != null ? prefabScript.Speed : bulletScript.Speed;
            float originalDestroyTime = prefabScript != null ? prefabScript.DestroyTime : bulletScript.DestroyTime;
            
            float baseBulletSpeed = playerStatusScript.bulletSpeed * playerStatusScript.bulletSpeedMag * (originalSpeed * 0.01f);
            baseBulletSpeed *= (1f + bulletChangeMultiplier);
            
            bulletScript.setStatus(spawnDir, baseBulletSpeed, finalDamage);
            bulletScript.DestroyTime = originalDestroyTime * playerStatusScript.bulletLifeMag * (1f + bulletChangeMultiplier);

            if (playerStatusScript != null && playerStatusScript.ignorePierceDecay)
            {
                bulletScript.localPierceDamageReductionRate = 0f;
            }
            else
            {
                bulletScript.localPierceDamageReductionRate = -1f;
            }

            
            

            if (cachedBulletManager == null && !hasSearchedBulletManager) 
            {
                cachedBulletManager = Object.FindAnyObjectByType<PlayerBulletManager_Alpha>();
                hasSearchedBulletManager = true;
                if (cachedBulletManager == null)
                {
                    Debug.LogWarning("[Player_Shooter] PlayerBulletManager_Alpha not found in scene. Pierce count from manager will be 0.");
                }
            }
            
            if (cachedBulletManager != null)
            {
                bulletScript.piercingCount += cachedBulletManager.pierceCount;
            }

            bulletScript.piercingCount += playerStatusScript.extraPierceCount;
            bulletScript.piercingCount += extraPierceForBullet;
            bulletScript.extraShotCount += extraShotsForBullet;
            bulletScript.voltTickReduceCount += voltTickReduceForBullet;
            bulletScript.secondaryDamageMultiplier += secondaryDamageUpForBullet;

            // 驍擾ｽｫ邵ｺ・ｮ郢ｧ・ｪ郢晢ｽｼ郢晢ｽｩ繝ｻ繝ｻrailRenderer繝ｻ蟲ｨ・定恪諷募飭邵ｺ・ｫ闔牙・ｽｸ繝ｻ
            if (isBouquet)
            {
                TrailRenderer tr = bulletPrefab.GetComponent<TrailRenderer>();
                if (tr == null) tr = bulletPrefab.AddComponent<TrailRenderer>();
                
                tr.enabled = true;
                tr.time = 0.2f;
                tr.startWidth = bulletPrefab.transform.localScale.x * 0.8f;
                tr.endWidth = 0f;
                tr.material = new Material(Shader.Find("Sprites/Default"));
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.6f, 0f, 1f), 0.0f), new GradientColorKey(new Color(0.3f, 0f, 0.5f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                tr.colorGradient = gradient;
                tr.sortingLayerName = "Effect";
                tr.sortingOrder = -1;
                tr.minVertexDistance = 0.1f;
            }
            else
            {
                TrailRenderer tr = bulletPrefab.GetComponent<TrailRenderer>();
                if (tr != null) tr.enabled = false;
            }

            bulletPrefab.SetActive(true);
            bulletScript.shoot();
        }
    }
}








