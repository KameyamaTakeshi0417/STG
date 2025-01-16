using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MightOfOak : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        m_PlayerScript.DamageAdd += 20;
    } //取得時のみ呼び出す

    //取得時：攻撃を20得る。\n装備時：攻撃を15,防御を40得る。移動速度が30%減少する。
    public override void EquipEffect()
    {
        base.EquipEffect();
        effect(1);
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        effect(-1);
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの

    private void effect(int plusOrMinus)
    {
        m_PlayerScript.DamageAdd += 15 * plusOrMinus;
        m_PlayerScript.BlockDmg += 40 * plusOrMinus;
        m_PlayerScript.moveSpeedMag -= 0.3f * plusOrMinus;
    }
}
