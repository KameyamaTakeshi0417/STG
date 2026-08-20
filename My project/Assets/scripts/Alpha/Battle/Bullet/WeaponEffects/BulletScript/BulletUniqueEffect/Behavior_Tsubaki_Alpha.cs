using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

public class Behavior_Tsubaki_Alpha : Alpha_BulletBehavior_Base
{
    private float flightTimer = 0f;
    private float stopDuration = 2f; // Dummy value for hover duration

    public override void Initialize(Bullet_Base b, int r)
    {
        base.Initialize(b, r);
        canInherit = true;
    }

    public override void OnSpawn()
    {
        flightTimer = 0f;
    }

    public override void OnFlight(float deltaTime)
    {
        flightTimer += deltaTime;
        if (flightTimer < stopDuration)
        {
            bullet.Speed = Mathf.Lerp(bullet.Speed, 0, deltaTime * 5f);
        }
    }
}
