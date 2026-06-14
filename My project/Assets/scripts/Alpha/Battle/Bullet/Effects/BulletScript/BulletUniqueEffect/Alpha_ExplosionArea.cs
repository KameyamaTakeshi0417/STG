using System.Collections.Generic;
using UnityEngine;

public class Alpha_ExplosionArea : MonoBehaviour, IAlphaPoolable
{
    public GameObject sourcePrefab; // プール用プレハブ記憶

    private float dmg;
    public float lifetime = 2.0f; // 領域が消えるまでの時間

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>(); // 同じ領域から何度も食らうのを防ぐ
    private bool isActivated = false;

    public void OnRentFromPool()
    {
        hitEnemies.Clear();
        isActivated = false;
    }

    public void OnReturnToPool()
    {
        isActivated = false;
        CancelInvoke(nameof(ReturnSelf));
    }

    public void ActivateExplosionArea(float damage)
    {
        this.dmg = damage;
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

            // 既にこの領域でダメージを受けている場合は無効
            if (hitEnemies.Contains(enemyObj)) return;
            hitEnemies.Add(enemyObj);

            _Health_Base health = enemyObj.GetComponent<_Health_Base>();
            if (health != null)
            {
                health.ApplyDamage(dmg);
            }
        }
    }
}
