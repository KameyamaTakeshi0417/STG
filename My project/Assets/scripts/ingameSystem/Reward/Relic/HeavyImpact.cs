using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyImpact : _Relic_Base
{
    public float oldSpan;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        //ダメージ軽減＋ダメージ上昇
        m_PlayerScript.DamageAdd += 10f;
        m_PlayerScript.BlockDmg += 20f;
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //発射スパン減少、防御力修正
        oldSpan = m_PlayerScript.BulletSpan;
        m_PlayerScript.DamageMag += 0.5f;
        m_PlayerScript.BlockMag += 0.5f;
        m_PlayerScript.BulletSpan = m_PlayerScript.BulletSpan * 0.75f;
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        m_PlayerScript.DamageMag -= 0.5f;
        m_PlayerScript.BlockMag -= 0.5f;
        m_PlayerScript.BulletSpan = oldSpan;
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
