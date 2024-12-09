using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ROUNDSTART : MonoBehaviour
{
    public TMP_Text textMeshProObject; // フェードさせるTextMeshProオブジェクト
    public float fadeInDuration = 2.0f; // フェードインの時間（秒）
    public float fadeOutDuration = 1.0f; // フェードアウトの時間（秒）

    void Start()
    {
        // フェードイン・フェードアウトを開始
        StartCoroutine(FadeText());
    }

    private IEnumerator FadeText()
    {
        // フェードイン
        yield return StartCoroutine(FadeIn());

        // 少し待機
        yield return new WaitForSeconds(1.0f);

        // フェードアウト
        yield return StartCoroutine(FadeOut());
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
