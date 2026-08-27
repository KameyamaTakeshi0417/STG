using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_ExplosionOnSpawn", menuName = "Alpha/Behaviors/ExplosionOnSpawn")]
public class Behavior_ExplosionOnSpawn_Alpha : Alpha_BulletBehavior_Base
{
    public GameObject explosionPrefab;
    
    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true;
    }

    public override void OnSpawn(Bullet_Base bullet, int rarity, ref float stateTimer)
    {
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, bullet.transform.position, Quaternion.identity);
            
            float radius = 1.5f;
            float damage = bullet.dmg * 0.3f;
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(bullet.transform.position, radius);
            foreach(var hit in hits)
            {
                if (hit.CompareTag("Enemy") || hit.CompareTag("MidBoss") || hit.CompareTag("Boss"))
                {
                    Health hp = hit.GetComponent<Health>();
                    if (hp != null) hp.TakeDamage(damage);
                }
            }
        }
    }
}
