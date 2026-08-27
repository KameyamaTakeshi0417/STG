using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_ExplosionOnFlight", menuName = "Alpha/Behaviors/ExplosionOnFlight")]
public class Behavior_ExplosionOnFlight_Alpha : Alpha_BulletBehavior_Base
{
    public GameObject explosionPrefab;
    public float interval = 0.5f; 
    
    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true;
    }

    public override void OnFlight(Bullet_Base bullet, int rarity, ref float stateTimer, float deltaTime)
    {
        stateTimer += deltaTime;
        if (stateTimer >= interval)
        {
            stateTimer = 0f;
            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, bullet.transform.position, Quaternion.identity);
                exp.transform.localScale = Vector3.one * 0.5f; 
                
                float radius = 1.0f;
                float damage = bullet.dmg * 0.2f; 
                
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
}
