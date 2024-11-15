using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Primer_Base : MonoBehaviour
{
    GameObject targetBulletObj;
    Case_Base targetCase;
    Bullet_Base targetBulletScript;
    protected float pow;
    protected float speed;
    protected int rarelity;
    public string primerName;
    // Start is called before the first frame update
    public string getName() { return primerName; }
    // Update is called once per frame
    void Update()
    {

    }
    public virtual void StrikePrimer()
    {
        StartCoroutine(Fire());
    }
    protected virtual IEnumerator Fire()
    {
        //弾丸を生成する

        //発射時の効果をここに記載する

        yield break;
    }
    public float getDmg() { return pow; }
    public float getSpeed() { return speed; }
    public int getRarelity() { return rarelity; }
}
