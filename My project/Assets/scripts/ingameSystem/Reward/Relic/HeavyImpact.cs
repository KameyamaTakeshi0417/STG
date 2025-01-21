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

    //取得時：攻撃、防御が15上昇する。\n装備時：攻撃と防御が1.5倍される。攻撃速度が大幅に遅くなる。
    public override void GetEffect()
    {
        base.GetEffect();
        //ダメージ軽減＋ダメージ上昇
        m_PlayerScript.DamageAdd += 15f;
        m_PlayerScript.BlockDmg += 15f;
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //発射スパン減少、防御力修正
        Effect(1);
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        Effect(-1);
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの

    private void Effect(int num)
    {
        m_PlayerScript.DamageMag += 0.5f * num;
        m_PlayerScript.BlockMag += 0.5f * num;
        m_PlayerScript.BulletSpanMag += 0.5f * num;
    }
}
