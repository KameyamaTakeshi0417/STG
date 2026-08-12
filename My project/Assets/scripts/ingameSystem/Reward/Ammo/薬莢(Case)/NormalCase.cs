using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCase : Case_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    //最終的にはこれを裁E��してぁE��弾は威力増加させたい
    

    public override void ApplyCaseEffect(GameObject bullet)
    {
        base.ApplyCaseEffect(bullet);
        GetComponent<Bullet_Base>().dmg += 5 * rarelity;
    }
}

