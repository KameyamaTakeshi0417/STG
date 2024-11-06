using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class clearedBoard : MonoBehaviour
{
    public TMP_Text textMeshProObject;  // アルファ値を設定するTextMeshProオブジェクト
    public float alpha = 1.0f;          // アルファ値 (0.0 - 1.0)

    public float fadeInDuration = 0.25f; // フェードインの時間（秒）
    public float fadeOutDuration = 0.5f; // フェードアウトの時間（秒）
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
        while (GameObject.Find("GameManager").GetComponent<GameManager>().getCleared()==false)
        {
            yield return new WaitForSeconds(0.5f);
        }
        yield return FadeText();
    }
    private IEnumerator FadeText()
    {
        // フェードイン
        yield return StartCoroutine(FadeIn());

        // 少し待機
        yield return new WaitForSeconds(1.0f);

        // フェードアウト
        yield return StartCoroutine(FadeOut());

        yield return null;
    }
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        // 初期アルファ値を0に設定
        Color color = textMeshProObject.color;
        color.a = 0;
        textMeshProObject.color = color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
            textMeshProObject.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        // アルファ値を1に設定
        Color color = textMeshProObject.color;
        color.a = 1;
        textMeshProObject.color = color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsedTime / fadeOutDuration));
            textMeshProObject.color = color;
            yield return null;
        }
        
    }
}
