using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletManager_Alpha : MonoBehaviour
{
    // Start is called before the first frame update
    public playerStatusManager_Alpha playerStatus;

    public float damage=0f;

    public int pierceCount = 0;
    public float getDamage()
    {
        damage = playerStatus.pow+(playerStatus.DamageAdd * playerStatus.DamageMag*0.01f);
        return damage;
    }
}
