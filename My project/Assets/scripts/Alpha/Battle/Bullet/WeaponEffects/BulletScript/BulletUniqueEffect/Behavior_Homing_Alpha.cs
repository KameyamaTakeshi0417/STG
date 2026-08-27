using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_Homing", menuName = "Alpha/Behaviors/Homing")]
public class Behavior_Homing_Alpha : Alpha_BulletBehavior_Base
{
    public float homingStrength = 100f; // degrees per second

    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true; 
    }

    public override void OnFlight(Bullet_Base bullet, int rarity, ref float stateTimer, float deltaTime)
    {
        if (bullet.lockedTarget != null)
        {
            Vector3 targetDir = (bullet.lockedTarget.position - bullet.transform.position).normalized;
            bullet.rotate = Vector3.RotateTowards(bullet.rotate, targetDir, homingStrength * Mathf.Deg2Rad * deltaTime, 0f).normalized;
            
            float angle = Mathf.Atan2(bullet.rotate.y, bullet.rotate.x) * Mathf.Rad2Deg;
            bullet.transform.localEulerAngles = new Vector3(0, 0, angle + 90f);
        }
    }
}
