using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangledFoot : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        m_Playerhealth.addHP(50);
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        if (m_Player.GetComponent<DrainHandler>() == null)
        {
            m_Player.AddComponent<DrainHandler>();
        }
        m_Player.GetComponent<DrainHandler>().DrainPoint += 4;
        m_PlayerScript.moveSpeedMag -= 0.3f;
    }

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        if (m_Player.GetComponent<DrainHandler>().DrainPoint > 4)
        {
            m_Player.GetComponent<DrainHandler>().DrainPoint -= 4;
        }
        else
        {
            if (m_Player.GetComponent<DrainHandler>() != null)
            {
                Destroy(m_Player.GetComponent<DrainHandler>());
            }
        }
        m_PlayerScript.moveSpeedMag += 0.3f;
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
