using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MisteryEventManager : MonoBehaviour
{
    public GameObject targetCanvas;

    // Start is called before the first frame update
    void Start() { }

    void Awake()
    {
        targetCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update() { }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.Return))
        {
            targetCanvas.SetActive(true);
        }
    }
}
