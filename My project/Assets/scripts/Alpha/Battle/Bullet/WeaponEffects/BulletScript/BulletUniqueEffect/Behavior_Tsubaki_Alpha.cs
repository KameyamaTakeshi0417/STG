using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_Tsubaki", menuName = "Alpha/Behaviors/Tsubaki")]
public class Behavior_Tsubaki_Alpha : Alpha_BulletBehavior_Base
{
    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true;
    }

    public override void OnSpawn(Bullet_Base bullet, int rarity, ref float stateTimer)
    {
    }

    public override void OnFlight(Bullet_Base bullet, int rarity, ref float stateTimer, float deltaTime)
    {
    }
}
