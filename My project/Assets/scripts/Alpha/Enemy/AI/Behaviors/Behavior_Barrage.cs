using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Barrage Behavior", menuName = "EnemyAI/Behaviors/Barrage")]
public class Behavior_Barrage : EnemyBehaviorData_Base
{
    public string attackName = "Normal Barrage";
    
    [Header("Barrage Parameters")]
    [Tooltip("1回のバーストで発射する回数")]
    public int repeats = 3;
    [Tooltip("バースト中の発射間隔（秒）")]
    public float fireInterval = 0.5f;     // 発射間隔（秒）
    [Tooltip("バースト終了後の待機時間（秒）")]
    public float cooldown = 2f;           // 1サイクルの後のインターバル
    [Tooltip("同時に発射する弾と弾の間のディレイ（秒）。0なら完全同時発射（扇状）。数値を入れれば連射（スイープ）になります。")]
    public float delayBetweenBullets = 0f;
    public float bulletSpeed = 5f;        // 弾速
    public int bulletCount = 3;           // 1度に発射する弾数（N-Way）
    public float spreadAngle = 30f;       // 拡散角度（扇状の広がり）
    public float waveRotationSpeed = 0f;  // 発射角を時間で回転させる場合

    [Header("Spawn & Aim Settings")]
    [Tooltip("発射位置のオフセット（エネミー中心からのローカル座標）")]
    public Vector2 spawnOffset = Vector2.zero;
    [Tooltip("trueの場合、プレイヤーの方向ではなく自身が移動している方向を基準（正面）とします")]
    public bool aimAtMoveDirection = true;

    [Header("Bullet Settings")]
    [Tooltip("弾のプレハブ。Bullet_Baseがアタッチされていること")]
    public GameObject bulletPrefab;
    [Tooltip("弾の生存時間（寿命）")]
    public float bulletLifeTime = 10f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // エリートAIであれば演出用のフックを叩く
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(attackName);
        }

        float currentRotationOffset = 0f;
        Vector2 lastPos = ai.transform.position;

        while (true)
        {
            for (int r = 0; r < repeats; r++)
            {
                // ターゲットがいなければ待機（移動方向を見る設定でない場合のみ）
                if (!aimAtMoveDirection && !ai.HasTarget())
                {
                    lastPos = ai.transform.position;
                    yield return new WaitForFixedUpdate();
                    continue; // ターゲットがいない場合はバースト回数を消化せずに待機
                }

                Vector2 currentPos = ai.transform.position;
                float baseAngle = 0f;

                if (aimAtMoveDirection)
                {
                    // 移動方向を計算（現在の位置 - 前回の位置）
                    Vector2 moveDir = currentPos - lastPos;
                    if (moveDir.sqrMagnitude > 0.0001f)
                    {
                        baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        // 止まっている場合は現在の自身の回転（Right）か、プレイヤー方向を向く
                        baseAngle = ai.transform.eulerAngles.z;
                    }
                }
                else
                {
                    // 基準となる角度（プレイヤー方向）
                    Vector2 toPlayer = ai.TargetTransform.position - ai.transform.position;
                    baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
                }

                // 回転を加味
                baseAngle += currentRotationOffset;
                currentRotationOffset += waveRotationSpeed * fireInterval;

                // 発射位置の計算（ローカルオフセットを加味）
                Vector3 spawnPos = currentPos + (Vector2)(ai.transform.rotation * spawnOffset);

                // 発射処理 (N-Way)
                float startAngle = baseAngle - (spreadAngle / 2f);
                float angleStep = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;

                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = startAngle + (angleStep * i);
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
                            bullet.setRotate(dir);
                            bullet.DestroyTime = bulletLifeTime;
                            bullet.shoot();
                        }
                    }

                    if (delayBetweenBullets > 0f)
                    {
                        yield return new WaitForSeconds(delayBetweenBullets);
                    }
                }

                lastPos = ai.transform.position;

                // 次の発射まで待機
                yield return new WaitForSeconds(fireInterval);
            }

            // バースト終了後のクールダウン
            yield return new WaitForSeconds(cooldown);
        }
    }
}
