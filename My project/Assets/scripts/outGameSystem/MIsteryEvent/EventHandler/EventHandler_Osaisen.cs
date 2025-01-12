using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandler_Osaisen : _EventHandlerBase
{
    // Start is called before the first frame update
    public override void Init()
    {
        useText[0] = "お賽銭箱がある・・・・。\nお賽銭をいれたらご利益があるかもしれない。";
        useText[1] = "滅茶苦茶ご利益があった！";
        useText[2] = "ぼちぼちのご利益があった・・・。";
        useText[3] = "なーんにも起きなかった・・・！";
        base.Init();
    }

    public override void Action1() { }

    public override void Action2() { }

    public override void Action3() { }

    public override void Action4() { }

    public override void Action5() { }
}
