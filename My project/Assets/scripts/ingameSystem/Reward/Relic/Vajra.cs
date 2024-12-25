using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vajra : _Relic_Base
{
    public override void GetEffect()
    {
        base.GetEffect();
        m_PlayerScript.DamageAdd += 10;
    }

    // Start is called before the first frame update
    public override void EquipEffect()
    {
        base.EquipEffect();
        m_PlayerScript.DamageAdd += 30;

        m_Playerhealth.VulnerableTime = 10f;
        m_Playerhealth.VulnerableFlg = true;
    } //バトル開始時呼び出す。
}
