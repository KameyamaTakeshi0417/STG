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
    public float moveSpeed;
    public GameObject bullet;
    public float bulletSpeed;
    public float BulletSpan; // フレーム
    public float bullettype = 0; //弾のタイプ決定
    public bool onCoolTime;
    public Vector3 watch;
    public float lockOnRadius = 5f; // ロックオンの半径
    public int Exp;

    private Transform lockOnTarget; // ロックオン対象
    private GameObject targetEnemy;
    public AudioSource shootAudioSource; // 弾の発射音用のAudioSource
    public AudioSource getExpAudioSource; // 経験値取得音用のAudioSource
    private Bullet_Base myBullet;
    private Case_Base myCase;
    private Primer_Base myPrimer;
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
    }

    void Start()
    {
        onCoolTime = false;
    }

    void Update()
    {
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
        if (Input.GetMouseButton(0) && !onCoolTime)
        {
            onCoolTime = true;
            ShootBullet();
            StartCoroutine(CoolTime());
        }
        if (Input.GetKey(KeyCode.Space))
        {
            // ダッシュ、もしくはクイックステップと呼称する緊急回避アクションを実装したい
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
    public GameObject getTargetEnemy() { return targetEnemy; }
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
        float ratio = 1.5f;
        Vector3 createPos = transform.position + watch * ratio;
        GameObject bulletPrefab = Instantiate(bullet, createPos, Quaternion.identity);
        bulletPrefab.GetComponent<Bullet_Base>().setStatus(watch, bulletSpeed, pow);
        bulletPrefab.AddComponent<HomingCase>();
        bulletPrefab.GetComponent<Bullet_Base>().fire();
        // サウンドエフェクトの再生
        shootAudioSource.Play();
    }
    public Vector3 getRotate() { return watch; }

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

    public void setBulletType(int type)
    {
        bullettype = type;

    }

}