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
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            currentWeaponGroup++;
            if (currentWeaponGroup > 2) currentWeaponGroup = 0;
            Debug.Log($"[Player_Shooter] 武器グループが {currentWeaponGroup + 1}段目 に切り替わりました。");
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
        GameObject bulletPrefab;
        float distance = moveRadius;
        Vector3 createPos = playerTransform.position + (watch * distance);
        Vector3 NcreatePos = Vector3.Normalize(watch);
        
        // InventoryManagerから現在のグループ(y = currentWeaponGroup)の3つの武器データを取得
        Alpha.Data.WeaponSeriesData_Alpha series1 = null;
        Alpha.Data.WeaponSeriesData_Alpha series2 = null;
        Alpha.Data.WeaponSeriesData_Alpha series3 = null;
        
        int rarity1 = 1;
        int rarity2 = 1;
        int rarity3 = 1;
        
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

        GameObject prefabToInstantiate = Resources.Load<GameObject>("Objects/Bullet/NormalBullet");
        
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

        // --- 追加: ObjectPoolManager を使って弾を取り出す ---
        if (Alpha_ObjectPoolManager.Instance != null)
        {
            bulletPrefab = Alpha_ObjectPoolManager.Instance.Rent(
                prefabToInstantiate,
                createPos,
                Quaternion.identity
            );
        }
        else
        {
            // マネージャーが無い場合は直接生成（保険）
            bulletPrefab = Instantiate(
                prefabToInstantiate,
                createPos,
                Quaternion.identity
            );
        }

        // 弾の向きを変更
        float rotationAngle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        bulletPrefab.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();
        
        // --- 追加: 自分が生まれたプレハブを記憶させる（Return時に必要なため） ---
        if (bulletScript != null)
        {
            bulletScript.sourcePrefab = prefabToInstantiate;
        }
        
        // 弾のステータス（角度、弾速、ダメージ）を設定
        // 関数が干渉する前の弾速の基本スピードを今の1.5倍にする
        // さらに、弾オブジェクト固有のSpeedパラメータ（100基準）を0.01倍して反映する
        float baseBulletSpeed = playerStatusScript.bulletSpeed * 1.5f * (bulletScript.Speed * 0.01f);
        bulletScript.setStatus(watch, baseBulletSpeed, playerStatusScript.pow);

        // 各武器の効果を実体化して弾に渡す
        List<Alpha_Effect_Base> effectsToApply = new List<Alpha_Effect_Base>();

        if (series1 != null && !string.IsNullOrEmpty(series1.activeEffectClassName))
        {
            var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(series1.activeEffectClassName, 0, rarity1);
            if (ef != null) effectsToApply.Add(ef);
        }
        if (series2 != null && !string.IsNullOrEmpty(series2.activeEffectClassName))
        {
            var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(series2.activeEffectClassName, 1, rarity2);
            if (ef != null) effectsToApply.Add(ef);
        }
        if (series3 != null && !string.IsNullOrEmpty(series3.activeEffectClassName))
        {
            var ef = Alpha.Battle.Bullet.EffectFactory_Alpha.CreateEffect(series3.activeEffectClassName, 2, rarity3);
            if (ef != null) effectsToApply.Add(ef);
        }

        bulletScript.SetWeaponEffects(effectsToApply, playerStatusScript.canUseAllEffects);

        // ※ 追加: PlayerBulletManager_Alphaの貫通回数を弾に上乗せする
        PlayerBulletManager_Alpha bulletManager = null;
        GameObject manager = GameObject.Find("manager");
        if (manager != null) bulletManager = manager.GetComponent<PlayerBulletManager_Alpha>();
        if (bulletManager == null) bulletManager = FindObjectOfType<PlayerBulletManager_Alpha>(); // managerにいなかった場合用

        if (bulletManager != null)
        {
            // プレハブの設定値(Awake) ＋ マネージャーの指定値
            bulletScript.piercingCount += bulletManager.pierceCount;
        }

        bulletScript.shoot();



        //ドレイン効果が付与できるなら付与する
        DrainHandler targetHandler = GetComponent<DrainHandler>();
        // サウンドエフェクトの再生
       // shootAudioSource.Play();
    }
}
