using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager_Alpha : MonoBehaviour
{
    [Header("Debug Key Bindings")]
    public KeyCode killAllEnemiesKey = KeyCode.F1;
    public KeyCode killAllBulletsKey = KeyCode.F2;

    void Update()
    {
        if (Input.GetKeyDown(killAllEnemiesKey))
        {
            KillAllEnemies();
        }

        if (Input.GetKeyDown(killAllBulletsKey))
        {
            KillAllBullets();
        }
    }

    public void KillAllEnemies()
    {
        // 画面上のHealthを持つオブジェクトを取得（エネミーのベースクラス）
        Health[] enemies = FindObjectsOfType<Health>();
        int killedCount = 0;

        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                // 超絶ダメージを与えて死亡処理を呼び出す（これによりDropMidBossRewardなども走る）
                enemy.ApplyDamage(999999f);
                killedCount++;
            }
        }

        Debug.Log($"[DebugManager] Killed {killedCount} enemies on screen.");
    }

    public void KillAllBullets()
    {
        int bulletCount = 0;

        // 一般的な敵弾のタグやレイヤーを想定して削除
        string[] bulletTags = new string[] { "EnemyAttack", "EnemyBullet", "Bullet" };

        foreach (string tag in bulletTags)
        {
            try
            {
                GameObject[] bullets = GameObject.FindGameObjectsWithTag(tag);
                foreach (var bullet in bullets)
                {
                    if (bullet != null)
                    {
                        Destroy(bullet);
                        bulletCount++;
                    }
                }
            }
            catch (UnityException)
            {
                // タグが未定義の場合は無視
            }
        }

        // 万が一タグが違う場合、レイヤー名「EnemyAttack」などでも検索
        int enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");
        if (enemyAttackLayer != -1)
        {
            // 全てのオブジェクトからレイヤーで検索（重い処理なのでデバッグ用途のみ）
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (var t in allTransforms)
            {
                if (t != null && t.gameObject.layer == enemyAttackLayer)
                {
                    Destroy(t.gameObject);
                    bulletCount++;
                }
            }
        }

        Debug.Log($"[DebugManager] Cleared {bulletCount} bullets on screen.");
    }
}
