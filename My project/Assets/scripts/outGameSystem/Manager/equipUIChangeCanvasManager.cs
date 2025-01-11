using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class equipUIChangeCanvasManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject scrollUI; // スクロールバーUIの親要素
    public GameObject elementPrefab; // 各要素のプレハブ
    public string targetObjCategory;
    public string targetMode; //activeかsubか
    public string AmmoType;
    private bool isInitialized = false; // 初期化されたかどうかを確認するフラグ
    public GameObject ViewPort;
    Canvas MyCanvas;

    public void Initialize(string category, string categoryType)
    {
        targetObjCategory = categoryType;
        targetMode = category;
        MyCanvas = gameObject.GetComponent<Canvas>();
        MyCanvas.sortingLayerName = "UI";
        MyCanvas.sortingOrder = 100;

        // 各ゲームオブジェクトをスクロールUIに追加
        foreach (
            GameObject obj in GameObject
                .Find("GameManager")
                .GetComponent<InventoryManager>()
                .AmmoObjectList
        )
        {
            if (obj.GetComponent<ItemPickUp>().itemType == targetObjCategory)
            {
                CreateScrollElement(obj);
            }
        }
        isInitialized = true; // 初期化完了
    }

    void Awake()
    {
        Debug.Log("CallObj:" + gameObject.name);
        // Awake は Start より先に呼ばれる
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
            Camera[] cameras = Camera.allCameras;
            foreach (Camera cam in cameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    // MainCameraを取得
                    Camera mainCamera = cam;
                    canvas.worldCamera = mainCamera;
                    break;
                }
            }

            Debug.Log("Canvas is set with the main camera.");
        }
        else
        {
            Debug.LogError("Canvas component not found on this GameObject.");
        }
    }

    void Start() { }

    void CreateScrollElement(GameObject obj)
    {
        // 要素を生成し、スクロールバーUIに追加
        GameObject newElement = Instantiate(elementPrefab, ViewPort.transform);
        if (newElement == null)
        {
            Debug.Log("エレメント取得できてない");
        }
        // スプライトと名前を設定
        Image spriteImage = newElement.transform.Find("ObjImage").GetComponent<Image>();

        if (spriteImage == null)
        {
            Debug.LogError("ObjImage not found in the new element!");
            return;
        }

        // クリックイベントを設定
        Button button = newElement.GetComponent<Button>();
        if (button != null)
        {
            switch (targetMode)
            {
                case "active":
                    button.onClick.AddListener(
                        () =>
                            GameObject
                                .Find("GameManager")
                                .GetComponent<EquipManager>()
                                .EquipItemtoMain(obj)
                    );
                    break;
                case "sub":
                    button.onClick.AddListener(
                        () =>
                            GameObject
                                .Find("GameManager")
                                .GetComponent<EquipManager>()
                                .EquipItemtoSub(obj)
                    );
                    break;
                case "first":
                    button.onClick.AddListener(
                        () =>
                            GameObject
                                .Find("GameManager")
                                .GetComponent<EquipManager>()
                                .EquipItemtoRelic(obj, targetMode)
                    );
                    break;
                case "second":
                    button.onClick.AddListener(
                        () =>
                            GameObject
                                .Find("GameManager")
                                .GetComponent<EquipManager>()
                                .EquipItemtoRelic(obj, targetMode)
                    );
                    break;
                case "third":
                    button.onClick.AddListener(
                        () =>
                            GameObject
                                .Find("GameManager")
                                .GetComponent<EquipManager>()
                                .EquipItemtoRelic(obj, targetMode)
                    );
                    break;
                default:
                    break;
            }
            button.onClick.AddListener(() => CloseUI());
        }
    }

    void OnElementClicked(GameObject obj)
    {
        Debug.Log("Clicked on: " + obj.name);
    }

    public void CloseUI()
    {
        Destroy(this.gameObject);
    }
}
