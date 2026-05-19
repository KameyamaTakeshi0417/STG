using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbControll_Alpha : MonoBehaviour
{
    // Start is called before the first frame update
    public int rarelity;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject.Find("manager").GetComponent<treasureManager_Alpha>().GetTreasure(rarelity);
            Destroy(gameObject);
        } 
        
    }
}
