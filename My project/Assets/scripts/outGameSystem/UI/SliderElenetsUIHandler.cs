using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderElenetsUIHandler : MonoBehaviour
{
    public string NameText;
    public string explainText;

    public TMP_Text NameObj;
    public TMP_Text explainObj;
    public Image myImageObj;

    // Start is called before the first frame update
    void Start() { }

    public void Init(string ObjName, string ObjExplain, SpriteRenderer ObjImage)
    {
        NameText = ObjName;
        explainText = ObjExplain;
        NameObj.text = NameText;
        //explainObj.text = explainText;
        myImageObj.sprite = ObjImage.sprite;
    }

    // Update is called once per frame
    void Update() { }
}
