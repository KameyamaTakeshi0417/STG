using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class _EventHandlerBase : MonoBehaviour
{
    // Start is called before the first frame update
    public string[] useText;
    public GameObject[] ButtonUI;
    public GameObject quitButton;
    public TextMeshProUGUI textZone;

    public virtual void Init()
    {
        for (int i = 0; i < ButtonUI.Length; i++)
        {
            ButtonUI[i].SetActive(true);
        }
        quitButton.SetActive(false);
        textZone.text = useText[0];
    }

    public virtual void Action1() { }

    public virtual void Action2() { }

    public virtual void Action3() { }

    public virtual void Action4() { }

    public virtual void Action5() { }

    public virtual void QuitEvent()
    {
        quitEvent();
    }

    protected void changeActivateButton(int num, bool changeStance = false)
    {
        if (num > ButtonUI.Length)
        {
            Debug.Log("変数が多すぎる");
            return;
        }
        for (int i = 0; i < num; i++)
        {
            ButtonUI[i].SetActive(changeStance);
        }
    }

    public void quitEvent()
    {
        GameObject.Find("MisteryEvent").GetComponent<MisteryEventManager>().closeUI();
    }
}
