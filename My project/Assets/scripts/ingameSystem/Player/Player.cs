using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private static Player _instance;
    public static Player Instance
    {
        get { return _instance; }
    }
    private Rigidbody2D rb;
    private bool isPaused = false;

    public float pow;
    public float DamageAdd = 0.0f; //バフとかで増やす値
    public float DamageMag = 1.0f; //非固定ダメージの倍率
    public float BlockDmg = 0f; //ダメージ軽減数値
    public float BlockMag = 1f; //ダメージ軽減倍率

    public float moveSpeed;
    public float moveSpeedMag = 1f;
    public float bulletSpeed;
    public float bulletSpeedMag = 1.0f;
    public float BulletSpan; // フレーム
    public float BulletSpanMag = 1.0f;
    public bool onCoolTime;
    public Vector3 watch;
    public float lockOnRadius = 5f; // ロックオンの半径
    public int Exp;

    private Transform lockOnTarget; // ロックオン対象
    private GameObject targetEnemy;
    public AudioSource shootAudioSource; // 弾の発射音用のAudioSource
    public AudioSource getExpAudioSource; // 経験値取得音用のAudioSource

    private EquipManager equipManager; // プレイヤーの装備を管理
    private Animator animator; // Animator コンポーネントを追加

    public bool UnBattle = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Animator を取得
        equipManager = GameObject.Find("GameManager").GetComponent<EquipManager>();
    }

    void Start()
    {
        onCoolTime = false;
        GameObject.Find("GameManager").GetComponent<PlayerStatusManager>().LoadStatus(gameObject);
        GameObject targetObj = GameObject.Find("PlayerUI");
        if (targetObj == null)
        {
            Debug.Log("取得できてないぞバー");
        }
        PlayerHPBar targetScr = GameObject.Find("PlayerUI").GetComponent<PlayerHPBar>();
        targetScr.playerObj = this.gameObject;
        targetScr.objectHealth = this.gameObject.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (isPaused)
        {
            rb.velocity = Vector2.zero; // ポーズ中は動きを停止
            return;
        }

        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // 近くのエネミーをロックオン
        LockOnEnemy(mousePosition);

        // ロックオン対象がいる場合、その方向に向ける
        if (lockOnTarget != null)
        {
            watch = (lockOnTarget.position - transform.position).normalized;
        }
        else
        {
            // ロックオン対象がいない場合はマウスポインタの方向に向ける
            watch = (mousePosition - transform.position).normalized;
        }

        // プレイヤーの向きを変更
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        //  transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // 弾の発射処理
        if (Time.timeScale != 0f && (Input.GetMouseButton(0) && !onCoolTime))
        {
            onCoolTime = true;
            //  ShootBullet();
            StartCoroutine(CoolTime());
        }
        // Animator パラメータの更新
        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            rb.velocity = Vector2.zero; // ポーズ中は動きを停止
            return;
        }

        // 入力がない場合は何もしない
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 入力の正規化
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }

        // キャラクターを移動させる
        float setSpd = (moveSpeed * moveSpeedMag);
        if (setSpd <= 0)
        {
            setSpd = 0.1f;
        }
        rb.velocity = input * setSpd;
    }

    private void LockOnEnemy(Vector3 mousePosition)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = lockOnRadius;
        lockOnTarget = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(mousePosition, enemy.transform.position);
            if (distance < closestDistance)
            {
                targetEnemy = enemy;
                closestDistance = distance;
                lockOnTarget = enemy.transform;
            }
        }
    }

    public GameObject getTargetEnemy()
    {
        return targetEnemy;
    }

    private IEnumerator CoolTime()
    {
        int count = 0;
        while (true)
        {
            if (count >= (BulletSpan * BulletSpanMag))
            {
                onCoolTime = false;
                yield break;
            }
            count++;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void addExp(int num)
    {
        getExpAudioSource.Play();
        Exp += num;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    private void UpdateAnimatorParameters()
    {
        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // マウス位置のX座標をAnimatorパラメータに設定
        float mouseXPosition = rb.velocity.x; // mousePosition.x;

        // プレイヤーの移動ベクトルの大きさを計算
        float moveVectorMag = rb.velocity.magnitude;

        // Animator パラメータを設定
        animator.SetFloat("mouseXPosition", mouseXPosition);
        animator.SetFloat("moveVectorMag", moveVectorMag);
    }

    void ShootBullet()
    {
        // 現在装備している弾丸、ケース、プライマーを取得
        GameObject activeBullet = equipManager.GetActiveBullet();
        GameObject activeCase = equipManager.GetActiveCase();
        GameObject activePrimer = equipManager.GetActivePrimer();
        GameObject bulletPrefab;
        float ratio = 1.5f;
        Vector3 createPos = transform.position + (watch * ratio);
        if (activeBullet == null)
        {
            Debug.LogWarning("No active bullet equipped.");
            bulletPrefab = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/NormalBullet"),
                createPos,
                Quaternion.identity
            );
        }
        else
        {
            // 弾丸の生成

            bulletPrefab = Instantiate(
                activeBullet.GetComponent<ItemPickUp>().targetObj,
                createPos,
                Quaternion.identity
            );
        }

        // 弾丸の基本ステータスを設定
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();
        if (bulletScript != null)
        {
            bulletScript.setStatus(watch, bulletSpeed, pow);
        }
        else
        {
            //何もついていないとき、通常弾生成
        }
        Case_Base caseScript;
        System.Type caseType;
        // ケースの効果を弾丸にアタッチ
        if (activeCase != null)
        {
            caseScript = activeCase.GetComponent<ItemPickUp>().targetObj.GetComponent<Case_Base>();
            caseType = caseScript.GetType();
            bulletPrefab.AddComponent(caseType);
            Debug.Log("Successfully added case script of type: " + caseType.Name);
        }
        else
        {
            //何もついてないとき、通常薬莢装備
            caseScript = Resources
                .Load<GameObject>("Objects/Case/NormalCase")
                .GetComponent<Case_Base>();
            caseType = caseScript.GetType();
            bulletPrefab.AddComponent(caseType);
        }

        // プライマーの効果を発動
        if (activePrimer != null)
        {
            Primer_Base primerScript = activePrimer
                .GetComponent<ItemPickUp>()
                .targetObj.GetComponent<Primer_Base>();
            primerScript.StrikePrimer();
        }

        // 弾丸の発射
        bulletScript?.fire();

        // サウンドエフェクトの再生
        shootAudioSource.Play();
    }

    public Vector3 getRotate()
    {
        return watch;
    }

    public void setPow(float addpoint)
    {
        pow += addpoint;
    }

    public void setShootSpeed(float addpoint)
    {
        bulletSpeed += addpoint;
    }

    public int getExp()
    {
        return Exp;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "DramaticMangaScene" || scene.name == "Title" || scene.name == "Continue")
        {
            // シーン遷移時にプレイヤーオブジェクトを非アクティブにする
            gameObject.SetActive(false);
        }
        else
        {
            // 戦闘シーンなどではプレイヤーオブジェクトをアクティブにする
            gameObject.SetActive(true);
        }
    }
}
