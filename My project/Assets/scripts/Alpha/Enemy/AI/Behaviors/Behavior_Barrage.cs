using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Barrage Behavior", menuName = "EnemyAI/Behaviors/Barrage")]
public class Behavior_Barrage : EnemyBehaviorData_Base
{
    public string attackName = "Normal Barrage";
    
    [Header("Barrage Parameters")]
    public float fireInterval = 0.5f;     // 発射間隔（秒）
    public float bulletSpeed = 5f;        // 弾速
    public int bulletCount = 3;           // 1度に発射する弾数（N-Way）
    public float spreadAngle = 30f;       // 拡散角度（扇状の広がり）
    public float waveRotationSpeed = 0f;  // 発射角を時間で回転させる場合
    public float cooldown = 2f;           // 1サイクルの後のインターバル（必要に応じて）

    [Tooltip("弾のプレハブ。Bullet_Baseがアタッチされていること")]
    public GameObject bulletPrefab;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // エリートAIであれば演出用のフックを叩く
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(attackName);
        }

        float currentRotationOffset = 0f;

        while (true)
        {
            // ターゲットがいなければ待機
            if (!ai.HasTarget())
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            // 基準となる角度（プレイヤー方向）
            Vector2 toPlayer = ai.TargetTransform.position - ai.transform.position;
            float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

            // 回転を加味
            baseAngle += currentRotationOffset;
            currentRotationOffset += waveRotationSpeed * fireInterval;

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
                    bObj = Alpha_ObjectPoolManager.Instance.Rent(bulletPrefab, ai.transform.position, Quaternion.identity);
                }
                else if (bulletPrefab != null)
                {
                    bObj = Instantiate(bulletPrefab, ai.transform.position, Quaternion.identity);
                }

                if (bObj != null)
                {
                    Bullet_Base bullet = bObj.GetComponent<Bullet_Base>();
                    if (bullet != null)
                    {
                        bullet.setStatus(dir, bulletSpeed, 10f); // ダメージは任意調整可能
                        bullet.shoot();
                    }
                }
            }

            // 次の発射まで待機
            yield return new WaitForSeconds(fireInterval);
        }
    }
}
