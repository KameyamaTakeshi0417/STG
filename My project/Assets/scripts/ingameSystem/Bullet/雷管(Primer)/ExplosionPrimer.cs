using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPrimer : Primer_Base
{
    // Start is called before the first frame update

    protected override IEnumerator Fire()
    {
        //弾丸を生成する
        GameObject bulletPrefab = Instantiate(Resources.Load<GameObject>("Objects/Effect_Explosion"), gameObject.transform.position, Quaternion.identity);
        bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);

        //発射時の効果をここに記載する

        yield break;
    }
}
