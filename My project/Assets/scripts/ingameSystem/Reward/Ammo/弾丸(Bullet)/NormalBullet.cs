using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : Bullet_Base
{
    public float AddDamageRatio = 0.3f;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
}
