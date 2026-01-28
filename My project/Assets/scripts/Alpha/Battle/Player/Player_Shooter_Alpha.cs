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
            playerStatusScript = PlayerObj.GetComponent<playerStatusManager_Alpha>();
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
        int count = 0;
        while (true)
        {
            if (count >= (playerStatusScript.BulletSpan * playerStatusScript.BulletSpanMag))
            {
                onCoolTime = false;
                yield break;
            }
            count++;
            yield return new WaitForSecondsRealtime(0.1f);
        }
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
        float ratio = 0.5f;
        Vector3 createPos =  (watch * ratio);
        Vector3 NcreatePos = Vector3.Normalize(watch);
        // オブジェクトの向きを変更
        // float rotationAngle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));


        


        // 弾丸の基本ステータスを設定


        Case_Base caseScript;
        System.Type caseType;
        // ケースの効果を弾丸にアタッチ
    
       
        //ドレイン効果が付与できるなら付与する
        DrainHandler targetHandler = GetComponent<DrainHandler>();
        // サウンドエフェクトの再生
       // shootAudioSource.Play();
    }
}
