using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class _EventHandlerBase : MonoBehaviour
{
    // Start is called before the first frame update
    public string[] useText;
    GameObject[] ButtonUI;
    public TextMeshProUGUI textZone;

    public virtual void Init()
    {
        textZone.text = useText[0];
    }

    public virtual void Action1() { }

    public virtual void Action2() { }

    public virtual void Action3() { }

    public virtual void Action4() { }

    public virtual void Action5() { }

    public void quitEvent()
    {
        GameObject.Find("MysteryEventManager").GetComponent<MisteryEventManager>().closeUI();
    }
}
