using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSceneManager : MonoBehaviour
{
    public bool cleared;

    // Start is called before the first frame update
    void Awake()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().setCleared(cleared);
    }

    // Update is called once per frame
    void Update() { }
}
