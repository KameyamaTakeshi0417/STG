using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullFocus : _Relic_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void GetEffect()
    {
        base.GetEffect();
        //攻撃力上方修正
    } //取得時のみ呼び出す

    public override void EquipEffect()
    {
        base.EquipEffect();
        //渾身を付与。発射回数に反比例して修正されるステータスを付与
        //2秒で最大の威力になる。
        //移動速度が大幅減少
    } //フロア開始時に呼び出す。

    public override void UnEquipEffect()
    {
        base.UnEquipEffect();
        //渾身状態を解除
    } //装備解除時に呼び出す。バフを打ち消したりするためのもの
}
