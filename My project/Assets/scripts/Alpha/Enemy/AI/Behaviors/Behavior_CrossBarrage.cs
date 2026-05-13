using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cross Barrage", menuName = "EnemyAI/Behaviors/CrossBarrage")]
public class Behavior_CrossBarrage : EnemyBehaviorData_Base
{
    public string attackName = "Cross Barrage";
    
    [Header("Barrage Parameters")]
    [Tooltip("1回のバーストで発射する回数")]
    public int repeats = 3;
    [Tooltip("バースト中の発射間隔（秒）")]
    public float fireInterval = 0.5f;     // 発射間隔（秒）
    [Tooltip("バースト終了後の待機時間（秒）")]
    public float cooldown = 2f;           // 1サイクルの後のインターバル
    public float bulletSpeed = 5f;        // 弾速

    [Header("Spawn & Aim Settings")]
    [Tooltip("発射位置のオフセット（エネミー中心からのローカル座標）")]
    public Vector2 spawnOffset = Vector2.zero;
    
    [Tooltip("trueの場合、エネミー自身の回転（Z角）に合わせて十字に撃ちます。falseの場合、常に画面の上下左右（0, 90, 180, 270度）へ固定して撃ちます。")]
    public bool rotateWithEnemy = true;

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

        while (true)
        {
            for (int r = 0; r < repeats; r++)
            {
                if (bulletPrefab == null)
                {
                    yield return new WaitForSeconds(fireInterval);
                    continue;
                }

                Vector2 currentPos = ai.transform.position;
                float baseAngle = 0f;

                if (rotateWithEnemy)
                {
                    // エネミーの現在のZ回転を取得
                    baseAngle = ai.transform.eulerAngles.z;
                }

                // 発射位置の計算（ローカルオフセットを加味）
                Vector3 spawnPos = currentPos + (Vector2)(ai.transform.rotation * spawnOffset);

                // 十字方向に4発撃つ（0, 90, 180, 270）
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + (90f * i);
                    Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                    // 弾の生成
                    GameObject bObj = null;
                    if (Alpha_ObjectPoolManager.Instance != null)
                    {
                        bObj = Alpha_ObjectPoolManager.Instance.Rent(bulletPrefab, spawnPos, Quaternion.identity);
                    }
                    else
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

                // 次の発射まで待機
                yield return new WaitForSeconds(fireInterval);
            }

            // バースト終了後のクールダウン
            yield return new WaitForSeconds(cooldown);
        }
    }
}
