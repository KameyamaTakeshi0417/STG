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
        m_PlayerScript.HP += 10;
        m_PlayerScript.currentHP += 10;
    } //取得時のみ呼び出す
}
