using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleHPBarManager : MonoBehaviour
{
    public GameObject[] CircleHPBar ;
    private int currentCircleBarCount = 0;
    private int nowCircleBarCount = 0;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void SetCircleBar(int num)
    {
        for (int i = 0; i < num; i++)
        {
            CircleHPBar[i].SetActive(false);
            if (i <num)
            {
                CircleHPBar[i].SetActive(true);
            }
            Debug.Log($"[CircleHPBarManager] SetCircleBar: {num} | CircleHPBar[{i}] active: {CircleHPBar[i].activeSelf}");
        }
        currentCircleBarCount = num;
    }
    public void UpdateCircleBar(int targetCircle,float ratio) {
        CircleHPBar[targetCircle-1].transform.Find("fill").GetComponent<Image>().fillAmount = ratio;


    }

}
