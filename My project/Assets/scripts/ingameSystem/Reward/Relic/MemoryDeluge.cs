using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryDeluge : _Relic_Base
{
    int addNum = 0;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        m_PlayerScript.addExp(300);
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        addNum = (m_PlayerScript.Exp / 100);
        if (addNum <= 0)
        {
            return;
        }

        m_PlayerScript.DamageAdd += addNum;
        m_PlayerScript.BlockDmg += addNum;
    }

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        m_PlayerScript.DamageAdd -= addNum;
        m_PlayerScript.BlockDmg -= addNum;
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
