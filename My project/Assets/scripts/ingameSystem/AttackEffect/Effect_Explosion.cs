using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Explosion : MonoBehaviour
{
    private float dmg = 30f;
    private int explosionTime = 50;
    private Vector3 scale = new Vector3(1, 1, 0);
    int damagedCount = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void startExplosion(float setdmg, int setExplosionTime)
    {
        dmg = setdmg;
        explosionTime = setExplosionTime;
        StartCoroutine(startExplosion());
    }
    private IEnumerator startExplosion()
    {
        int count = 0;
        while (count < explosionTime)
        {

            count++;
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);
        yield break;
    }
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (damagedCount >0)
            {
                collision.GetComponent<Health>().TakeDamage(dmg);
                damagedCount-=1;
            }

        }
    }
}
