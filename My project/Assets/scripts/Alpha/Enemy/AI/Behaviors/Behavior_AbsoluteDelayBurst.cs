using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Absolute Delay Burst", menuName = "EnemyAI/Behaviors/AbsoluteDelayBurst")]
public class Behavior_AbsoluteDelayBurst : EnemyBehaviorData_Base
{
    public string attackName = "Absolute Delay Burst";

    [Header("Behavior Settings")]
    [Tooltip("この攻撃行動を有効にするか")]
    public bool isEnabled = true;

    [Header("Bullet Settings")]
    [Tooltip("発射する弾のプレハブ（RichBullet等を指定）")]
    public GameObject bulletPrefab;
    [Tooltip("弾の移動速度")]
    public float bulletSpeed = 5f;
    [Tooltip("弾のダメージ")]
    public float bulletDamage = 10f;
    [Tooltip("起爆解除後の寿命")]
    public float bulletLifeTime = 10f;

    [Header("Spawn Settings")]
    [Tooltip("弾を生成する画面の絶対位置（ワールド座標）")]
    public Vector2 spawnPosition = Vector2.zero;
    [Tooltip("弾の進行方向（Vector2で指定。例: 0, -1 で下向き）")]
    public Vector2 shootDirection = Vector2.down;
    
    [Header("Burst & Delay Settings")]
    [Tooltip("発射する総弾数")]
    public int totalBullets = 5;
    [Tooltip("設定時間a: すべての弾を発射しきるまでの時間(秒)")]
    public float spawnDuration = 2f;
    [Tooltip("設定時間b: 最後の弾を発射してから一斉起爆(DestroyAction)するまでの待機時間(秒)")]
    public float detonateDelay = 3f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (!isEnabled || bulletPrefab == null || totalBullets <= 0)
        {
            yield break;
        }

        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(attackName);
        }

        // 発射間隔の計算 (弾数が1発なら間隔は0)
        float spawnInterval = totalBullets > 1 ? spawnDuration / (totalBullets - 1) : 0f;

        List<Bullet_Base> spawnedBullets = new List<Bullet_Base>();

        // ==========================================
        // 【設定時間a】 弾の生成と発射フェーズ
        // ==========================================
        for (int i = 0; i < totalBullets; i++)
        {
            GameObject bObj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                bObj = Alpha_ObjectPoolManager.Instance.Rent(bulletPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                bObj = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            }

            if (bObj != null)
            {
                Bullet_Base bullet = bObj.GetComponent<Bullet_Base>();
                if (bullet != null)
                {
                    bullet.isEnemyBullet = true; 
                    
                    // 勝手に壁ヒット等で消滅しないように保護する
                    bullet.preventAutoDestroy = true;
                    bullet.DestroyTime = bulletLifeTime;

                    bullet.setStatus(shootDirection.normalized, bulletSpeed, bulletDamage);
                    bullet.setRotate(shootDirection.normalized);
                    bullet.shoot();

                    spawnedBullets.Add(bullet);
                }
            }

            // 最後の弾を発射した後は待機しない（直後に時間bのカウントダウンへ）
            if (i < totalBullets - 1 && spawnInterval > 0f)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // ==========================================
        // 【設定時間b】 全弾発射完了後の待機フェーズ
        // ==========================================
        if (detonateDelay > 0f)
        {
            yield return new WaitForSeconds(detonateDelay);
        }

        // ==========================================
        // 一斉起爆 (DestroyAction)
        // ==========================================
        foreach (var bullet in spawnedBullets)
        {
            // すでに別の要因（エラーや手動破棄）で消えていなければ
            if (bullet != null && bullet.gameObject.activeInHierarchy)
            {
                // 保護を解除
                bullet.preventAutoDestroy = false;
                
                // 弾のDestroyActionを強制起動
                bullet.DestroyAction();
            }
        }
    }
}
