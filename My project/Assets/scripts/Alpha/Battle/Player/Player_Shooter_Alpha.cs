using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shooter_Alpha : MonoBehaviour
{
    public bool onCoolTime;
    public AudioSource shootAudioSource; // 蠑ｾ縺ｮ逋ｺ蟆・浹逕ｨ縺ｮAudioSource
    public float moveRadius = 2f; // 繝励Ξ繧､繝､繝ｼ繧剃ｸｭ蠢・→縺吶ｋ蜊雁ｾ・

    [Header("Weapon Settings")]
    public BASE_WeaponData_Alpha equippedWeaponData; // 迴ｾ蝨ｨ陬・ｙ縺励※縺・ｋ豁ｦ蝎ｨ繝・・繧ｿ・・nspector縺九ｉ繧｢繧ｿ繝・メ蜿ｯ閭ｽ・・

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
    }

    void Start()
    {
        onCoolTime = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f || isPaused)
            return;
        if (isPaused)
        {
            return;
        }

        // 豁ｦ蝎ｨ繧ｰ繝ｫ繝ｼ繝怜・繧頑崛縺・(1繧ｭ繝ｼ: 蜑阪・陦・ 3繧ｭ繝ｼ: 谺｡縺ｮ陦・
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            currentWeaponGroup--;
            if (currentWeaponGroup < 0) currentWeaponGroup = 2;
            Debug.Log("Switched weapon group.");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            currentWeaponGroup++;
            if (currentWeaponGroup > 2) currentWeaponGroup = 0;
            Debug.Log("Switched weapon group.");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }

        // 繝槭え繧ｹ縺ｮ菴咲ｽｮ繧貞叙蠕・
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z蠎ｧ讓吶・0縺ｫ蝗ｺ螳・

        // 繧ｿ繝ｼ繧ｲ繝・ヨ繝ｭ繝・け繧ｪ繝ｳ譎ゅ・蜃ｦ逅・ｒ霑ｽ蜉�
        if (pointerSystem == null) 
        {
            pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        }
        
        Vector3 direction;
        Vector3 pPos = playerTransform.position;
        pPos.z = 0; // Z蠎ｧ讓吶・0縺ｫ蝗ｺ螳・

        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            // 繝ｭ繝・け繧ｪ繝ｳ縺励※縺・ｋ蟇ｾ雎｡縺後＞繧後・縲√◎縺ｮ蟇ｾ雎｡縺ｮ譁ｹ蜷代ｒ蜷代￥
            Vector3 targetPos = pointerSystem.CurrentTarget.position;
            targetPos.z = 0; 
            direction = (targetPos - pPos).normalized;
        }
        else
        {
            // 縺・↑縺代ｌ縺ｰ莉翫∪縺ｧ騾壹ｊ繝槭え繧ｹ縺ｮ譁ｹ蜷代ｒ蜷代￥
            direction = (mousePosition - pPos).normalized;
        }

        // 繧ｪ繝悶ず繧ｧ繧ｯ繝医・蜷代″繧偵・繧ｦ繧ｹ繝昴う繝ｳ繧ｿ・医∪縺溘・繧ｿ繝ｼ繧ｲ繝・ヨ・峨・譁ｹ蜷代↓蜷代￠繧・
        watch = direction;
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;

        // 蠑ｾ縺ｮ逋ｺ蟆・・逅・
        if (Time.timeScale != 0f && (Input.GetMouseButton(0) && !onCoolTime))
        {
            onCoolTime = true;
            StartCoroutine(ShootAndCooldownRoutine());
        }
    }

    private IEnumerator ShootAndCooldownRoutine()
    {
        int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
        float burstInterval = 0.05f; // 繝ｦ繝ｼ繧ｶ繝ｼ隕∵悍縺ｫ繧医ｊ0.1遘偵∪縺溘・2-3繝輔Ξ繝ｼ繝�遞句ｺｦ縺ｮ遏ｭ縺・俣髫・

        for (int i = 0; i < burstCount; i++)
        {
            ShootBullet(); // 蜊倡匱逋ｺ蟆・ｼ・pawnBulletRoutine繧貞他縺ｳ蜃ｺ縺呻ｼ・
            
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // 繝舌・繧ｹ繝育匱蟆・ｵゆｺ・ｾ後↓繧ｯ繝ｼ繝ｫ繧ｿ繧､繝�・医Μ繝ｭ繝ｼ繝会ｼ峨ｒ髢句ｧ九☆繧・
        // 蝓ｺ貅悶・逋ｺ蟆・俣髫斐ｒ0.8遘偵↓險ｭ螳・
        float baseInterval = 0.8f;
        // 髢｢謨ｰ縺ｮ蛟咲紫繧帝←逕ｨ (萓・ BulletSpanMag縺・00縺ｮ蝣ｴ蜷医・1蛟阪・0縺ｮ蝣ｴ蜷医・0.5蛟・
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

    void ShootBullet()
    {
        // InventoryManager縺九ｉ迴ｾ蝨ｨ縺ｮ繧ｰ繝ｫ繝ｼ繝・y = currentWeaponGroup)縺ｮ3縺､縺ｮ豁ｦ蝎ｨ繝・・繧ｿ繧貞叙蠕・
        Alpha.Data.WeaponSeriesData_Alpha series1 = null;
        Alpha.Data.WeaponSeriesData_Alpha series2 = null;
        Alpha.Data.WeaponSeriesData_Alpha series3 = null;
        
        int rarity1 = 1, rarity2 = 1, rarity3 = 1;
        
        if (inventoryManager != null)
        {
            var inst1 = inventoryManager.Get(0, currentWeaponGroup);
            var inst2 = inventoryManager.Get(1, currentWeaponGroup);
            var inst3 = inventoryManager.Get(2, currentWeaponGroup);
            
            series1 = inst1.series;
            rarity1 = inst1.rarity > 0 ? inst1.rarity : 1;
            
            series2 = inst2.series;
            rarity2 = inst2.rarity > 0 ? inst2.rarity : 1;
            
            series3 = inst3.series;
            rarity3 = inst3.rarity > 0 ? inst3.rarity : 1;
        }

        bool isBouquet = false;
        if (inventoryManager != null) isBouquet = inventoryManager.IsBouquetActive();

        GameObject prefabToInstantiate = Resources.Load<GameObject>("Objects/Bullet/NormalBullet");
        
        if (isBouquet && bouquetBulletPrefab != null)
        {
            prefabToInstantiate = bouquetBulletPrefab;
        }
        else
        {
            // --- 譯・: 繧ｵ繝ｼ繧ｭ繝･繝ｩ繝ｼ蠑ｾ縺ｮ蜆ｪ蜈亥・逅・---
            // 縺ｩ縺ｮ驛ｨ菴阪↓陬・ｙ縺輔ｌ縺ｦ縺・※繧ゅ√・繝ｬ繝上ヶ縺ｫCircularObject縺後い繧ｿ繝・メ縺輔ｌ縺ｦ縺・ｌ縺ｰ譛蜆ｪ蜈医→縺吶ｋ
            bool circularFound = false;
            if (series3 != null && series3.bulletPrefab != null && series3.bulletPrefab.GetComponent<CircularObject>() != null)
            {
                prefabToInstantiate = series3.bulletPrefab;
                circularFound = true;
            }
            else if (series2 != null && series2.bulletPrefab != null && series2.bulletPrefab.GetComponent<CircularObject>() != null)
            {
                prefabToInstantiate = series2.bulletPrefab;
                circularFound = true;
            }
            else if (series1 != null && series1.bulletPrefab != null && series1.bulletPrefab.GetComponent<CircularObject>() != null)
            {
                prefabToInstantiate = series1.bulletPrefab;
                circularFound = true;
            }

            // 繧ｵ繝ｼ繧ｭ繝･繝ｩ繝ｼ蠑ｾ縺瑚ｦ九▽縺九ｉ縺ｪ縺九▲縺溷�ｴ蜷医・騾壼ｸｸ縺ｮ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ蜃ｦ逅・
            if (!circularFound)
            {
                // 蠑ｾ鬆ｭ・医う繝ｳ繝・ャ繧ｯ繧ｹ2縲《eries3・峨ｒ譛蜆ｪ蜈・
                if (series3 != null && series3.bulletPrefab != null)
                {
                    prefabToInstantiate = series3.bulletPrefab;
                }
                else if (series2 != null && series2.bulletPrefab != null) // 阮ｬ闔｢(繧､繝ｳ繝・ャ繧ｯ繧ｹ1)縺ｮ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ繧定ｿｽ蜉�
                {
                    prefabToInstantiate = series2.bulletPrefab;
                }
                else if (series1 != null && series1.bulletPrefab != null) // 髮ｷ邂｡(繧､繝ｳ繝・ャ繧ｯ繧ｹ0)縺ｮ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
                {
                    prefabToInstantiate = series1.bulletPrefab;
                }
                else
                {
                    Debug.LogWarning($"[Player_Shooter] {currentWeaponGroup + 1}段目の武器に弾プレハブが未設定です。デフォルト弾を使用します。");
                }
            }
        }

        // 豁ｦ蝎ｨ・医・繝ｬ繝上ヶ・峨・蝓ｺ譛ｬ繝€繝｡繝ｼ繧ｸ繧貞叙蠕・
        float baseWeaponDamage = 0f;
        if (prefabToInstantiate != null)
        {
            Bullet_Base baseBullet = prefabToInstantiate.GetComponent<Bullet_Base>();
            if (baseBullet != null)
            {
                baseWeaponDamage = baseBullet.dmg;
            }
        }

        // --- 逋ｺ蟆・↓蠢・ｦ√↑蜈ｱ騾壹ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｮ險育ｮ・---
        float finalDamage = playerStatusScript.GetFinalDamage(baseWeaponDamage);
        int totalShotCount = 1 + playerStatusScript.extraShotCount;
        var pattern = playerStatusScript.currentSpawnPattern;

        int localExtraShots = 0;
        int localExtraPierce = 0;

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
                            if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.ShotCountPlus)
                                localExtraShots += (int)eff.GetValue(rarity);
                            else if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.PierceCountPlus)
                                localExtraPierce += (int)eff.GetValue(rarity);
                        }
                    }
                }
            }
        }

        int bulletExtraShots = 0;
        if (prefabToInstantiate != null && prefabToInstantiate.GetComponent<CircularObject>() != null)
        {
            bulletExtraShots = localExtraShots + playerStatusScript.extraShotCount;
            totalShotCount = 1; // 繧ｵ繝ｼ繧ｭ繝･繝ｩ繝ｼ閾ｪ菴薙・1縺､縺�縺代せ繝昴・繝ｳ縺吶ｋ
        }
        else
        {
            totalShotCount += localExtraShots;
        }

        Vector3 aimPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
        Vector3 aimDirection = watch; // 繝槭え繧ｹ縺後・繝ｬ繧､繝､繝ｼ縺ｫ霑代☆縺弱ｋ縺ｨ (aimPoint - muzzlePos) 縺碁・ｻ｢縺吶ｋ繝舌げ繧帝亟縺舌◆繧√∝ｸｸ縺ｫwatch譁ｹ蜷代ｒ菴ｿ逕ｨ

        StartCoroutine(SpawnBulletRoutine(prefabToInstantiate, muzzlePos, aimDirection, aimPoint, totalShotCount, pattern, finalDamage, isTargetLocked, lockedTarget, isBouquet, bulletExtraShots, localExtraPierce));
    }

    private IEnumerator SpawnBulletRoutine(GameObject prefab, Vector3 muzzlePos, Vector3 aimDir, Vector3 aimPoint, int shotCount, playerStatusManager_Alpha.SpawnPattern pattern, float finalDmg, bool isTargetLocked, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0)
    {
        for (int i = 0; i < shotCount; i++)
        {
            Vector3 spawnPos = muzzlePos;
            Vector3 spawnDir = aimDir;
            float currentReverseTime = 0f;

            // 蜷・ｼｾ縺斐→縺ｫ繧ｨ繝輔ぉ繧ｯ繝医う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧堤函謌舌☆繧・
            List<Alpha_Effect_Base> effectsToApply = new List<Alpha_Effect_Base>();

            float totalHomingStrength = 0f;
            if (inventoryManager != null)
            {
                totalHomingStrength = inventoryManager.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.Homing, isBouquet ? -1 : currentWeaponGroup);
                if (totalHomingStrength > 0f)
                {
                    totalHomingStrength = Mathf.Min(totalHomingStrength, 100f); // 譌句屓蜉帙・譛螟ｧ100
                    // 驛ｨ菴阪・蠑ｾ鬆ｭ(2)縲√Ξ繧｢蠎ｦ縺ｯ繝繝溘・(1)縺ｨ縺励※謇ｱ縺・
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
                // 繧ｿ繝ｼ繧ｲ繝・ヨ繝ｭ繝・け縺ｮ譛臥┌縺ｫ髢｢繧上ｉ縺壹∫漁縺｣縺滓婿蜷托ｼ医・繧ｦ繧ｹ縺ｾ縺溘・繧ｿ繝ｼ繧ｲ繝・ヨ・峨・騾・∈逋ｺ蟆・＠縲∽ｸ螳壽凾髢灘ｾ後↓譛ｬ譚･縺ｮ譁ｹ蜷代∈蜷代°縺・
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
                            if (!string.IsNullOrEmpty(inst.series.activeEffectClassName))
                            {
                                var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(inst.series.activeEffectClassName, n % 3, inst.rarity > 0 ? inst.rarity : 1);
                                if (ef != null) 
                                {
                                    ef.sourceSeries = inst.series;
                                    effectsToApply.Add(ef);
                                }
                            }
                            
                            // 繝代ャ繧ｷ繝門柑譫懊・蛻､螳・
                            if (inst.currentEffects != null)
                            {
                                foreach (var effSO in inst.currentEffects)
                                {
                                    if (effSO != null && effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Volt)
                                    {
                                        float interval = effSO.GetValue(inst.rarity);
                                        // 阮ｬ闔｢縺ｮ驛ｨ菴・1)縺ｨ縺励※謇ｱ縺・°縲∫樟蝨ｨ縺ｮ陬・ｙ邂・園(n % 3)縺ｨ縺励※謇ｱ縺・°
                                        // 闊ｪ陦御ｸｭ縺ｫ關ｽ縺ｨ縺励◆縺・・縺ｧ驛ｨ菴阪↓髢｢繧上ｉ縺壹ヱ繝・す繝悶→縺励※霑ｽ蜉�
                                        var passiveVolt = new Effect_VoltPassive_Alpha(n % 3, inst.rarity > 0 ? inst.rarity : 1, interval);
                                        passiveVolt.sourceSeries = inst.series;
                                        effectsToApply.Add(passiveVolt);
                                    }
                                    if (effSO != null && effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Explosion)
                                    {
                                        float interval = effSO.GetValue(inst.rarity);
                                        var passiveExplosion = new Effect_ExplosionPassive_Alpha(n % 3, inst.rarity > 0 ? inst.rarity : 1, interval);
                                        passiveExplosion.sourceSeries = inst.series;
                                        effectsToApply.Add(passiveExplosion);
                                    }
                                }
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
                            Debug.Log("Switched weapon group.");
                            if (!string.IsNullOrEmpty(inst.series.activeEffectClassName))
                            {
                                var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(inst.series.activeEffectClassName, n, inst.rarity > 0 ? inst.rarity : 1);
                                if (ef != null) 
                                {
                                    ef.sourceSeries = inst.series;
                                    effectsToApply.Add(ef);
                                }
                            }

                            // 繝代ャ繧ｷ繝門柑譫懊・蛻､螳・
                            if (inst.currentEffects != null)
                            {
                                foreach (var effSO in inst.currentEffects)
                                {
                                    if (effSO != null && effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Volt)
                                    {
                                        float interval = effSO.GetValue(inst.rarity);
                                        var passiveVolt = new Effect_VoltPassive_Alpha(n, inst.rarity > 0 ? inst.rarity : 1, interval);
                                        passiveVolt.sourceSeries = inst.series;
                                        effectsToApply.Add(passiveVolt);
                                    }
                                    if (effSO != null && effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Explosion)
                                    {
                                        float interval = effSO.GetValue(inst.rarity);
                                        var passiveExplosion = new Effect_ExplosionPassive_Alpha(n, inst.rarity > 0 ? inst.rarity : 1, interval);
                                        passiveExplosion.sourceSeries = inst.series;
                                        effectsToApply.Add(passiveExplosion);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            CreateSingleBullet(prefab, spawnPos, spawnDir, aimDir, currentReverseTime, finalDmg, effectsToApply, lockedTarget, extraShotsForBullet, extraPierceForBullet);

            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝医・蜀咲函・亥ｿ・ｦ√↓蠢懊§縺ｦ・・
            // if (shootAudioSource != null) shootAudioSource.Play();

            if (pattern == playerStatusManager_Alpha.SpawnPattern.Barrage || pattern == playerStatusManager_Alpha.SpawnPattern.Reverse)
            {
                yield return new WaitForSeconds(shotIntervalSec);
            }
        }
    }

    private void CreateSingleBullet(GameObject prefabToInstantiate, Vector3 spawnPos, Vector3 spawnDir, Vector3 originalAimDir, float reverseTime, float finalDamage, List<Alpha_Effect_Base> effectsToApply, Transform lockedTarget, int extraShotsForBullet = 0, int extraPierceForBullet = 0)
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
            
            bulletScript.setStatus(spawnDir, baseBulletSpeed, finalDamage);
            bulletScript.DestroyTime = originalDestroyTime * playerStatusScript.bulletLifeMag;

            if (playerStatusScript != null && playerStatusScript.ignorePierceDecay)
            {
                bulletScript.localPierceDamageReductionRate = 0f;
            }
            else
            {
                bulletScript.localPierceDamageReductionRate = -1f;
            }

            bulletScript.SetWeaponEffects(effectsToApply, playerStatusScript.canUseAllEffects);

            PlayerBulletManager_Alpha bulletManager = null;
            GameObject manager = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
            if (manager != null) bulletManager = manager.GetComponent<PlayerBulletManager_Alpha>();
            if (bulletManager == null) bulletManager = FindObjectOfType<PlayerBulletManager_Alpha>();
            
            if (bulletManager != null)
            {
                bulletScript.piercingCount += bulletManager.pierceCount;
            }

            bulletScript.piercingCount += playerStatusScript.extraPierceCount;
            bulletScript.piercingCount += extraPierceForBullet;
            bulletScript.extraShotCount += extraShotsForBullet;

            bulletScript.shoot();
        }
    }
}

