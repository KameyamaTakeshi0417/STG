using UnityEngine;

public class Effect_Explosion_Alpha : Alpha_Effect_Base
{
    private GameObject explosionAreaPrefab;

    public Effect_Explosion_Alpha(int pos, int rarity = 1) : base(pos, rarity) 
    {
        // 航行中の展開間隔。ボルトと同じように間隔を開ける
        flightEffectInterval = 0.5f;

        // リソース等から爆発領域プレハブをロード
        explosionAreaPrefab = Resources.Load<GameObject>("Objects/Effect_ExplosionArea_Alpha")
                           ?? Resources.Load<GameObject>("Objects/Effect_ExplosionArea");
    }

    protected virtual float CalculateExplosionDamage(Bullet_Base bullet)
    {
        return bullet.dmg * (0.25f * rarity);
    }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        Vector3 spawnPos = bullet.transform.position;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            spawnPos = player.transform.position;
        }
        else if (playerStatusManager_Alpha.Instance != null && playerStatusManager_Alpha.Instance.transform.parent != null)
        {
            spawnPos = playerStatusManager_Alpha.Instance.transform.position;
        }
        
        SpawnExplosionArea(spawnPos, CalculateExplosionDamage(bullet), 1.4f);
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        SpawnExplosionArea(bullet.transform.position, CalculateExplosionDamage(bullet));
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        if (!isSubBullet && bullet.GetComponent<CircularObject>() != null) return;

        Vector3 spawnPos = bullet.transform.position;
        if (target != null && (target.CompareTag("Enemy") || target.CompareTag("Player")))
        {
            spawnPos = target.transform.position;
        }
        SpawnExplosionArea(spawnPos, CalculateExplosionDamage(bullet));
    }

    private void SpawnExplosionArea(Vector3 position, float dmg, float scaleMultiplier = 1.0f)
    {
        if (explosionAreaPrefab != null)
        {
            GameObject obj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                obj = Alpha_ObjectPoolManager.Instance.Rent(explosionAreaPrefab, position, Quaternion.identity);
            }
            else
            {
                obj = GameObject.Instantiate(explosionAreaPrefab, position, Quaternion.identity);
            }
            
            // スケールの適用（プールからの再利用時にもリセット・適用されるように必ず設定）
            if (obj != null)
            {
                obj.transform.localScale = explosionAreaPrefab.transform.localScale * scaleMultiplier;
            }
            
            Alpha_ExplosionArea areaScript = obj.GetComponent<Alpha_ExplosionArea>();
            if (areaScript != null)
            {
                areaScript.sourcePrefab = explosionAreaPrefab; // プール用
                areaScript.ActivateExplosionArea(dmg);
            }
        }
        else
        {
            Debug.LogWarning("Alpha_ExplosionArea のプレハブが見つかりません。Resources/Objects/Effect_ExplosionArea_Alpha を作成・確認してください。");
        }
    }
}
