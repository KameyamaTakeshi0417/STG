using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleHPBarManager : MonoBehaviour
{
    public GameObject[] CircleHPBar ;
    private int currentCircleBarCount = 0;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void SetCircleBar(int num)
    {
        for (int i = 0; i < CircleHPBar.Length; i++)
        {
            CircleHPBar[i].SetActive(i < num);
        }
        currentCircleBarCount = num;
    }
    public void UpdateCircleBar(int targetCircle,float ratio) {
        if (targetCircle - 1 < 0 || targetCircle - 1 >= CircleHPBar.Length) return;
        CircleHPBar[targetCircle-1].transform.Find("fill").GetComponent<Image>().fillAmount = ratio;
    }

}
