using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltPrimer : Primer_Base
{
    // Start is called before the first frame update

    protected override IEnumerator Fire()
    {
        
        GameObject bulletPrefab = Instantiate(Resources.Load<GameObject>("Objects/Effect_Volt"), gameObject.transform.position, Quaternion.identity);
        bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);

       

        yield break;
    }
}
