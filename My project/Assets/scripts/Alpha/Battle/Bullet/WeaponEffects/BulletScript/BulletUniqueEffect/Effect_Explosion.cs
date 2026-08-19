using UnityEngine;

public class Effect_Explosion_Alpha : Alpha_Effect_Base
{
    private GameObject explosionAreaPrefab;

    public Effect_Explosion_Alpha(int pos, int rarity = 1) : base(pos, rarity) 
    {
        // 航行中の展開間隔。ボルトと同じように間隔を開ける
        flightEffectInterval = 0.5f;

        // リソース等から爆発領域プレハブをロード
        explosionAreaPrefab = Resources.Load<GameObject>("Objects/Effect_Explosion")
                           ?? Resources.Load<GameObject>("Objects/Effect_Explosion");
    }

    protected virtual float CalculateExplosionDamage(Bullet_Base bullet)
    {
        return bullet.dmg * (0.25f * rarity) * bullet.secondaryDamageMultiplier;
    }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        // 発射時の爆発（マズルフラッシュ的な前方爆発）
        // bullet.transform.position は既にマズル位置にあるので、さらに少し前方にオフセットする
        Vector3 spawnPos = bullet.transform.position + bullet.originalAimDirection.normalized * 0.5f;
        
        SpawnExplosionArea(spawnPos, CalculateExplosionDamage(bullet), 1.5f);
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        // 航行時の爆発（軌道上に等倍の爆発）
        SpawnExplosionArea(bullet.transform.position, CalculateExplosionDamage(bullet), 1.0f);
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        if (!isSubBullet && bullet.GetComponent<CircularObject>() != null) return;

        // 着弾時の爆発（巨大な爆発）
        Vector3 spawnPos = bullet.transform.position;
        if (target != null && (target.CompareTag("Enemy") || target.CompareTag("Player")))
        {
            spawnPos = target.transform.position;
        }
        SpawnExplosionArea(spawnPos, CalculateExplosionDamage(bullet), 3.0f);
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
            else
            {
                Effect_Explosion oldScript = obj.GetComponent<Effect_Explosion>();
                if (oldScript != null)
                {
                    oldScript.sourcePrefab = explosionAreaPrefab;
                    oldScript.startExplosion(dmg, 10);
                }
            }
        }
        else
        {
            Debug.LogWarning("Alpha_ExplosionArea のプレハブが見つかりません。Resources/Objects/Effect_ExplosionArea_Alpha を作成・確認してください。");
        }
    }
}
