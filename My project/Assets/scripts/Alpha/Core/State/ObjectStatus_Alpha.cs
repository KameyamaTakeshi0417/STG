using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStatus_Alpha : MonoBehaviour
{
    // Start is called before the first frame update
    public float HP;
    public float currentHP;

    public float pow;
    public float DamageAdd = 0.0f; //バフとかで増やす値
    public float DamageMag = 100.0f; //非固定ダメージの倍率
    public float BlockDmg = 0f; //ダメージ軽減数値
    public float BlockMag = 100f; //ダメージ軽減倍率
}
