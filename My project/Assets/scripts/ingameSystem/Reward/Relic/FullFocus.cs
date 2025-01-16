using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullFocus : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    //取得時:移動速度が15%上昇する。\n装備時：3秒攻撃をしていない場合、次の一撃の威力が20倍になる。\n攻撃力が-30される。
    public override void GetEffect()
    {
        base.GetEffect();
        //攻撃力上方修正
        m_PlayerScript.moveSpeedMag += 0.15f;
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //渾身を付与。発射回数に反比例して修正されるステータスを付与
        //2秒で最大の威力になる。
        if (m_Player.GetComponent<FullFocusHandler>() == null)
        {
            m_Player.AddComponent<FullFocusHandler>();
        }
        m_PlayerScript.DamageAdd -= 30;
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        //渾身状態を解除
        if (m_Player.GetComponent<FullFocusHandler>() != null)
        {
            Destroy(m_Player.GetComponent<FullFocusHandler>());
        }
        m_PlayerScript.DamageAdd += 30;
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
