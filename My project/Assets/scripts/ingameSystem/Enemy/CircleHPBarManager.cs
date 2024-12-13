using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleHPBarManager : MonoBehaviour
{
    public GameObject[] CircleHPBar = new GameObject[3];

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void SetCircleBar(int num)
    {
        for (int i = 0; i < 3; i++)
        {
            CircleHPBar[i].SetActive(false);
            if (i < num)
            {
                CircleHPBar[i].SetActive(true);
            }
        }
    }
}
