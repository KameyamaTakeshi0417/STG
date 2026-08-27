using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_ExplosionOnHit", menuName = "Alpha/Behaviors/ExplosionOnHit")]
public class Behavior_ExplosionOnHit_Alpha : Alpha_BulletBehavior_Base
{
    public GameObject explosionPrefab;
    
    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true;
    }

    public override void OnHit(Bullet_Base bullet, int rarity, Collider2D collision)
    {
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, bullet.transform.position, Quaternion.identity);
            
            float radius = 2.0f; 
            float damage = bullet.dmg * 0.5f; 
            
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
