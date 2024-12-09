using System.Collections;
using UnityEngine;

public class ShooterHandler : MonoBehaviour
{
    public float bulletSpeed;
    public float BulletSpan; // フレーム
    public bool onCoolTime;
    public AudioSource shootAudioSource; // 弾の発射音用のAudioSource
    public float moveRadius = 5f; // プレイヤーを中心とする半径
    public GameObject targetObj; // Shooter オブジェクト（プレイヤーの子オブジェクト）

    private EquipManager equipManager; // プレイヤーの装備を管理
    private Vector3 watch;
    private bool isPaused = false;
    private Transform playerTransform;

    void Awake()
    {
        equipManager = GameObject.Find("GameManager").GetComponent<EquipManager>();
        playerTransform = GameObject.FindWithTag("Player").transform;
        if (playerTransform != null)
        {
            targetObj = playerTransform.Find("Shooter").gameObject;
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

        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // プレイヤーを中心にマウスの方向に追従させる
        Vector3 direction = (mousePosition - playerTransform.position).normalized;
        targetObj.transform.position = playerTransform.position + direction * moveRadius;

        // オブジェクトの向きをマウスポインタの方向に向ける
        watch = direction;
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        targetObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

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
            if (count >= BulletSpan)
            {
                onCoolTime = false;
                yield break;
            }
            count++;
            yield return new WaitForEndOfFrame();
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
        GameObject activeBullet = equipManager.GetActiveBullet();
        GameObject activeCase = equipManager.GetActiveCase();
        GameObject activePrimer = equipManager.GetActivePrimer();
        GameObject bulletPrefab;
        float ratio = 0.5f;
        Vector3 createPos = targetObj.transform.position + (watch * ratio);
        Vector3 NcreatePos = Vector3.Normalize(watch);
        // オブジェクトの向きを変更
        float rotationAngle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));

        if (activeBullet == null)
        {
            Debug.LogWarning("No active bullet equipped.");
            bulletPrefab = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/NormalBullet"),
                createPos,
                Quaternion.Euler(new Vector3(0, 0, rotationAngle + 270))
            );
        }
        else
        {
            // 弾丸の生成
            bulletPrefab = Instantiate(
                activeBullet.GetComponent<ItemPickUp>().targetObj,
                createPos,
                Quaternion.Euler(new Vector3(0, 0, rotationAngle + 270))
            );
        }

        // 弾丸の基本ステータスを設定
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();
        if (bulletScript != null)
        {
            bulletScript.setStatus(watch, bulletSpeed, gameObject.GetComponent<Player>().pow);
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
            primerScript.targetBullet = bulletPrefab;
            primerScript.StrikePrimer();
        }

        // 弾丸の発射
        bulletScript?.fire();

        // サウンドエフェクトの再生
        shootAudioSource.Play();
    }
}
