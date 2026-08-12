using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCase : Case_Base
{
    // Start is called before the first frame update
    

    public override void setScriptableData()
    {
        mydata.setDataItemInfo("Explosion_Case", 1);
        mydata.setDataforPlayer(0, 0, 0);
        mydata.setDataforBullet(dmg, Speed, 0);
    }
}

