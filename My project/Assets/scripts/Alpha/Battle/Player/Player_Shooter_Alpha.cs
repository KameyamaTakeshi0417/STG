using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shooter_Alpha : MonoBehaviour
{
    public bool onCoolTime;
    public AudioSource shootAudioSource; // 弾の発射音用のAudioSource
    public float moveRadius = 2f; // プレイヤーを中心とする半径

    [Header("Weapon Settings")]
    public BASE_WeaponData_Alpha equippedWeaponData; // 現在装備している武器データ（Inspectorからアタッチ可能）

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
            playerStatusScript = GameObject.Find("manager").GetComponent<playerStatusManager_Alpha>();
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

        // 武器グループ切り替え (1キー: 前の行, 3キー: 次の行)
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            currentWeaponGroup--;
            if (currentWeaponGroup < 0) currentWeaponGroup = 2;
            Debug.Log($"[Player_Shooter] 武器グループが {currentWeaponGroup + 1}段目 に切り替わりました。");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            currentWeaponGroup++;
            if (currentWeaponGroup > 2) currentWeaponGroup = 0;
            Debug.Log($"[Player_Shooter] 武器グループが {currentWeaponGroup + 1}段目 に切り替わりました。");
            if (playerStatusScript != null) playerStatusScript.UpdateEquipmentBuffs();
        }

        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // ターゲットロックオン時の処理を追加
        if (pointerSystem == null) 
        {
            pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        }
        
        Vector3 direction;
        Vector3 pPos = playerTransform.position;
        pPos.z = 0; // Z座標は0に固定

        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            // ロックオンしている対象がいれば、その対象の方向を向く
            Vector3 targetPos = pointerSystem.CurrentTarget.position;
            targetPos.z = 0; 
            direction = (targetPos - pPos).normalized;
        }
        else
        {
            // いなければ今まで通りマウスの方向を向く
            direction = (mousePosition - pPos).normalized;
        }

        // オブジェクトの向きをマウスポインタ（またはターゲット）の方向に向ける
        watch = direction;
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;

        // 弾の発射処理
        if (Time.timeScale != 0f && (Input.GetMouseButton(0) && !onCoolTime))
        {
            onCoolTime = true;
            StartCoroutine(ShootAndCooldownRoutine());
        }
    }

    private IEnumerator ShootAndCooldownRoutine()
    {
        int burstCount = playerStatusScript != null ? Mathf.Max(1, playerStatusScript.burstCount) : 1;
        float burstInterval = 0.05f; // ユーザー要望により0.1秒または2-3フレーム程度の短い間隔

        for (int i = 0; i < burstCount; i++)
        {
            ShootBullet(); // 単発発射（SpawnBulletRoutineを呼び出す）
            
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // バースト発射終了後にクールタイム（リロード）を開始する
        // 基準の発射間隔を0.8秒に設定
        float baseInterval = 0.8f;
        // 関数の倍率を適用 (例: BulletSpanMagが100の場合は1倍、50の場合は0.5倍)
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
        // InventoryManagerから現在のグループ(y = currentWeaponGroup)の3つの武器データを取得
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
            // --- 案1: サーキュラー弾の優先処理 ---
            // どの部位に装備されていても、プレハブにCircularObjectがアタッチされていれば最優先とする
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

            // サーキュラー弾が見つからなかった場合の通常のフォールバック処理
            if (!circularFound)
            {
                // 弾頭（インデックス2、series3）を最優先
                if (series3 != null && series3.bulletPrefab != null)
                {
                    prefabToInstantiate = series3.bulletPrefab;
                }
                else if (series2 != null && series2.bulletPrefab != null) // 薬莢(インデックス1)のフォールバックを追加
                {
                    prefabToInstantiate = series2.bulletPrefab;
                }
                else if (series1 != null && series1.bulletPrefab != null) // 雷管(インデックス0)のフォールバック
                {
                    prefabToInstantiate = series1.bulletPrefab;
                }
                else
                {
                    Debug.LogWarning($"[Player_Shooter] {currentWeaponGroup + 1}段目の武器に弾プレハブが未設定です。デフォルト弾を使用します。");
                }
            }
        }

        // 武器（プレハブ）の基本ダメージを取得
        float baseWeaponDamage = 0f;
        if (prefabToInstantiate != null)
        {
            Bullet_Base baseBullet = prefabToInstantiate.GetComponent<Bullet_Base>();
            if (baseBullet != null)
            {
                baseWeaponDamage = baseBullet.dmg;
            }
        }

        // --- 発射に必要な共通パラメータの計算 ---
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
            totalShotCount = 1; // サーキュラー自体は1つだけスポーンする
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
        Vector3 aimDirection = watch; // マウスがプレイヤーに近すぎると (aimPoint - muzzlePos) が逆転するバグを防ぐため、常にwatch方向を使用

        StartCoroutine(SpawnBulletRoutine(prefabToInstantiate, muzzlePos, aimDirection, aimPoint, totalShotCount, pattern, finalDamage, isTargetLocked, lockedTarget, isBouquet, bulletExtraShots, localExtraPierce));
    }

    private IEnumerator SpawnBulletRoutine(GameObject prefab, Vector3 muzzlePos, Vector3 aimDir, Vector3 aimPoint, int shotCount, playerStatusManager_Alpha.SpawnPattern pattern, float finalDmg, bool isTargetLocked, Transform lockedTarget, bool isBouquet, int extraShotsForBullet = 0, int extraPierceForBullet = 0)
    {
        for (int i = 0; i < shotCount; i++)
        {
            Vector3 spawnPos = muzzlePos;
            Vector3 spawnDir = aimDir;
            float currentReverseTime = 0f;

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
                if (isTargetLocked)
                {
                    float randomAngle = Random.Range(-spreadRangeDeg / 2f, spreadRangeDeg / 2f);
                    spawnDir = Quaternion.Euler(0, 0, randomAngle) * (-aimDir);
                    currentReverseTime = reverseTravelTimeSec;
                }
                else
                {
                    // ターゲットロックされていない場合は単純に真っすぐ航行
                    spawnDir = aimDir;
                    currentReverseTime = 0f;
                }
            }

            // 各弾ごとにエフェクトインスタンスを生成する
            List<Alpha_Effect_Base> effectsToApply = new List<Alpha_Effect_Base>();
            
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
                            
                            // パッシブ効果の判定
                            if (inst.currentEffects != null)
                            {
                                foreach (var effSO in inst.currentEffects)
                                {
                                    if (effSO != null && effSO.effectType == Alpha.Data.WeaponEffectType_Alpha.AddActiveEffect_Volt)
                                    {
                                        float interval = effSO.GetValue(inst.rarity);
                                        // 薬莢の部位(1)として扱うか、現在の装備箇所(n % 3)として扱うか
                                        // 航行中に落としたいので部位に関わらずパッシブとして追加
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
                            Debug.Log($"[Player_Shooter] Slot {n} has series: {inst.series.name}");
                            if (!string.IsNullOrEmpty(inst.series.activeEffectClassName))
                            {
                                var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(inst.series.activeEffectClassName, n, inst.rarity > 0 ? inst.rarity : 1);
                                if (ef != null) 
                                {
                                    ef.sourceSeries = inst.series;
                                    effectsToApply.Add(ef);
                                }
                            }

                            // パッシブ効果の判定
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

            // サウンドエフェクトの再生（必要に応じて）
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
            GameObject manager = GameObject.Find("manager");
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
