using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltCase : Case_Base
{
    

    public override void setScriptableData()
    {
        mydata.setDataItemInfo("Volt_Case", 1);
        mydata.setDataforPlayer(0, 0, 0);
        mydata.setDataforBullet(dmg, Speed, 0);
    }
}

