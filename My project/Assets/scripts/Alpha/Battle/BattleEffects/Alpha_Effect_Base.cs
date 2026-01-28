using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alpha_Effect_Base : MonoBehaviour
{
    //エフェクトそのものの基底クラス
    // Start is called before the first frame update
    public virtual void StartEffect(int rarity) {
    
    }
    public virtual void ShootEffect(int rarity)
    {

    }
    public virtual void FlyEffect(int rarity) { }
    public virtual void HitEffect(int rarity) { }
    public virtual void EquippedEffect(int rarity) { }
    public virtual void DequipEffect(int rarity) { }
}
