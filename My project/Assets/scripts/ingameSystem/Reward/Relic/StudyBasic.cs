using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyBasic : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    //取得時効果：攻撃、防御、移動速度が5％ずつ上昇する。\n装備時効果：攻撃速度が-10％され、攻撃、防御、移動速度が10％ずつ上昇する。
    public override void GetEffect()
    {
        base.GetEffect();
        //全てのステータスをちょっぴり上昇させる
        m_PlayerScript.DamageMag += 0.05f;
        m_PlayerScript.BlockMag += 0.05f;
        m_PlayerScript.moveSpeedMag += 0.05f;
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        Effect(1);
        //さらに上昇させるけどデバフを付与する
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        Effect(-1);
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの

    private void Effect(int num)
    {
        m_PlayerScript.BulletSpan -= 0.1f * num;
        m_PlayerScript.DamageMag += 0.1f * num;
        m_PlayerScript.BlockMag += 0.1f * num;
        m_PlayerScript.moveSpeedMag += 0.1f * num;
    }
}
