using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeBoard : MonoBehaviour
{
    public GameObject targetObject;
    private Image imageObject; // フェードさせるImageオブジェクト
    public float fadeInDuration = 2.0f; // フェードインの時間（秒）
    public float fadeOutDuration = 1.0f; // フェードアウトの時間（秒）
    public bool nowFade = false;

    void Start()
    {
        imageObject = targetObject.GetComponent<Image>();
        FadestartStage();
    }

    void Awake() { }

    public void callFadeScreen()
    {
        // フェードイン・フェードアウトを開始
        StartCoroutine(FadeImageCoroutine());
    }

    public void FadestartStage()
    {
        // フェードアウト
        StartCoroutine(FadeOut());
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

    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        nowFade = true;
        targetObject.SetActive(true);
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
        nowFade = false;
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
        targetObject.SetActive(false);
    }
}
