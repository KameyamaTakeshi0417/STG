using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrawBerry : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        m_Playerhealth.addHP(10);
        m_Playerhealth.AddCurrentHP(10);
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        m_Playerhealth.TakeDamage(30);
    }

    public override void EquipHitEffect()
    {
        m_Playerhealth.AddCurrentHP(2);
    }
}
