using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shooter_Alpha : MonoBehaviour
{
    public bool onCoolTime;
    public AudioSource shootAudioSource; // è ‘ï½¾ç¸ºE®é€‹ï½ºèŸEEæµ¹é€•ï½¨ç¸ºE®AudioSource
    public float moveRadius = 2f; // ç¹åŠ±Îç¹§E¤ç¹ï½¤ç¹ï½¼ç¹§å‰E½¸E­è ¢ãƒ»â†’ç¸ºå¶E‹èœŠé›E½¾ãƒ»

    [Header("Weapon Settings")]
    public BASE_WeaponData_Alpha equippedWeaponData; // è¿´E¾è¨E¨é™¬ãƒ»E™ç¸ºåŠ±â€»ç¸ºãƒ»E‹è±E½¦èï½¨ç¹ãEãƒ»ç¹§E¿ãƒ»ãƒ»nspectorç¸ºä¹ï½‰ç¹§E¢ç¹§E¿ç¹ãEãƒ¡èœ¿E¯é–­E½ãƒ»ãƒ»

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

        // æ­¦å™¨ã‚°ãƒ«ãƒ¼ãƒ—ãEãƒ«ãƒ¼ãƒ—åEã‚Šæ›¿ãE
        if (Alpha.Managers.InputManager_Alpha.Instance.WasWeaponPrevPressed || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            currentWeaponGroup--;
            if (currentWeaponGroup < 0) currentWeaponGroup = 2;
            Debug.Log("Switched weapon group.");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }
        else if (Alpha.Managers.InputManager_Alpha.Instance.WasWeaponNextPressed || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            currentWeaponGroup++;
            if (currentWeaponGroup > 2) currentWeaponGroup = 0;
            Debug.Log("Switched weapon group.");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }

        // ã‚¨ã‚¤ãƒ å¯¾è±¡ã®åº§æ¨™ã‚’å–å¾E
        Vector3 aimPos = Alpha.Managers.InputManager_Alpha.Instance.GetWorldAimPosition(transform.position);

        // ç¹§E¿ç¹ï½¼ç¹§E²ç¹ãEãƒ¨ç¹ï½­ç¹ãEã‘ç¹§Eªç¹ï½³è­ã‚…ãƒ»èœE½¦é€EEE’éœ‘E½èœE
        if (pointerSystem == null) 
        {
            pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        }
        
        Vector3 direction;
        Vector3 pPos = playerTransform.position;
        pPos.z = 0; // Zè ï½§è®“å¶ãƒ»0ç¸ºE«è—ï½ºè³ãƒ»

        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            // ç¹ï½­ç¹ãEã‘ç¹§Eªç¹ï½³ç¸ºåŠ±â€»ç¸ºãƒ»E‹èŸ‡E¾é›ï½¡ç¸ºå¾Œï¼ç¹§å¾ŒãEç¸²âˆšâ—ç¸ºE®èŸE½¾é›ï½¡ç¸ºE®è­E½¹èœ·ä»£E’èœ·ä»£E¥
            Vector3 targetPos = pointerSystem.CurrentTarget.position;
            targetPos.z = 0; 
            direction = (targetPos - pPos).normalized;
        }
        else
        {
            // ç¸ºãƒ»â†‘ç¸ºä»£EŒç¸ºE°è‰ç¿«âˆªç¸ºE§é¨¾å£¹EŠç¹æ§­ãˆç¹§E¹ç¸ºE®è­E½¹èœ·ä»£E’èœ·ä»£E¥
            direction = (aimPos - pPos).normalized;
        }

        // ç™ºå°E–¹å‘ãEè¨ˆç®—ï¼ˆãEã‚¦ã‚¹æ–¹å‘ã¾ãŸãEå³ã‚¹ãƒE‚£ãƒE‚¯æ–¹å‘ï¼E
        watch = direction;
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;

        // å¼¾ã®ç™ºå°Eˆ¤å®E
        if (Time.timeScale != 0f && (Alpha.Managers.InputManager_Alpha.Instance.IsFiring && !onCoolTime))
        {
            onCoolTime = true;
            StartCoroutine(ShootAndCooldownRoutine());
        }
    }

    private IEnumerator ShootAndCooldownRoutine()
    {
        int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
        float burstInterval = 0.05f; // ç¹ï½¦ç¹ï½¼ç¹§E¶ç¹ï½¼éš•âˆµæ‚ç¸ºE«ç¹§åŒ»EE.1é˜åEâˆªç¸ºæº˜ãE2-3ç¹è¼”Îç¹ï½¼ç¹ï¿½éå¥EºE¦ç¸ºE®éï½­ç¸ºãƒ»ä¿£é««ãƒ»

        for (int i = 0; i < burstCount; i++)
        {
            ShootBullet(); // èœŠå€¡åŒ±é€‹ï½ºèŸEEE¼ãƒ»pawnBulletRoutineç¹§è²ä»–ç¸ºE³èœE½ºç¸ºå‘»E¼ãƒ»
            
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // ç¹èEãƒ»ç¹§E¹ç¹è‚²åŒ±èŸEEEµã‚E½ºãƒ»E¾å¾ŒâEç¹§E¯ç¹ï½¼ç¹ï½«ç¹§E¿ç¹§E¤ç¹ï¿½ãƒ»åŒ»Îœç¹ï½­ç¹ï½¼ç¹ä¼šï½¼å³¨E’é«¢å¥E§ä¹âEç¹§ãƒ»
        // è“ï½ºè²E‚¶ãƒ»é€‹ï½ºèŸEEä¿£é««æ–ï½E.8é˜åEâ†“éšªE­è³ãƒ»
        float baseInterval = 0.8f;
        // é«¢E¢è¬¨E°ç¸ºE®è›Ÿå’²ç´«ç¹§å¸âEé€•ï½¨ (è“ãE BulletSpanMagç¸ºãƒ»00ç¸ºE®è£E´èœ·åŒ»ãƒ»1è›Ÿé˜ªÂ€ãƒ»0ç¸ºE®è£E´èœ·åŒ»ãƒ»0.5è›ŸãE
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
        // InventoryManagerã‹ã‚‰ç¾åœ¨ã®æ­¦å™¨(y = currentWeaponGroup)ã®3ã¤ã®æ­¦å™¨ãƒEEã‚¿ã‚’å–å¾E
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

        bool isBouquet = false;
        if (inventoryManager != null) isBouquet = inventoryManager.IsBouquetActive();

        List<GameObject> prefabsToInstantiate = new List<GameObject>();
        float bulletChangeMultiplier = 0f;

        if (isBouquet)
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
            // --- æ–°ã—ã„å¼¾å„ªå…ˆåº¦ãƒ­ã‚¸ãƒE‚¯ ---
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

        // --- é€‹ï½ºèŸEEâ†“è ¢ãƒ»E¦âˆšâEèœˆï½±é¨¾å£¹ãƒ±ç¹ï½©ç¹ï½¡ç¹ï½¼ç¹§E¿ç¸ºE®éšªè‚²E®ãƒ»---
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
            bulletExtraShots = localCircularSubShots; // å°‚ç”¨ã‚¨ãƒ•ã‚§ã‚¯ãƒˆãEå­å¼¾å¢—åŠ 

            int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
            if (burstCount > 1 || totalShotCount > 1)
            {
                finalDamage *= 0.3f; // è¦ªç™ºå°E•°ãŒå¢—ãˆãŸå ´åˆãE0.3å€E
            }
        }
        else
        {
            // do nothing
        }
        Vector3 aimPoint = Alpha.Managers.InputManager_Alpha.Instance.GetWorldAimPosition(transform.position);
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
        Vector3 aimDirection = watch; // ç¹æ§­ãˆç¹§E¹ç¸ºå¾ŒãEç¹ï½¬ç¹§E¤ç¹ï½¤ç¹ï½¼ç¸ºE«éœ‘ä»£â˜E¸ºå¼±E‹ç¸ºE¨ (aimPoint - muzzlePos) ç¸ºç¢E€ãƒ»E»E¢ç¸ºå¶E‹ç¹èEã’ç¹§å¸äºŸç¸ºèˆŒâ—†ç¹§âˆšÂ€âˆï½¸E¸ç¸ºE«watchè­E½¹èœ·ä»£E’è´E¿é€•ï½¨

        StartCoroutine(SpawnBulletRoutine(prefabsToInstantiate, muzzlePos, aimDirection, aimPoint, totalShotCount, pattern, finalDamage, isTargetLocked, lockedTarget, isBouquet, bulletExtraShots, 0, bulletChangeMultiplier, localVoltTickReduce, localSecondaryDamageUp));
    }

    private IEnumerator SpawnBulletRoutine(List<GameObject> prefabs, Vector3 muzzlePos, Vector3 aimDir, Vector3 aimPoint, int shotCount, playerStatusManager_Alpha.SpawnPattern pattern, float finalDmg, bool isTargetLocked, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0, float bulletChangeMultiplier = 0f, int voltTickReduceForBullet = 0, float secondaryDamageUpForBullet = 0f)
    {
        for (int i = 0; i < shotCount; i++)
        {
            Vector3 spawnPos = muzzlePos;
            Vector3 spawnDir = aimDir;
            float currentReverseTime = 0f;

            // èœ·ãƒ»E¼E¾ç¸ºæ–âEç¸ºE«ç¹§E¨ç¹è¼”ã‰ç¹§E¯ç¹åŒ»ãE¹ï½³ç¹§E¹ç¹§E¿ç¹ï½³ç¹§E¹ç¹§å ¤å‡½è¬ŒèEâ˜E¹§ãƒ»
            List<Alpha_Effect_Base> effectsToApply = new List<Alpha_Effect_Base>();

            float totalHomingStrength = 0f;
            if (inventoryManager != null)
            {
                totalHomingStrength = inventoryManager.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.Homing, isBouquet ? -1 : currentWeaponGroup);
                if (totalHomingStrength > 0f)
                {
                    totalHomingStrength = Mathf.Min(totalHomingStrength, 100f); // è­Œå¥å±“èœ‰å¸™ãEè­›Â€èŸï½§100
                    // é©›ï½¨è´é˜ªãƒ»è ‘ï½¾é¬E½­(2)ç¸²âˆšÎç¹§E¢è ï½¦ç¸ºE¯ç¹Â€ç¹æº˜ãE(1)ç¸ºE¨ç¸ºåŠ±â€»è¬E½±ç¸ºãƒ»
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
                // ç¹§E¿ç¹ï½¼ç¹§E²ç¹ãEãƒ¨ç¹ï½­ç¹ãEã‘ç¸ºE®è­›èEâ”Œç¸ºE«é«¢E¢ç¹§ä¸Šï½‰ç¸ºå£¹Â€âˆ«æ¼ç¸ºE£ç¸ºæ»“å©¿èœ·æ‰˜ï½¼åŒ»ãƒ»ç¹§E¦ç¹§E¹ç¸ºE¾ç¸ºæº˜ãEç¹§E¿ç¹ï½¼ç¹§E²ç¹ãEãƒ¨ãƒ»å³¨ãƒ»é¨¾ãƒ»âˆˆé€‹ï½ºèŸEEE ç¸²âˆ½E¸Â€è³å£½å‡¾é«¢ç˜ï½¾å¾ŒâEè­›ï½¬è­šï½¥ç¸ºE®è­E½¹èœ·ä»£âˆˆèœ·ä»£Â°ç¸ºãƒ»
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
                            
                            // ç¹ä»£ãƒ£ç¹§E·ç¹é–€æŸ‘è­«æ‡ŠãEè›»E¤è³ãƒ»
                            if (inst.currentEffects != null)
                            {
                                foreach (var effSO in inst.currentEffects)
                                {
                                    if (effSO != null && effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Volt)
                                    {
                                        float interval = effSO.GetValue(inst.rarity);
                                        // é˜®E¬é—”ï½¢ç¸ºE®é©›ï½¨è´ãƒ»1)ç¸ºE¨ç¸ºåŠ±â€»è¬E½±ç¸ºãƒ»Â°ç¸²âˆ«æ¨Ÿè¨E¨ç¸ºE®é™¬ãƒ»E™é‚‚ãƒ»åœEn % 3)ç¸ºE¨ç¸ºåŠ±â€»è¬E½±ç¸ºãƒ»Â°
                                        // é—Šï½ªé™¦å¾¡E¸E­ç¸ºE«é—œï½½ç¸ºE¨ç¸ºåŠ±â—E¸ºãƒ»ãƒ»ç¸ºE§é©›ï½¨è´é˜ªâ†“é«¢E¢ç¹§ä¸Šï½‰ç¸ºå£¹ãƒ±ç¹ãEã™ç¹æ‚¶â†’ç¸ºåŠ±â€»éœ‘ï½½èœ‰ï¿½
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

                            // ç¹ä»£ãƒ£ç¹§E·ç¹é–€æŸ‘è­«æ‡ŠãEè›»E¤è³ãƒ»
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

            GameObject prefabToUse = prefabs[i % prefabs.Count];
            CreateSingleBullet(prefabToUse, spawnPos, spawnDir, aimDir, currentReverseTime, finalDmg, effectsToApply, lockedTarget, isBouquet, extraShotsForBullet, extraPierceForBullet, bulletChangeMultiplier, voltTickReduceForBullet, secondaryDamageUpForBullet);

            // ã‚µã‚¦ãƒ³ãƒ‰ã‚¨ãƒ•ã‚§ã‚¯ãƒˆãEå†ç”ŸEˆå¿E¦ã«å¿œã˜ã¦EE
            // if (shootAudioSource != null) shootAudioSource.Play();

            if (pattern == playerStatusManager_Alpha.SpawnPattern.Barrage || pattern == playerStatusManager_Alpha.SpawnPattern.Reverse)
            {
                yield return new WaitForSeconds(shotIntervalSec);
            }
        }
    }

    private void CreateSingleBullet(GameObject prefabToInstantiate, Vector3 spawnPos, Vector3 spawnDir, Vector3 originalAimDir, float reverseTime, float finalDamage, List<Alpha_Effect_Base> effectsToApply, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0, float bulletChangeMultiplier = 0f, int voltTickReduceForBullet = 0, float secondaryDamageUpForBullet = 0f)
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

            // ç´«ã®ã‚ªãƒ¼ãƒ©EErailRendererE‰ã‚’å‹•çš„ã«ä»˜ä¸E
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

