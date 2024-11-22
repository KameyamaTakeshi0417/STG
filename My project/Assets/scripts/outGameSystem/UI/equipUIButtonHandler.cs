using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class equipUIButtonHandler : MonoBehaviour
{
    public InventoryManager inventoryManager; // インベントリの参照
    public ScrollRect scrollView; // スクロールビューの参照
    public GameObject buttonPrefab; // 各アイテムを表示するボタンのプレハブ
    public string targetObjCategory; // フィルタするカテゴリ名
    public Button showScrollViewButton; // スクロールビューを表示するボタン

    void OnEnable()
    {
        inventoryManager = GameObject.Find("GameManager").GetComponent<InventoryManager>();
        //gameObject
    }

    void Start()
    {
        // ボタンがクリックされたときにスクロールビューを表示する
        showScrollViewButton.onClick.AddListener(() => ShowScrollView());
    }

    public void ShowScrollView()
    {
        // スクロールビューを表示
        scrollView.gameObject.SetActive(true);
        // ScrollViewにインベントリのアイテムを表示
        PopulateScrollView();
    }

    public void PopulateScrollView()
    {
        // 既存の子オブジェクトを削除
        foreach (Transform child in scrollView.content)
        {
            Destroy(child.gameObject);
        }

        // InventoryManagerからアイテムを取得
        List<GameObject> items = inventoryManager.AmmoObjectList;
        foreach (GameObject item in items)
        {
            ItemPickUp itemPickUp = item.GetComponent<ItemPickUp>();
            if (itemPickUp != null && itemPickUp.itemType == targetObjCategory)
            {
                // ボタンを作成し、ScrollViewに追加
                GameObject newButton = Instantiate(buttonPrefab, scrollView.content);
                newButton.GetComponentInChildren<Text>().text = item.name;

                // ボタンがクリックされたときのイベントを設定
                Button buttonComponent = newButton.GetComponent<Button>();
                buttonComponent.onClick.AddListener(() => OnItemButtonClicked(item));
            }
        }
    }

    private void OnItemButtonClicked(GameObject item)
    {
        // 選択されたアイテムの名前をログに出力
        Debug.Log("Selected Item: " + item.name);
    }
}
