using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class clearedBoard : MonoBehaviour
{
    public TMP_Text textMeshProObject;  // アルファ値を設定するTextMeshProオブジェクト
    public float alpha = 1.0f;          // アルファ値 (0.0 - 1.0)

    // Start is called before the first frame update
    void Start()
    {

    }
    void Awake()
    {
        Color color = textMeshProObject.color;
        color.a = Mathf.Clamp01(0.0f); // アルファ値を0から1に制限
        textMeshProObject.color = color;
        StartCoroutine("clearchecker");
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator clearchecker()
    {
        while (true)
        {
            if (GameObject.Find("GameManager").GetComponent<GameManager>().getCleared())
            {
                Color color = textMeshProObject.color;
                color.a = Mathf.Clamp01(1.0f); // アルファ値を0から1に制限
                textMeshProObject.color = color;
                Debug.Log("Cleard");
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }
}
