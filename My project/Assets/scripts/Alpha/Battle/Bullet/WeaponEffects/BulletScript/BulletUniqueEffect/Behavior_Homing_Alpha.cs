using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

public class Behavior_Homing_Alpha : Alpha_BulletBehavior_Base
{
    public float homingStrength = 100f;

    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true;
    }

    public override void OnSpawn()
    {
        // Setup initial homing target if needed
    }

    public override void OnFlight(float deltaTime)
    {
        if (bullet.lockedTarget != null)
        {
            Vector3 targetDir = (bullet.lockedTarget.position - bullet.transform.position).normalized;
            bullet.rotate = Vector3.RotateTowards(bullet.rotate, targetDir, homingStrength * Mathf.Deg2Rad * deltaTime, 0f);
        }
    }
}
