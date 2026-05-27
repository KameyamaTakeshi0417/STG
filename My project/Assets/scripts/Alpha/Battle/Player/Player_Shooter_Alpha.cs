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
            ShootBullet();
            StartCoroutine(CoolTime());
        }
    }

    private IEnumerator CoolTime()
    {
        // 基準の発射間隔を0.8秒に設定
        float baseInterval = 0.8f;
        // 関数の倍率を適用 (例: BulletSpanMagが100の場合は1倍、50の場合は0.5倍)
        float targetInterval = baseInterval * (playerStatusScript.BulletSpanMag * 0.01f);
        
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
            // 雷管（インデックス2＝series3）の弾プレハブを優先
            if (series3 != null && series3.bulletPrefab != null)
            {
                prefabToInstantiate = series3.bulletPrefab;
            }
            else if (series1 != null && series1.bulletPrefab != null) // フォールバック
            {
                prefabToInstantiate = series1.bulletPrefab;
            }
            else
            {
                Debug.LogWarning($"[Player_Shooter] {currentWeaponGroup + 1}段目の武器に弾プレハブが未設定です。デフォルト弾を使用します。");
            }
        }

        // --- 発射に必要な共通パラメータの計算 ---
        float finalDamage = playerStatusScript.GetFinalDamage();
        int totalShotCount = 1 + playerStatusScript.extraShotCount;
        var pattern = playerStatusScript.currentSpawnPattern;

        Vector3 aimPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        aimPoint.z = 0;
        bool isTargetLocked = false;
        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            aimPoint = pointerSystem.CurrentTarget.position;
            aimPoint.z = 0;
            isTargetLocked = true;
        }

        Vector3 muzzlePos = playerTransform.position + (watch * moveRadius);
        Vector3 aimDirection = watch; // マウスがプレイヤーに近すぎると (aimPoint - muzzlePos) が逆転するバグを防ぐため、常にwatch方向を使用

        StartCoroutine(SpawnBulletRoutine(prefabToInstantiate, muzzlePos, aimDirection, aimPoint, totalShotCount, pattern, finalDamage, isTargetLocked, isBouquet));
    }

    private IEnumerator SpawnBulletRoutine(GameObject prefab, Vector3 muzzlePos, Vector3 aimDir, Vector3 aimPoint, int shotCount, playerStatusManager_Alpha.SpawnPattern pattern, float finalDmg, bool isTargetLocked, bool isBouquet)
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
                        if (inst.series != null && !string.IsNullOrEmpty(inst.series.activeEffectClassName))
                        {
                            var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(inst.series.activeEffectClassName, n % 3, inst.rarity > 0 ? inst.rarity : 1);
                            if (ef != null) effectsToApply.Add(ef);
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
                        if (inst.series != null && !string.IsNullOrEmpty(inst.series.activeEffectClassName))
                        {
                            var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(inst.series.activeEffectClassName, n, inst.rarity > 0 ? inst.rarity : 1);
                            if (ef != null) effectsToApply.Add(ef);
                        }
                    }
                }
            }

            CreateSingleBullet(prefab, spawnPos, spawnDir, aimDir, currentReverseTime, finalDmg, effectsToApply);

            // サウンドエフェクトの再生（必要に応じて）
            // if (shootAudioSource != null) shootAudioSource.Play();

            if (pattern == playerStatusManager_Alpha.SpawnPattern.Barrage || pattern == playerStatusManager_Alpha.SpawnPattern.Reverse)
            {
                yield return new WaitForSeconds(shotIntervalSec);
            }
        }
    }

    private void CreateSingleBullet(GameObject prefabToInstantiate, Vector3 spawnPos, Vector3 spawnDir, Vector3 originalAimDir, float reverseTime, float finalDamage, List<Alpha_Effect_Base> effectsToApply)
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

            Bullet_Base prefabScript = prefabToInstantiate.GetComponent<Bullet_Base>();
            float originalSpeed = prefabScript != null ? prefabScript.Speed : bulletScript.Speed;
            float originalDestroyTime = prefabScript != null ? prefabScript.DestroyTime : bulletScript.DestroyTime;
            
            float baseBulletSpeed = playerStatusScript.bulletSpeed * playerStatusScript.bulletSpeedMag * 1.5f * (originalSpeed * 0.01f);
            
            bulletScript.setStatus(spawnDir, baseBulletSpeed, finalDamage);
            bulletScript.DestroyTime = originalDestroyTime * playerStatusScript.bulletLifeMag;

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

            bulletScript.shoot();
        }
    }
}
