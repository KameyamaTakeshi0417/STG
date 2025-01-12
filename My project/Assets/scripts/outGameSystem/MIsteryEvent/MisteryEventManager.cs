using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MisteryEventManager : MonoBehaviour
{
    public GameObject targetCanvas;
    public bool check;

    // Start is called before the first frame update
    void Start() { }

    void Awake()
    {
        targetCanvas.GetComponent<_EventHandlerBase>().Init();
        targetCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (check == true && Input.GetKeyDown(KeyCode.Return))
        {
            targetCanvas.SetActive(true);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.Return))
        {
            targetCanvas.SetActive(true);
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            check = true;
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            check = false;
        }
    }

    public void closeUI()
    {
        targetCanvas.SetActive(false);
    }
}
