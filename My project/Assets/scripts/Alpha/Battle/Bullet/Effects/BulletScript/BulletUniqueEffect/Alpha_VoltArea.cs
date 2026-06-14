using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

public class Alpha_VoltArea : MonoBehaviour, IAlphaPoolable
{
    public GameObject sourcePrefab;

    private float dmg;
    private int voltLevel = 0;
    public float lifetime = 4.5f;

    private HashSet<GameObject> currentEnemiesInside = new HashSet<GameObject>();
    private Dictionary<GameObject, int> enemyTickCounters = new Dictionary<GameObject, int>();
    private bool isActivated = false;

    // 検索・連鎖用パラメータ
    public float chainRadius = 5f; 

    // 新機能: Tick短縮用パラメータ
    private int tickReduceCount = 0;
    private float rapidDamageTimer = 0f;
    private float rapidDamageInterval = 0.1f;

    public void OnRentFromPool()
    {
        currentEnemiesInside.Clear();
        enemyTickCounters.Clear();
        isActivated = false;
        voltLevel = 0;
        tickReduceCount = 0;
        rapidDamageTimer = 0f;
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

    public void ActivateVoltArea(float damage, int voltLvl, int tickReduce = 0)
    {
        this.dmg = damage;
        this.voltLevel = voltLvl;
        this.tickReduceCount = tickReduce;
        this.rapidDamageTimer = 0f;
        isActivated = true;
        
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
                enemyTickCounters[enemyObj] = 0;
                
                // 触れた瞬間に1回目のダメージと連鎖（canChain = true）
                ApplyDamageAndVolt(enemyObj, true);
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

    private void Update()
    {
        if (!isActivated) return;
        
        // カウント4以上なら、Tickシステムを無視して0.1秒ごとの超高速ダメージ（連鎖はしない）
        if (tickReduceCount >= 4)
        {
            rapidDamageTimer += Time.deltaTime;
            if (rapidDamageTimer >= rapidDamageInterval)
            {
                rapidDamageTimer -= rapidDamageInterval;

                List<GameObject> currentEnemiesList = new List<GameObject>(currentEnemiesInside);
                List<GameObject> enemiesToRemove = new List<GameObject>();

                foreach (var enemyObj in currentEnemiesList)
                {
                    if (enemyObj == null || !enemyObj.activeInHierarchy || !currentEnemiesInside.Contains(enemyObj))
                    {
                        enemiesToRemove.Add(enemyObj);
                        continue;
                    }
                    // 定期ダメージでは連鎖は発生させない（canChain = false）
                    ApplyDamageAndVolt(enemyObj, false);
                }

                foreach (var e in enemiesToRemove)
                {
                    currentEnemiesInside.Remove(e);
                    enemyTickCounters.Remove(e);
                }
            }
        }
    }

    private void HandleTick()
    {
        if (!isActivated) return;
        if (tickReduceCount >= 4) return; // 高速モードの時はTick処理をスキップ

        List<GameObject> enemiesToRemove = new List<GameObject>();
        List<GameObject> currentEnemiesList = new List<GameObject>(currentEnemiesInside);

        // 必要Tick数の計算（最大4, tickReduceCountが1なら3, 2なら2...）
        int requiredTicks = Mathf.Max(1, 4 - tickReduceCount);

        foreach (var enemyObj in currentEnemiesList)
        {
            if (enemyObj == null || !enemyObj.activeInHierarchy || !currentEnemiesInside.Contains(enemyObj))
            {
                enemiesToRemove.Add(enemyObj);
                continue;
            }

            enemyTickCounters[enemyObj]++;

            if (enemyTickCounters[enemyObj] >= requiredTicks)
            {
                enemyTickCounters[enemyObj] = 0;
                // 定期ダメージでは連鎖は発生させない（canChain = false）
                ApplyDamageAndVolt(enemyObj, false);
            }
        }

        foreach (var e in enemiesToRemove)
        {
            currentEnemiesInside.Remove(e);
            enemyTickCounters.Remove(e);
        }
    }

    private void ApplyDamageAndVolt(GameObject enemyObj, bool canChain)
    {
        _Health_Base health = enemyObj.GetComponent<_Health_Base>();
        if (health != null && health.getCurrentHP() > 0)
        {
            health.ApplyDamage(dmg);

            health.VoltCount += voltLevel;
            // Debug.Log($"[{enemyObj.name}] に帯電 {voltLevel} を付与。現在帯電数: {health.VoltCount}");

            if (canChain)
            {
                int nextVoltLevel = voltLevel - 1;
                if (nextVoltLevel > 0)
                {
                    TryChainToNearbyEnemy(enemyObj.transform.position, nextVoltLevel);
                }
            }
        }
    }

    private void TryChainToNearbyEnemy(Vector3 centerPosition, int nextVoltLevel)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerPosition, chainRadius);
        
        Transform bestTarget = null;
        float closestDistSq = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                float distSq = (col.transform.position - centerPosition).sqrMagnitude;
                if (distSq < 0.1f) continue; 

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
                    // 新しく生まれたエリアの連鎖回数は変わるが、Tick短縮状態は親のものを引き継ぐ
                    nextArea.ActivateVoltArea(this.dmg, nextVoltLevel, this.tickReduceCount); 
                }
            }
        }
    }
}
