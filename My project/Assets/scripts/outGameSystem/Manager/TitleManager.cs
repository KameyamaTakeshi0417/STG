using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject CreditPanel;
    public static TitleManager Instance { get; private set; }
    public AudioSource SE_Decide,
        SE_Cancel,
        SE_Start,
        SE_ButtonChange;

    // Start is called before the first frame update
    void Start()
    {
        CreditPanel.SetActive(false);

        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded; // シーンロードイベントを追加
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // イベントの解除
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    // Update is called once per frame
    void Update() { }

    public void creditActivate(bool activate)
    {
        CreditPanel.SetActive(activate);
        if (activate)
        {
            SE_Decide.Play();
        }
        else
        {
            SE_Cancel.Play();
        }
    }

    public void DebugChangeScene(string name)
    {
        Debug.Log(name);
        StartCoroutine(sceneChangeOn(name));
    }

    private IEnumerator sceneChangeOn(string name)
    {
        if (SE_Start != null)
        {
            // SEを再生
            SE_Start.Play();

            // 再生中は待機
            while (SE_Start.isPlaying)
            {
                yield return null; // 次のフレームまで待機
            }
        }
        else
        {
            Debug.LogWarning("SE_Start is not assigned!");
        }

        // SEの再生終了後にシーンを切り替え
        SceneManager.LoadScene(name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}