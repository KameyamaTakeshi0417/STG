using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltPrimer : Primer_Base
{
    // Start is called before the first frame update
    public override void StrikePrimer()
    {
               GameObject bulletPrefab = Instantiate(
            Resources.Load<GameObject>("Objects/Effect_Volt"),
            GameObject.Find("Player").transform.position,
            Quaternion.identity
        );
        bulletPrefab.GetComponent<Effect_Volt>().startVolt(30,20,2);
    }
}
