using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vajra : _Relic_Base
{
    // Start is called before the first frame update
    public override void EquipEffect()
    {
        m_PlayerScript.DamageAdd += 30;
        PlayerHealth health = m_Player.GetComponent<PlayerHealth>();
        health.VulnerableTime = 10f;
        health.VulnerableFlg = true;
    } //バトル開始時呼び出す。
}
