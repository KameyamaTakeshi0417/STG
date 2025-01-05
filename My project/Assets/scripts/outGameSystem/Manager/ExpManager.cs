using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExpManager : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    Player targetScript;
    int Exp;

    // Start is called before the first frame update
    void Awake()
    {
        targetScript = GameObject.Find("Player").GetComponent<Player>();
        Exp = targetScript.getExp();
        targetText.text = Exp.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Exp != targetScript.getExp())
        {
            Exp = targetScript.getExp();
            targetText.text = Exp.ToString();
        }
    }
}
