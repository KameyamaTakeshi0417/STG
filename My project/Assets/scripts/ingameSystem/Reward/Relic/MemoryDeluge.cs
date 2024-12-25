using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryDeluge : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        m_PlayerScript.Exp += 300;
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        int addNum = (m_PlayerScript.Exp / 100);
        if (addNum <= 0)
        {
            return;
        }

        m_PlayerScript.DamageAdd += addNum;
        m_PlayerScript.BlockDmg += addNum;
    }
}
