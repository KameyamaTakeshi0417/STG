using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickStep : _Relic_Base
{
    // Start is called before the first frame update
    public float gainSpeed = 0;
    public float lostBlockRatio = 0.1f;

    void Start() { }

    // Update is called once per frame
    void Update() { }

    //取得時：スペースキーでダッシュができるようになる。\n装備時：攻撃と防御力が5減少する。攻撃速度が20%上昇する。
    public override void GetEffect()
    {
        base.GetEffect();
        if (m_Player.GetComponent<Action_QuickStep>() == null)
        {
            m_Player.AddComponent<Action_QuickStep>();
        }
        //プレイヤーにクイックステップを入れる
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //移動速度アップ、防御力低下
        Effect(1);
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        Effect(-1);
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの

    private void Effect(int num)
    {
        m_PlayerScript.DamageAdd -= 5f * num;
        m_PlayerScript.BlockDmg -= 5f * num;
        m_PlayerScript.moveSpeedMag -= 0.2f * num;
    }
}
