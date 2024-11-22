using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testscript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject scrollUI; // スクロールバーUIの親要素
    public GameObject elementPrefab; // 各要素のプレハブ

    void Start()
    {
        if (scrollUI == null || elementPrefab == null)
        {
            Debug.LogError("必要なコンポーネントが設定されていません");
            return;
        }

        // 各ゲームオブジェクトをスクロールUIに追加
        foreach (
            GameObject obj in GameObject
                .Find("GameManager")
                .GetComponent<InventoryManager>()
                .AmmoObjectList
        )
        {
            CreateScrollElement(obj);
        }
    }

    void CreateScrollElement(GameObject obj)
    {
        // 要素を生成し、スクロールバーUIに追加
        GameObject newElement = Instantiate(elementPrefab, scrollUI.transform);

        // スプライトと名前を設定
        Image spriteImage = newElement.transform.Find("SpriteImage").GetComponent<Image>();
        Text nameText = newElement.transform.Find("NameText").GetComponent<Text>();

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
            button.onClick.AddListener(() => OnElementClicked(obj));
        }
    }

    void OnElementClicked(GameObject obj)
    {
        Debug.Log("Clicked on: " + obj.name);
    }
}
