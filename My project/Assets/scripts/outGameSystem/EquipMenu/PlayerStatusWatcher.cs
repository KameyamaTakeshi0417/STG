using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatusWatcher : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] showUI;
    Player targetScript;
    PlayerHealth targetHealthScript;

    void Start() { }

    public void Init(GameObject PlayerObj)
    {
        targetScript = PlayerObj.GetComponent<Player>();
        SetText(showUI[0], targetHealthScript.HP.ToString());
        SetText(showUI[1], targetHealthScript.currentHP.ToString());
        SetText(showUI[2], targetScript.DamageMag.ToString());
        SetText(showUI[3], targetScript.DamageAdd.ToString());
        SetText(showUI[4], targetScript.BlockMag.ToString());
        SetText(showUI[5], targetScript.BlockDmg.ToString());
        SetText(showUI[6], targetScript.moveSpeed.ToString());
        SetText(showUI[7], targetScript.BulletSpan.ToString());
        SetText(showUI[8], targetScript.bulletSpeed.ToString());
    }

    // Update is called once per frame
    void Update() { }

    public void SetText(GameObject targetObj, string setText)
    {
        targetObj.GetComponent<TextMeshProUGUI>().text = setText;
    }

    public static string[] statusStringArray =
    {
        "HP",
        "currentHP",
        "damageRatio",
        "addDmg",
        "blockRatio",
        "addBlock",
        "playerSpeed",
        "coolTime",
        "bulletSpeed",
    };
}
