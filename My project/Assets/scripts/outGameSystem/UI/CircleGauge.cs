using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleGauge : MonoBehaviour
{
    public GameObject objHPGauge;
    private Image imgHPGauge;
    public Slider targetSlider;

    public float maxHp,
        hp;

    // Use this for initialization
    void Start()
    {
        imgHPGauge = objHPGauge.GetComponent<Image>();
    }

    public void setMaxHP(float value)
    {
        maxHp = value;
    }

    public void setCurrentHP(float value)
    {
        hp = value;
    }

    // Update is called once per frame
    void Update()
    {
        imgHPGauge.fillAmount = (float)hp / maxHp;
    }
}
