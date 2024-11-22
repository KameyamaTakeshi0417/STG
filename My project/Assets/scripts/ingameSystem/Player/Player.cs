using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player instance; // Singletonインスタンス
    private Rigidbody2D rb;
    private bool isPaused = false;

    public float HP;
    public float currentHP;
    public float pow;
    public float DamageMag = 1.0f; //非固定ダメージの倍率

    public float moveSpeed;
    public float bulletSpeed;
    public float BulletSpan; // フレーム
    public bool onCoolTime;
    public Vector3 watch;
    public float lockOnRadius = 5f; // ロックオンの半径
    public int Exp;

    private Transform lockOnTarget; // ロックオン対象
    private GameObject targetEnemy;
    public AudioSource shootAudioSource; // 弾の発射音用のAudioSource
    public AudioSource getExpAudioSource; // 経験値取得音用のAudioSource

    private EquipManager equipManager; // プレイヤーの装備を管理

    void Awake()
    {
        // Singletonパターンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでもオブジェクトを破棄しない
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 既存のインスタンスがある場合、新しいインスタンスを破棄
        }

        rb = GetComponent<Rigidbody2D>();
        equipManager = GameObject.Find("GameManager").GetComponent<EquipManager>();
    }

    void Start()
    {
        onCoolTime = false;
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
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // 弾の発射処理
        if (Time.timeScale != 0f && (Input.GetMouseButton(0) && !onCoolTime))
        {
            onCoolTime = true;
            ShootBullet();
            StartCoroutine(CoolTime());
        }
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
        rb.velocity = input * moveSpeed;
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
            if (count >= BulletSpan)
            {
                onCoolTime = false;
                yield break;
            }
            count++;
            yield return new WaitForEndOfFrame();
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

    public void setHP(float addpoint)
    {
        HP += addpoint;
        currentHP += addpoint;
    }

    public void setPow(float addpoint)
    {
        pow += addpoint;
    }

    public void setShootSpeed(float addpoint)
    {
        bulletSpeed += addpoint;
    }
}
