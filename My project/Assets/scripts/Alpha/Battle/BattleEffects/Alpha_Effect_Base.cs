using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alpha_Effect_Base : MonoBehaviour
{
    //エフェクトそのものの基底クラス
    // Start is called before the first frame update
    protected virtual void StartEffect() { }
    protected virtual void FlyEffect() { }
    protected virtual void HitEffect() { }
    protected virtual void EquippedEffect() { }
    protected virtual void DequipEffect() { }
}
