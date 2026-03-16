using System.Collections.Generic;
using UnityEngine;

// 帯電領域（VoltArea）自体の制御を担うスクリプト
// アタッチするプレハブは、「トリガー付きのCollider2D（円形など）」を持っている想定です。
public class Alpha_VoltArea : MonoBehaviour, IAlphaPoolable
{
    public GameObject sourcePrefab; // プール用プレハブ記憶

    public void OnRentFromPool()
    {
        hitEnemies.Clear();
        isActivated = false;
        voltLevel = 0;
    }

    public void OnReturnToPool()
    {
        isActivated = false;
        CancelInvoke(nameof(ReturnSelf));
    }
    private float dmg;
    private int voltLevel = 0; // 現在付与できる「帯電x」の数値
    public float lifetime = 1.0f; // 領域が消えるまでの時間

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>(); // 同じ領域から何度も食らうのを防ぐ

    private bool isActivated = false;

    // 検索・連鎖用パラメータ
    public float chainRadius = 5f; // 連鎖先を探す半径

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated) return;

        if (collision.CompareTag("Enemy"))
        {
            GameObject enemyObj = collision.gameObject;

            // 既にこの領域でダメージを受けている場合は無視
            if (hitEnemies.Contains(enemyObj)) return;
            hitEnemies.Add(enemyObj);

            _Health_Base health = enemyObj.GetComponent<_Health_Base>();
            if (health != null)
            {
                // ① ダメージを与える（領域のダメージは任意でスケール可能）
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
    }

    // 連鎖先を探してそこに新しい帯電領域を作る
    private void TryChainToNearbyEnemy(Vector3 centerPosition, int nextVoltLevel)
    {
        // 物理判定の円キャストで周囲の敵を探す（自身のレイヤーに注意・ここでは簡易的に全Enemyを探す）
        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerPosition, chainRadius);
        
        Transform bestTarget = null;
        float closestDistSq = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                // ①自分自身（今当たった敵）は除外する
                // （hitEnemiesに入っているか、距離が一番近いかで判定）
                if (hitEnemies.Contains(col.gameObject)) continue;

                _Health_Base h = col.GetComponent<_Health_Base>();
                if (h != null && h.getCurrentHP() > 0)
                {
                    float distSq = (col.transform.position - centerPosition).sqrMagnitude;
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        bestTarget = col.transform;
                    }
                }
            }
        }

        // 一番近い別の敵が見つかったら、その敵の少し手前か真上に新しい帯電領域を発生させる
        if (bestTarget != null)
        {
            Debug.Log($"連鎖発生！ 次のVoltLv: {nextVoltLevel} -> Target: {bestTarget.name}");
            
            // 自分(帯電領域プレハブ)と同じものを連鎖先で生成する
            GameObject chainObj = null;
            if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
            {
                chainObj = Alpha_ObjectPoolManager.Instance.Rent(sourcePrefab, bestTarget.position, Quaternion.identity);
                Alpha_VoltArea ca = chainObj.GetComponent<Alpha_VoltArea>();
                if (ca != null) ca.sourcePrefab = sourcePrefab;
            }
            else
            {
                chainObj = Instantiate(gameObject, bestTarget.position, Quaternion.identity);
            }
            
            Alpha_VoltArea chainScript = chainObj.GetComponent<Alpha_VoltArea>();
            if (chainScript != null)
            {
                // Instantiate時は自分自身がコピー元になるのでsourcePrefabを継承する可能性があるがRentは初期化される
                if (chainScript.sourcePrefab == null) chainScript.sourcePrefab = this.sourcePrefab;
                // すでに自分が踏んだ敵リストを渡し、逆流を防ぐことも可能
                chainScript.hitEnemies = new HashSet<GameObject>(this.hitEnemies); 
                
                // x-1のレベルで起動
                chainScript.ActivateVoltArea(dmg, nextVoltLevel);
            }
        }
    }
}
