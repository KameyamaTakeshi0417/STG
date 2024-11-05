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
        if (Input.GetKeyDown(KeyCode.Return)&&(canRest && !useRest))
        {
            GameObject.Find("fadeBoard").GetComponent<FadeBoard>().callFadeScreen();
            useRest = true;
            Health playerHealth;
            playerHealth = GameObject.Find("Player").GetComponent<Health>();
            int healHP;
            healHP = (int)(playerHealth.getCurrentHP() + (int)playerHealth.getHP() * 0.3f);
            if (healHP > playerHealth.getCurrentHP())
            {
                playerHealth.setCurrentHP(playerHealth.getCurrentHP());
            }
            else
            {
                playerHealth.setCurrentHP(healHP);
            }

        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canRest = true;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag==("Player"))
        {
            canRest = false;
        }

    }
}
