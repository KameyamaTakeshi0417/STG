using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : Bullet_Base
{
    public float AddDamageRatio = 0.3f;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    protected override void Update() { base.Update(); }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
}