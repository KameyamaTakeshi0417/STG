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

    public override void GetEffect()
    {
        base.GetEffect();
        m_Player.GetComponent<Action_QuickStep>().enabled = true;
        //プレイヤーにクイックステップを入れる
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //移動速度アップ、防御力低下
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
