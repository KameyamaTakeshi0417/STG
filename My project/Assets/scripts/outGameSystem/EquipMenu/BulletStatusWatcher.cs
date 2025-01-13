using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BulletStatusWatcher : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] showUI;

    void Start() { }

    public void BulletInit(GameObject BulletObj)
    {
        Bullet_Base targetScript = BulletObj.GetComponent<Bullet_Base>();
        SetText(showUI[0], BulletObj.name);
        SetText(showUI[1], targetScript.dmg.ToString());
        Debug.Log("BulletStats呼び出せてる");
        return;
    }

    public void CaseInit(GameObject CaseObj)
    {
        Case_Base targetScript = CaseObj.GetComponent<Case_Base>();
        SetText(showUI[2], CaseObj.name);
        SetText(showUI[3], targetScript.dmg.ToString());
        return;
    }

    public void PrimerInit(GameObject PrimerObj)
    {
        Primer_Base targetScript = PrimerObj.GetComponent<Primer_Base>();
        SetText(showUI[4], PrimerObj.name);
        SetText(showUI[5], targetScript.pow.ToString());
        return;
    }

    // Update is called once per frame
    void Update() { }

    public void SetText(GameObject targetObj, string setText)
    {
        targetObj.GetComponent<TextMeshProUGUI>().text = setText;
    }
}
