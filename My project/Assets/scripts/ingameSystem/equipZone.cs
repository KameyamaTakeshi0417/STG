using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equipZone : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
           GameObject.Find("GameManager").GetComponent<EquipUIManager>().CallEquipUI();

        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {


    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}
