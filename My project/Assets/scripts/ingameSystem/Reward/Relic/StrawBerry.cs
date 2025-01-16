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
        if (m_Player.GetComponent<DrainHandler>() == null)
        {
            m_Player.AddComponent<DrainHandler>();
        }
        m_Player.AddComponent<DrainHandler>().DrainPoint += 2;
    }

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        if (m_Player.AddComponent<DrainHandler>().DrainPoint > 2)
        {
            m_Player.AddComponent<DrainHandler>().DrainPoint -= 2;
        }
        else
        {
            if (m_Player.GetComponent<DrainHandler>() != null)
            {
                Destroy(m_Player.GetComponent<DrainHandler>());
            }
        }
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
