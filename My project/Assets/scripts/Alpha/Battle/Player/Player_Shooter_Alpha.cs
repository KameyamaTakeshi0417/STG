using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shooter_Alpha : MonoBehaviour
{
    public bool onCoolTime;
    public AudioSource shootAudioSource; // 陟托ｽｾ邵ｺ・ｮ騾具ｽｺ陝・・豬ｹ騾包ｽｨ邵ｺ・ｮAudioSource
    public float moveRadius = 2f; // 郢晏干ﾎ樒ｹｧ・､郢晢ｽ､郢晢ｽｼ郢ｧ蜑・ｽｸ・ｭ陟｢繝ｻ竊堤ｸｺ蜷ｶ・玖怺髮・ｽｾ繝ｻ

    [Header("Weapon Settings")]
    public BASE_WeaponData_Alpha equippedWeaponData; // 霑ｴ・ｾ陜ｨ・ｨ髯ｬ繝ｻ・咏ｸｺ蜉ｱ窶ｻ邵ｺ繝ｻ・玖ｱ・ｽｦ陜趣ｽｨ郢昴・繝ｻ郢ｧ・ｿ繝ｻ繝ｻnspector邵ｺ荵晢ｽ臥ｹｧ・｢郢ｧ・ｿ郢昴・繝｡陷ｿ・ｯ髢ｭ・ｽ繝ｻ繝ｻ

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

        // 豁ｦ蝎ｨ繧ｰ繝ｫ繝ｼ繝励・繝ｫ繝ｼ繝怜・繧頑崛縺・
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

        // --- アクティブスキルの発動判定 ---
        if (playerStatusScript != null && playerStatusScript.HasActiveSkill)
        {
            if (Alpha.Managers.PlayerInputManager_Alpha.Instance.WasSpecialPressed)
            {
                TriggerActiveSkill();
            }
        }

        // 繧ｨ繧､繝蟇ｾ雎｡縺ｮ蠎ｧ讓吶ｒ蜿門ｾ・
        Vector3 aimPos = Alpha.Managers.PlayerInputManager_Alpha.Instance.GetWorldAimPosition(transform.position);

        // 郢ｧ・ｿ郢晢ｽｼ郢ｧ・ｲ郢昴・繝ｨ郢晢ｽｭ郢昴・縺醍ｹｧ・ｪ郢晢ｽｳ隴弱ｅ繝ｻ陷・ｽｦ騾・・・帝恆・ｽ陷・
        if (pointerSystem == null) 
        {
            pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        }
        
        Vector3 direction;
        Vector3 pPos = playerTransform.position;
        pPos.z = 0; // Z陟趣ｽｧ隶灘生繝ｻ0邵ｺ・ｫ陜暦ｽｺ陞ｳ繝ｻ

        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            // 郢晢ｽｭ郢昴・縺醍ｹｧ・ｪ郢晢ｽｳ邵ｺ蜉ｱ窶ｻ邵ｺ繝ｻ・玖汞・ｾ髮趣ｽ｡邵ｺ蠕鯉ｼ樒ｹｧ蠕後・邵ｲ竏壺落邵ｺ・ｮ陝・ｽｾ髮趣ｽ｡邵ｺ・ｮ隴・ｽｹ陷ｷ莉｣・定惺莉｣・･
            Vector3 targetPos = pointerSystem.CurrentTarget.position;
            targetPos.z = 0; 
            direction = (targetPos - pPos).normalized;
        }
        else
        {
            // 邵ｺ繝ｻ竊醍ｸｺ莉｣・檎ｸｺ・ｰ闔臥ｿｫ竏ｪ邵ｺ・ｧ鬨ｾ螢ｹ・顔ｹ晄ｧｭ縺育ｹｧ・ｹ邵ｺ・ｮ隴・ｽｹ陷ｷ莉｣・定惺莉｣・･
            direction = (aimPos - pPos).normalized;
        }

        // 逋ｺ蟆・婿蜷代・險育ｮ暦ｼ医・繧ｦ繧ｹ譁ｹ蜷代∪縺溘・蜿ｳ繧ｹ繝・ぅ繝・け譁ｹ蜷托ｼ・
        watch = direction;
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;

        // 蠑ｾ縺ｮ逋ｺ蟆・愛螳・
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
        float burstInterval = 0.05f; // 郢晢ｽｦ郢晢ｽｼ郢ｧ・ｶ郢晢ｽｼ髫補扱謔咲ｸｺ・ｫ郢ｧ蛹ｻ・・.1驕伜・竏ｪ邵ｺ貅倥・2-3郢晁ｼ釆樒ｹ晢ｽｼ郢晢ｿｽ驕槫唱・ｺ・ｦ邵ｺ・ｮ驕擾ｽｭ邵ｺ繝ｻ菫｣鬮ｫ繝ｻ

        for (int i = 0; i < burstCount; i++)
        {
            Shoot(); // 発射処理を呼び出し
            
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // 郢晁・繝ｻ郢ｧ・ｹ郢晁ご蛹ｱ陝・・・ｵ繧・ｽｺ繝ｻ・ｾ蠕娯・郢ｧ・ｯ郢晢ｽｼ郢晢ｽｫ郢ｧ・ｿ郢ｧ・､郢晢ｿｽ繝ｻ蛹ｻﾎ懃ｹ晢ｽｭ郢晢ｽｼ郢昜ｼ夲ｽｼ蟲ｨ・帝ｫ｢蜿･・ｧ荵昶・郢ｧ繝ｻ
        // 陜難ｽｺ雋・じ繝ｻ騾具ｽｺ陝・・菫｣鬮ｫ譁撰ｽ・.8驕伜・竊馴坎・ｭ陞ｳ繝ｻ
        float baseInterval = 0.8f;
        // 鬮｢・｢隰ｨ・ｰ邵ｺ・ｮ陋溷調邏ｫ郢ｧ蟶昶・騾包ｽｨ (關薙・ BulletSpanMag邵ｺ繝ｻ00邵ｺ・ｮ陜｣・ｴ陷ｷ蛹ｻ繝ｻ1陋滄亂ﾂ€繝ｻ0邵ｺ・ｮ陜｣・ｴ陷ｷ蛹ｻ繝ｻ0.5陋溘・
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
        // InventoryManager縺九ｉ迴ｾ蝨ｨ縺ｮ豁ｦ蝎ｨ(y = currentWeaponGroup)縺ｮ3縺､縺ｮ豁ｦ蝎ｨ繝・・繧ｿ繧貞叙蠕・
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
            // --- 譁ｰ縺励＞蠑ｾ蜆ｪ蜈亥ｺｦ繝ｭ繧ｸ繝・け ---
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

        // --- 騾具ｽｺ陝・・竊楢｢繝ｻ・ｦ竏壺・陷茨ｽｱ鬨ｾ螢ｹ繝ｱ郢晢ｽｩ郢晢ｽ｡郢晢ｽｼ郢ｧ・ｿ邵ｺ・ｮ髫ｪ閧ｲ・ｮ繝ｻ---
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
            bulletExtraShots = localCircularSubShots; // 蟆ら畑繧ｨ繝輔ぉ繧ｯ繝医・蟄仙ｼｾ蠅怜刈

            int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
            if (burstCount > 1 || totalShotCount > 1)
            {
                finalDamage *= 0.3f; // 隕ｪ逋ｺ蟆・焚縺悟｢励∴縺溷�ｴ蜷医・0.3蛟・
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
        Vector3 aimDirection = watch; // 郢晄ｧｭ縺育ｹｧ・ｹ邵ｺ蠕後・郢晢ｽｬ郢ｧ・､郢晢ｽ､郢晢ｽｼ邵ｺ・ｫ髴台ｻ｣笘・ｸｺ蠑ｱ・狗ｸｺ・ｨ (aimPoint - muzzlePos) 邵ｺ遒・繝ｻ・ｻ・｢邵ｺ蜷ｶ・狗ｹ晁・縺堤ｹｧ蟶昜ｺ溽ｸｺ闊娯螺郢ｧ竏堋竏晢ｽｸ・ｸ邵ｺ・ｫwatch隴・ｽｹ陷ｷ莉｣・定抄・ｿ騾包ｽｨ

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
                areaScript.lifetime = 1.0f; // 1秒で消える
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

            if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Explosion_OnFire)
            {
                Vector3 spawnPos = muzzlePos + aimDir.normalized * 0.5f;
                float scale = GetExplosionScaleByRarity(rarity);
                float explDmg = finalDamage * (0.25f * rarity);
                SpawnExplosionArea(spawnPos, explDmg, scale);
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
            float currentReverseTime = 0f;

            // 陷ｷ繝ｻ・ｼ・ｾ邵ｺ譁絶・邵ｺ・ｫ郢ｧ・ｨ郢晁ｼ斐♂郢ｧ・ｯ郢晏現縺・ｹ晢ｽｳ郢ｧ・ｹ郢ｧ・ｿ郢晢ｽｳ郢ｧ・ｹ郢ｧ蝣､蜃ｽ隰瑚・笘・ｹｧ繝ｻ
            List<Alpha_Effect_Base> effectsToApply = new List<Alpha_Effect_Base>();
            List<Alpha.Data.ActiveWeaponEffect_Alpha> soEffectsToApply = new List<Alpha.Data.ActiveWeaponEffect_Alpha>();

            float totalHomingStrength = 0f;
            if (inventoryManager != null)
            {
                totalHomingStrength = inventoryManager.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.Homing, isBouquet ? -1 : currentWeaponGroup);
                if (totalHomingStrength > 0f)
                {
                    totalHomingStrength = Mathf.Min(totalHomingStrength, 100f); // 隴悟唱螻楢怏蟶吶・隴崢陞滂ｽｧ100
                    // 鬩幢ｽｨ闖ｴ髦ｪ繝ｻ陟托ｽｾ鬯・ｽｭ(2)邵ｲ竏墅樒ｹｧ・｢陟趣ｽｦ邵ｺ・ｯ郢敖郢晄ｺ倥・(1)邵ｺ・ｨ邵ｺ蜉ｱ窶ｻ隰・ｽｱ邵ｺ繝ｻ
                    effectsToApply.Add(new Effect_Homing_Alpha(2, 1, totalHomingStrength));
                }
            }

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
            else if (pattern == playerStatusManager_Alpha.SpawnPattern.Barrage)
            {
                float randomAngle = Random.Range(-spreadRangeDeg / 2f, spreadRangeDeg / 2f);
                spawnDir = Quaternion.Euler(0, 0, randomAngle) * aimDir;
            }
            else if (pattern == playerStatusManager_Alpha.SpawnPattern.Reverse)
            {
                // 郢ｧ・ｿ郢晢ｽｼ郢ｧ・ｲ郢昴・繝ｨ郢晢ｽｭ郢昴・縺醍ｸｺ・ｮ隴幄・笏檎ｸｺ・ｫ鬮｢・｢郢ｧ荳奇ｽ臥ｸｺ螢ｹﾂ竏ｫ貍∫ｸｺ・｣邵ｺ貊灘ｩｿ陷ｷ謇假ｽｼ蛹ｻ繝ｻ郢ｧ・ｦ郢ｧ・ｹ邵ｺ・ｾ邵ｺ貅倥・郢ｧ・ｿ郢晢ｽｼ郢ｧ・ｲ郢昴・繝ｨ繝ｻ蟲ｨ繝ｻ鬨ｾ繝ｻ竏磯具ｽｺ陝・・・�邵ｲ竏ｽ・ｸﾂ陞ｳ螢ｽ蜃ｾ鬮｢轣假ｽｾ蠕娯・隴幢ｽｬ隴夲ｽ･邵ｺ・ｮ隴・ｽｹ陷ｷ莉｣竏郁惺莉｣ﾂｰ邵ｺ繝ｻ
                float randomAngle = Random.Range(-spreadRangeDeg / 2f, spreadRangeDeg / 2f);
                spawnDir = Quaternion.Euler(0, 0, randomAngle) * (-aimDir);
                currentReverseTime = reverseTravelTimeSec;
            }
            
            if (isBouquet)
            {
                if (inventoryManager != null)
                {
                    for (int n = 0; n < 9; n++)
                    {
                        var inst = inventoryManager.Get(n % 3, n / 3);
                        if (inst.series != null)
                        {
                            // 郢昜ｻ｣繝｣郢ｧ・ｷ郢晞摩譟題ｭｫ諛翫・陋ｻ・､陞ｳ繝ｻ
                            System.Action<Alpha.Data.WeaponEffectSO_Alpha, int> addEffect = null;
                            addEffect = (effSO, rarity) =>
                            {
                                if (effSO == null) return;
                                soEffectsToApply.Add(new Alpha.Data.ActiveWeaponEffect_Alpha(effSO, rarity));
                                if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
                                {
                                    var comp = effSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                                    if (comp != null && comp.subEffects != null)
                                    {
                                        foreach (var sub in comp.subEffects) addEffect(sub, rarity);
                                    }
                                    return;
                                }
                                if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Volt)
                                {
                                    float interval = effSO.GetValue(rarity);
                                    var passiveVolt = new Effect_VoltPassive_Alpha(n % 3, rarity, interval);
                                    passiveVolt.sourceSeries = inst.series;
                                    effectsToApply.Add(passiveVolt);
                                }
                                if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Explosion)
                                {
                                    float interval = effSO.GetValue(rarity);
                                    var passiveExplosion = new Effect_ExplosionPassive_Alpha(n % 3, rarity, interval);
                                    passiveExplosion.sourceSeries = inst.series;
                                    effectsToApply.Add(passiveExplosion);
                                }
                            };

                            if (inst.series.passiveEffects != null)
                            {
                                foreach (var pe in inst.series.passiveEffects)
                                {
                                    if (pe.effect != null)
                                    {
                                        int r = pe.fixedQualityOverride > 0 ? pe.fixedQualityOverride : (inst.rarity > 0 ? inst.rarity : 1);
                                        addEffect(pe.effect, r);
                                    }
                                }
                            }
                            if (inst.currentEffects != null)
                            {
                                foreach (var effSO in inst.currentEffects)
                                {
                                    int r = inst.rarity > 0 ? inst.rarity : 1;
                                    addEffect(effSO, r);
                                }
                            }
                            if (inst.setBonusEffect != null && inventoryManager.IsGroupSeriesAligned(n / 3))
                            {
                                int r = inst.rarity > 0 ? inst.rarity : 1;
                                addEffect(inst.setBonusEffect, r);
                            }
                        }
                    }
                }
            }
            else
            {
                if (inventoryManager != null)
                {
                    for (int n = 0; n < 3; n++)
                    {
                        var inst = inventoryManager.Get(n, currentWeaponGroup);
                        if (inst.series != null)
                        {
                            // 郢昜ｻ｣繝｣郢ｧ・ｷ郢晞摩譟題ｭｫ諛翫・陋ｻ・､陞ｳ繝ｻ
                            System.Action<Alpha.Data.WeaponEffectSO_Alpha, int> addEffect = null;
                            addEffect = (effSO, rarity) =>
                            {
                                if (effSO == null) return;
                                soEffectsToApply.Add(new Alpha.Data.ActiveWeaponEffect_Alpha(effSO, rarity));
                                if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
                                {
                                    var comp = effSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                                    if (comp != null && comp.subEffects != null)
                                    {
                                        foreach (var sub in comp.subEffects) addEffect(sub, rarity);
                                    }
                                    return;
                                }
                                if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Volt)
                                {
                                    float interval = effSO.GetValue(rarity);
                                    var passiveVolt = new Effect_VoltPassive_Alpha(n, rarity, interval);
                                    passiveVolt.sourceSeries = inst.series;
                                    effectsToApply.Add(passiveVolt);
                                }
                                if (effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Explosion)
                                {
                                    float interval = effSO.GetValue(rarity);
                                    var passiveExplosion = new Effect_ExplosionPassive_Alpha(n, rarity, interval);
                                    passiveExplosion.sourceSeries = inst.series;
                                    effectsToApply.Add(passiveExplosion);
                                }
                            };

                            if (inst.series.passiveEffects != null)
                            {
                                foreach (var pe in inst.series.passiveEffects)
                                {
                                    if (pe.effect != null)
                                    {
                                        int r = pe.fixedQualityOverride > 0 ? pe.fixedQualityOverride : (inst.rarity > 0 ? inst.rarity : 1);
                                        addEffect(pe.effect, r);
                                    }
                                }
                            }
                            if (inst.currentEffects != null)
                            {
                                foreach (var effSO in inst.currentEffects)
                                {
                                    int r = inst.rarity > 0 ? inst.rarity : 1;
                                    addEffect(effSO, r);
                                }
                            }
                            if (inst.setBonusEffect != null && inventoryManager.IsGroupSeriesAligned(currentWeaponGroup))
                            {
                                int r = inst.rarity > 0 ? inst.rarity : 1;
                                addEffect(inst.setBonusEffect, r);
                            }
                        }
                    }
                }
            }

            GameObject prefabToUse = prefabs[i % prefabs.Count];
            CreateSingleBullet(prefabToUse, spawnPos, spawnDir, aimDir, currentReverseTime, finalDmg, effectsToApply, soEffectsToApply, lockedTarget, isBouquet, extraShotsForBullet, extraPierceForBullet, bulletChangeMultiplier, voltTickReduceForBullet, secondaryDamageUpForBullet);

            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝医・蜀咲函・亥ｿ・ｦ√↓蠢懊§縺ｦ・・
            // if (shootAudioSource != null) shootAudioSource.Play();

            if (pattern == playerStatusManager_Alpha.SpawnPattern.Barrage || pattern == playerStatusManager_Alpha.SpawnPattern.Reverse)
            {
                yield return new WaitForSeconds(shotIntervalSec);
            }
        }
    }

    private void CreateSingleBullet(GameObject prefabToInstantiate, Vector3 spawnPos, Vector3 spawnDir, Vector3 originalAimDir, float reverseTime, float finalDamage, List<Alpha_Effect_Base> effectsToApply, List<Alpha.Data.ActiveWeaponEffect_Alpha> soEffectsToApply, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0, float bulletChangeMultiplier = 0f, int voltTickReduceForBullet = 0, float secondaryDamageUpForBullet = 0f)
    {
        GameObject bulletPrefab;
        if (Alpha_ObjectPoolManager.Instance != null)
        {
            bulletPrefab = Alpha_ObjectPoolManager.Instance.Rent(prefabToInstantiate, spawnPos, Quaternion.identity);
        }
        else
        {
            bulletPrefab = Instantiate(prefabToInstantiate, spawnPos, Quaternion.identity);
        }

        float rotationAngle = Mathf.Atan2(spawnDir.y, spawnDir.x) * Mathf.Rad2Deg;
        bulletPrefab.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();
        
        if (bulletScript != null)
        {
            bulletScript.sourcePrefab = prefabToInstantiate;
            bulletScript.originalAimDirection = originalAimDir;
            bulletScript.reverseTimeRemaining = reverseTime;
            bulletScript.lockedTarget = lockedTarget;

            Bullet_Base prefabScript = prefabToInstantiate.GetComponent<Bullet_Base>();
            float originalSpeed = prefabScript != null ? prefabScript.Speed : bulletScript.Speed;
            float originalDestroyTime = prefabScript != null ? prefabScript.DestroyTime : bulletScript.DestroyTime;
            
            float baseBulletSpeed = playerStatusScript.bulletSpeed * playerStatusScript.bulletSpeedMag * 1.5f * (originalSpeed * 0.01f);
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

            bulletScript.SetWeaponEffects(effectsToApply, playerStatusScript.canUseAllEffects);
            bulletScript.SetWeaponEffectsSO(soEffectsToApply);

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

            // 邏ｫ縺ｮ繧ｪ繝ｼ繝ｩ・・railRenderer・峨ｒ蜍慕噪縺ｫ莉倅ｸ・
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

            bulletScript.shoot();
        }
    }
}

