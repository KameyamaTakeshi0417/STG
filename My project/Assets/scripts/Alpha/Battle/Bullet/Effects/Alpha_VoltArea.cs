using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

// 帯電領域（VoltArea）本体の制御を担うスクリプト
// アタッチするプレハブは、「トリガー付きのCollider2D（円形など）」を持っている想定です。
public class Alpha_VoltArea : MonoBehaviour, IAlphaPoolable
{
    public GameObject sourcePrefab; // プール用プレハブ記憶

    private float dmg;
    private int voltLevel = 0; // 現在付与できる「帯電x」の数値
    public float lifetime = 4.5f; // 領域が消えるまでの時間 (4-5秒に延長)

    private HashSet<GameObject> currentEnemiesInside = new HashSet<GameObject>();
    private Dictionary<GameObject, int> enemyTickCounters = new Dictionary<GameObject, int>();
    private bool isActivated = false;

    // 検索・連鎖用パラメータ
    public float chainRadius = 5f; // 連鎖先を探す半径

    public void OnRentFromPool()
    {
        currentEnemiesInside.Clear();
        enemyTickCounters.Clear();
        isActivated = false;
        voltLevel = 0;
        if (Alpha_TickManager.Instance != null)
        {
            Alpha_TickManager.Instance.OnTick += HandleTick;
        }
    }

    public void OnReturnToPool()
    {
        isActivated = false;
        CancelInvoke(nameof(ReturnSelf));
        if (Alpha_TickManager.Instance != null)
        {
            Alpha_TickManager.Instance.OnTick -= HandleTick;
        }
    }

    public void ActivateVoltArea(float damage, int voltLvl)
    {
        this.dmg = damage;
        this.voltLevel = voltLvl;
        isActivated = true;
        
        // 寿命が来たら消滅（プーリングの場合はInvokeで返却）
        Invoke(nameof(ReturnSelf), lifetime);
    }

    private void ReturnSelf()
    {
        if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
        {
            Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Alpha_TickManager.Instance != null)
        {
            Alpha_TickManager.Instance.OnTick -= HandleTick;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated) return;

        if (collision.CompareTag("Enemy"))
        {
            GameObject enemyObj = collision.gameObject;
            if (!currentEnemiesInside.Contains(enemyObj))
            {
                currentEnemiesInside.Add(enemyObj);
                enemyTickCounters[enemyObj] = 0; // Tickカウンタ初期化
                
                // 触れた瞬間に1回目のダメージと効果
                ApplyDamageAndVolt(enemyObj);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActivated) return;

        if (collision.CompareTag("Enemy"))
        {
            GameObject enemyObj = collision.gameObject;
            if (currentEnemiesInside.Contains(enemyObj))
            {
                currentEnemiesInside.Remove(enemyObj);
                enemyTickCounters.Remove(enemyObj);
            }
        }
    }

    private void HandleTick()
    {
        if (!isActivated) return;

        List<GameObject> enemiesToRemove = new List<GameObject>();

        foreach (var enemyObj in currentEnemiesInside)
        {
            if (enemyObj == null || !enemyObj.activeInHierarchy)
            {
                enemiesToRemove.Add(enemyObj);
                continue;
            }

            enemyTickCounters[enemyObj]++;

            // 2tick(1.0秒)ごとにダメージ
            if (enemyTickCounters[enemyObj] >= 2)
            {
                enemyTickCounters[enemyObj] = 0;
                ApplyDamageAndVolt(enemyObj);
            }
        }

        foreach (var e in enemiesToRemove)
        {
            currentEnemiesInside.Remove(e);
            enemyTickCounters.Remove(e);
        }
    }

    private void ApplyDamageAndVolt(GameObject enemyObj)
    {
        _Health_Base health = enemyObj.GetComponent<_Health_Base>();
        if (health != null && health.getCurrentHP() > 0)
        {
            // ① ダメージを与える
            health.TakeDamage(dmg);

            // ② 帯電x を付与する
            health.VoltCount += voltLevel;
            Debug.Log($"[{enemyObj.name}] に帯電 {voltLevel} を付与。現在帯電数: {health.VoltCount}");

            // ③ もしx-1が0より大きいなら、周囲に新しい帯電領域(x-1)を連鎖生成する
            int nextVoltLevel = voltLevel - 1;
            if (nextVoltLevel > 0)
            {
                TryChainToNearbyEnemy(enemyObj.transform.position, nextVoltLevel);
            }
        }
    }

    // 連鎖先を探してそこに新しい帯電領域を作る
    private void TryChainToNearbyEnemy(Vector3 centerPosition, int nextVoltLevel)
    {
        // 物理判定の円キャストで周囲の敵を探す（自身を含めないように注意）
        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerPosition, chainRadius);
        
        Transform bestTarget = null;
        float closestDistSq = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                // 今当たった敵（中心にいる敵）は除外する
                float distSq = (col.transform.position - centerPosition).sqrMagnitude;
                if (distSq < 0.1f) continue; // ほぼ同じ位置ならスキップ

                _Health_Base h = col.GetComponent<_Health_Base>();
                if (h != null && h.getCurrentHP() > 0)
                {
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        bestTarget = col.transform;
                    }
                }
            }
        }

        if (bestTarget != null)
        {
            if (sourcePrefab != null)
            {
                GameObject obj = null;
                if (Alpha_ObjectPoolManager.Instance != null)
                {
                    obj = Alpha_ObjectPoolManager.Instance.Rent(sourcePrefab, bestTarget.position, Quaternion.identity);
                }
                else
                {
                    obj = Instantiate(sourcePrefab, bestTarget.position, Quaternion.identity);
                }

                Alpha_VoltArea nextArea = obj.GetComponent<Alpha_VoltArea>();
                if (nextArea != null)
                {
                    nextArea.sourcePrefab = this.sourcePrefab;
                    nextArea.ActivateVoltArea(this.dmg, nextVoltLevel); // ダメージは減衰させない仕様
                }
            }
        }
    }
}
