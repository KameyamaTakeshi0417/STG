using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPrimer : Primer_Base
{
    // Start is called before the first frame update
    public override void StrikePrimer()
    {
        GameObject bulletPrefab = Instantiate(
            Resources.Load<GameObject>("Objects/Effect_Explosion"),
            GameObject.Find("Player").transform.position,
            GameObject.Find("Player").transform.rotation
        );
        bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);
    }
}
