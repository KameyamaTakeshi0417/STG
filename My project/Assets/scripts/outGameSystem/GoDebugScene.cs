using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoDebugScene : MonoBehaviour
{
    // Start is called before the first frame update
    public bool Onstep;
    public bool canEnter;
    public int stepNum;
    public string nextScene;

    // Start is called before the first frame update
    void Start()
    {
        Onstep = false;
    }

    // Update is called once per frame
    void Update() { }

    public void setEnterable(bool set)
    {
        canEnter = set;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().DebugChangeScene(nextScene);
        }
    }
}
