using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class equipMenuManager : MonoBehaviour
{
    public GameObject selectEquipScrollUI;

    // Start is called before the first frame update
    void Awake()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
            Debug.Log("startRewarUI");
        }
        else
        {
            Debug.LogError("Canvas component not found on this GameObject.");
        }
    }

    void Start()
    {
        rewardManager targetRewardManager = GameObject
            .Find("GameManager")
            .GetComponent<rewardManager>();
        // 対象のカメラをメインカメラに設定
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
            Debug.Log("startRewarUI");
        }
        else
        {
            Debug.LogError("Canvas component not found on this GameObject.");
        }
    }

    // Update is called once per frame
    void OnEnable()
    {
        AssignCamera();
    }

    public void closeUI()
    {
        GameObject targetUICanvas = GameObject.Find("equipChangeCanvas(Clone)");
        if (targetUICanvas != null)
        {
            Destroy(targetUICanvas);
        }
        this.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    void AssignCamera()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            Camera mainCamera = FindMainCameraInActiveScene();
            if (mainCamera != null)
            {
                canvas.worldCamera = Camera.main;
                Debug.Log("Camera assigned successfully.");
            }
            else
            {
                Debug.LogWarning("MainCamera not found in scene.");
            }
        }
        else
        {
            Debug.LogError("Canvas component not found on this GameObject.");
        }
    }

    public Camera FindMainCameraInActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log("nowScene:" + activeScene.name);
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == "MainCamera")
            {
                Camera cameraComponent = rootObject.GetComponent<Camera>();
                if (cameraComponent != null)
                {
                    Debug.Log("Found MainCamera in active scene.");
                    return cameraComponent;
                }
            }
        }

        Debug.LogWarning("MainCamera not found in active scene.");
        return null;
    }

    public void callEquipScrollBar(string categoryName, string categoryType)
    {
        //activeかsubか設定しておく
        GameObject UIPrefab = Instantiate(selectEquipScrollUI, Vector3.zero, Quaternion.identity);
        UIPrefab.GetComponent<equipUIChangeCanvasManager>().Initialize(categoryName, categoryType); // ここで targetObjCategory を設定
    }
}
