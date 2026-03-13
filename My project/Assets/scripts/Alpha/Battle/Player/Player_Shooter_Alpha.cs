using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shooter_Alpha : MonoBehaviour
{
    public bool onCoolTime;
    public AudioSource shootAudioSource; // 弾の発射音用のAudioSource
    public float moveRadius = 2f; // プレイヤーを中心とする半径

    private Vector3 watch;
    private bool isPaused = false;
    private Transform playerTransform;
    GameObject PlayerObj;
    playerStatusManager_Alpha playerStatusScript;

    void Awake()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        PlayerObj = playerTransform.gameObject;
        if (playerTransform != null)
        {
            playerStatusScript = GameObject.Find("manager").GetComponent<playerStatusManager_Alpha>();
        }
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

        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // プレイヤーを中心にマウスの方向に追従させる
        Vector3 direction = (mousePosition - playerTransform.position).normalized;


        // オブジェクトの向きをマウスポインタの方向に向ける
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
        // 現在装備している弾丸、ケース、プライマーを取得

        GameObject bulletPrefab;
        float distance = moveRadius;
        Vector3 createPos = playerTransform.position + (watch * distance);
        Vector3 NcreatePos = Vector3.Normalize(watch);
        bulletPrefab = Instantiate(
                  Resources.Load<GameObject>("Objects/Bullet/NormalBullet"),
                  createPos,
                  Quaternion.identity
              );        // 弾の向きを変更
        float rotationAngle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        bulletPrefab.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();
        
        // 弾のステータス（角度、弾速、ダメージ）を設定
        // 関数が干渉する前の弾速の基本スピードを今の1.5倍にする
        float baseBulletSpeed = playerStatusScript.bulletSpeed * 1.5f;
        bulletScript.setStatus(watch, baseBulletSpeed, playerStatusScript.pow);

        bulletScript.shoot();



        //ドレイン効果が付与できるなら付与する
        DrainHandler targetHandler = GetComponent<DrainHandler>();
        // サウンドエフェクトの再生
       // shootAudioSource.Play();
    }
}
