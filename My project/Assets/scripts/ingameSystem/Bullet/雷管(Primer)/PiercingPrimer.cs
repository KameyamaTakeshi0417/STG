using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingPrimer : Primer_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void StrikePrimer()
    {
        if (targetBullet != null)
        {
            targetBullet.GetComponent<Bullet_Base>().piercingCount = rarelity + 1;
        }
    }
}
