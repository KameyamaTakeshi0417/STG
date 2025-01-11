using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyImpact : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        //ダメージ軽減＋ダメージ上昇
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //発射スパン減少、防御力修正
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
