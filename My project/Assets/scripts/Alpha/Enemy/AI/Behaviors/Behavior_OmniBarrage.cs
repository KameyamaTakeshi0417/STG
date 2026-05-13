using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Omni Barrage Behavior", menuName = "EnemyAI/Behaviors/OmniBarrage")]
public class Behavior_OmniBarrage : EnemyBehaviorData_Base
{
    public string attackName = "Omni Barrage";

    [Header("Barrage Settings")]
    [Tooltip("1周（360度）に発射する弾の数")]
    public int bulletsPerShot = 12;
    [Tooltip("1サイクルの発射回数")]
    public int repeats = 3;
    [Tooltip("発射ごとの待機時間（秒）")]
    public float waitInterval = 0.2f;
    [Tooltip("全サイクル終了後のクールダウン（秒）")]
    public float cooldown = 2.0f;

    [Header("Angle Settings")]
    [Tooltip("初期の角度オフセット")]
    public float angleOffset = 0f;
    [Tooltip("毎回の発射ごとに加算される角度（渦巻きのようにする場合）")]
    public float rotatePerRepeat = 10f;

    [Header("Spawn & Aim Settings")]
    [Tooltip("発射位置のオフセット（エネミー中心からのローカル座標）")]
    public Vector2 spawnOffset = Vector2.zero;
    [Tooltip("trueの場合、移動方向を基準（正面＝0度）として弾を発射します")]
    public bool aimAtMoveDirection = true;
    [Tooltip("trueの場合、連射中（repeats中）もエネミーの移動に合わせて常に発射位置を更新します。falseの場合は発射開始時の位置で固定されます。")]
    public bool updatePositionDuringBarrage = true;

    [Header("Bullet Settings")]
    [Tooltip("弾のプレハブ")]
    public GameObject bulletPrefab;
    [Tooltip("弾の速度")]
    public float bulletSpeed = 5f;
    [Tooltip("弾の生存時間")]
    public float bulletLifeTime = 10f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        Vector2 lastPos = ai.transform.position;

        while (true)
        {
            if (ai is Alpha_EliteEnemyAI eliteAi)
            {
                eliteAi.TriggerAttackEvent(attackName);
            }

            Vector2 currentPos = ai.transform.position;
            float baseAngle = 0f;

            if (aimAtMoveDirection)
            {
                Vector2 moveDir = currentPos - lastPos;
                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                }
                else
                {
                    baseAngle = ai.transform.eulerAngles.z;
                }
            }

            float currentAngleOffset = baseAngle + angleOffset;
            Vector3 spawnPos = currentPos + (Vector2)(ai.transform.rotation * spawnOffset);

            for (int r = 0; r < repeats; r++)
            {
                if (bulletsPerShot <= 0) break;

                if (updatePositionDuringBarrage)
                {
                    currentPos = ai.transform.position;
                    spawnPos = currentPos + (Vector2)(ai.transform.rotation * spawnOffset);
                }

                float angleStep = 360f / bulletsPerShot;

                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float angle = currentAngleOffset + (angleStep * i);
                    Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                    // 弾の生成
                    GameObject bObj = null;
                    if (Alpha_ObjectPoolManager.Instance != null && bulletPrefab != null)
                    {
                        bObj = Alpha_ObjectPoolManager.Instance.Rent(bulletPrefab, spawnPos, Quaternion.identity);
                    }
                    else if (bulletPrefab != null)
                    {
                        bObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
                    }

                    if (bObj != null)
                    {
                        Bullet_Base bullet = bObj.GetComponent<Bullet_Base>();
                        if (bullet != null)
                        {
                            // ダメージはプレハブの数値をそのまま使用し、速度と寿命を上書き
                            bullet.setStatus(dir, bulletSpeed, bullet.dmg);
                            bullet.DestroyTime = bulletLifeTime;
                            bullet.shoot();
                        }
                    }
                }

                currentAngleOffset += rotatePerRepeat;

                // 次の発射まで待機
                yield return new WaitForSeconds(waitInterval);
            }

            lastPos = ai.transform.position;

            // 次のサイクルまで待機
            yield return new WaitForSeconds(cooldown);
        }
    }
}
