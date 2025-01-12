using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUIManager : MonoBehaviour
{
    public Canvas canvas;

    void Awake()
    {
        // Canvas の取得
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas component not found!");
            return;
        }

        // シーンロード完了時にカメラを再設定
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 初期カメラ設定
        AssignMainCamera();
    }

    private void AssignMainCamera()
    {
        // 現在のシーンの Main Camera を取得
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera; // 必要に応じて設定
            canvas.worldCamera = mainCamera;
            Debug.Log("Main Camera assigned to Canvas: " + mainCamera.name);
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンロード後にカメラを再設定
        Debug.Log("Scene loaded: " + scene.name);
        AssignMainCamera();
    }

    private void OnDestroy()
    {
        // イベントリスナーの解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
