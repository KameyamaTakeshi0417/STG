using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Volt : MonoBehaviour
{
    private float dmg = 30f;
    private int voltTime = 50;
    private Vector3 scale = new Vector3(1, 1, 0);
    int damagedCount = 0;
    int shockCount = 2;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void startVolt(float setdmg, int setShockTime, int shockCount)
    {
        dmg = setdmg;
        voltTime = setShockTime;
        StartCoroutine(startVolt());
    }
    private IEnumerator startVolt()
    {
        int count = 0;
        while (count < voltTime)
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
            if (shockCount > 0)
            {
                collision.GetComponent<Health>().TakeDamage(dmg);
            }

        }
    }
}
