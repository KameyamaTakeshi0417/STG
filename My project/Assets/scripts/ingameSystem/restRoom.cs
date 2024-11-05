using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class restRoom : MonoBehaviour
{
    public bool canRest;
    public bool useRest;
    // Start is called before the first frame update
    void Start()
    {
        useRest = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canRest && !useRest)
        {
            GameObject.Find("fadeBoard").GetComponent<FadeBoard>().callFadeScreen();
            useRest = true;
            Health playerHealth;
            playerHealth = GameObject.Find("Player").GetComponent<Health>();
            playerHealth.setCurrentHP(playerHealth.getCurrentHP()+(int)playerHealth.getHP() * 0.3f);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        canRest = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        canRest = false;
    }
}
