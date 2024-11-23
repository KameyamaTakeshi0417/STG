using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class equipUIChangeCanvasManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject scrollUI; // スクロールバーUIの親要素
    public GameObject elementPrefab; // 各要素のプレハブ
    public string targetObjCategory;
    public string AmmoType;
    private bool isInitialized = false; // 初期化されたかどうかを確認するフラグ

    public void Initialize(string category)
    {
        targetObjCategory = category;
        isInitialized = true; // 初期化完了
    }

    void Awake()
    {
        // Awake は Start より先に呼ばれる
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
            Debug.Log("Canvas is set with the main camera.");
        }
        else
        {
            Debug.LogError("Canvas component not found on this GameObject.");
        }
    }

    void Start()
    {
        // 各ゲームオブジェクトをスクロールUIに追加
        foreach (
            GameObject obj in GameObject
                .Find("GameManager")
                .GetComponent<InventoryManager>()
                .AmmoObjectList
        )
        {
            if (obj.GetComponent<ItemPickUp>().itemType == AmmoType)
            {
                CreateScrollElement(obj);
            }
        }
    }

    void CreateScrollElement(GameObject obj)
    {
        // 要素を生成し、スクロールバーUIに追加
        GameObject newElement = Instantiate(elementPrefab, scrollUI.transform);

        // スプライトと名前を設定
        Image spriteImage = newElement.transform.Find("Image").GetComponent<Image>();
        TextMesh nameText = newElement.transform.Find("objectExplain").GetComponent<TextMesh>();

        if (spriteImage != null && nameText != null)
        {
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteImage.sprite = spriteRenderer.sprite;
            }
            nameText.text = obj.name;
        }

        // クリックイベントを設定
        Button button = newElement.GetComponent<Button>();
        if (button != null)
        {
            if (targetObjCategory == "active")
            {
                button.onClick.AddListener(
                    () =>
                        GameObject
                            .Find("GameManager")
                            .GetComponent<EquipManager>()
                            .EquipItemtoMain(obj)
                );
            }
            else if (targetObjCategory == "sub")
            {
                button.onClick.AddListener(
                    () =>
                        GameObject
                            .Find("GameManager")
                            .GetComponent<EquipManager>()
                            .EquipItemtoSub(obj)
                );
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
        Destroy(gameObject.transform.root);
    }
}
