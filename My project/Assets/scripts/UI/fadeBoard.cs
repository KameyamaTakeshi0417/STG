using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeBoard : MonoBehaviour
{
    public Image imageObject;       // フェードさせるImageオブジェクト
    public float fadeInDuration = 2.0f;  // フェードインの時間（秒）
    public float fadeOutDuration = 1.0f; // フェードアウトの時間（秒）

    void Start()
    {

    }
    public void callFadeScreen()
    {
        // フェードイン・フェードアウトを開始
        StartCoroutine(FadeImageCoroutine());
    }
    private IEnumerator FadeImageCoroutine()
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
        Color color = imageObject.color;
        color.a = 0;
        imageObject.color = color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
            imageObject.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        // アルファ値を1に設定
        Color color = imageObject.color;
        color.a = 1;
        imageObject.color = color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsedTime / fadeOutDuration));
            imageObject.color = color;
            yield return null;
        }
    }
}
