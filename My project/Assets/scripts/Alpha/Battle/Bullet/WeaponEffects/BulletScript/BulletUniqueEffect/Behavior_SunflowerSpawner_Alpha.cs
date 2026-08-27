using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_SunflowerSpawner", menuName = "Alpha/Behaviors/SunflowerSpawner")]
public class Behavior_SunflowerSpawner_Alpha : Alpha_BulletBehavior_Base
{
    public GameObject sunflowerPrefab;

    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = false; 
    }

    public override void OnSpawn(Bullet_Base bullet, int rarity, ref float stateTimer)
    {
        if (sunflowerPrefab != null)
        {
            GameObject sunflower = Instantiate(sunflowerPrefab, bullet.transform.position, Quaternion.identity);
            
            Effect_Sunflower_Alpha sunflowerScript = sunflower.GetComponent<Effect_Sunflower_Alpha>();
            if (sunflowerScript == null) sunflowerScript = sunflower.AddComponent<Effect_Sunflower_Alpha>();
            
            sunflowerScript.Initialize(bullet.rotate, bullet.Speed, bullet.dmg, rarity);
        }
    }
}
