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
    }
}
