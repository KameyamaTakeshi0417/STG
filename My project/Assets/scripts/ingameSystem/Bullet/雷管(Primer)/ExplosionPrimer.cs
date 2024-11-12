using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPrimer : Primer_Base
{
    // Start is called before the first frame update

    protected override IEnumerator Fire()
    {
        
        GameObject bulletPrefab = Instantiate(Resources.Load<GameObject>("Objects/Effect_Explosion"), gameObject.transform.position, Quaternion.identity);
        bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);

       

        yield break;
    }
}
