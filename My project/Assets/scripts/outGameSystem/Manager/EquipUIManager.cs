using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EquipUIManager : _Manager_Base
{
    public GameObject Relic1UI;
    public GameObject Relic2UI;
    public GameObject Relic3UI;
    public delegate void EquipChangedHandler(string updatedCategory, Sprite newSprite);
    public static event EquipChangedHandler OnEquipChanged;

    // Start is called before the first frame update
    void Start()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 初期状態で選択 UI を非表示にしておく
        }
    }

    void Awake()
    {
        // Canvas の取得

        // シーンロード完了時にカメラを再設定
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 初期カメラ設定
        AssignMainCamera();
        selectionCanvas.GetComponent<PlayerStatusWatcher>().Init(GameObject.Find("Player"));
        EquipManager targetManager = GetComponent<EquipManager>();
        GameObject targetObj = null;
        if (targetManager.activeBullet != null)
        {
            Debug.Log("呼び出し成功");
            targetObj = targetManager.activeBullet.GetComponent<ItemPickUp>().targetObj;
            selectionCanvas.GetComponent<BulletStatusWatcher>().BulletInit(targetObj);
        }
        if (targetManager.activeCase != null)
        {
            targetObj = targetManager.activeCase.GetComponent<ItemPickUp>().targetObj;
            selectionCanvas.GetComponent<BulletStatusWatcher>().CaseInit(targetObj);
        }
        if (targetManager.activePrimer != null)
        {
            targetObj = targetManager.activePrimer.GetComponent<ItemPickUp>().targetObj;
            selectionCanvas.GetComponent<BulletStatusWatcher>().PrimerInit(targetObj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CallEquipUI();
        }
    }

    public void CallEquipUI()
    {
        Time.timeScale = 0f;
        selectionCanvas.SetActive(true);
        selectionCanvas.GetComponent<PlayerStatusWatcher>().Init(GameObject.Find("Player"));
        EquipManager targetManager = GetComponent<EquipManager>();
        GameObject targetObj = null;
        if (targetManager.activeBullet != null)
        {
            Debug.Log("呼び出し成功");
            targetObj = targetManager.activeBullet.GetComponent<ItemPickUp>().targetObj;
            selectionCanvas.GetComponent<BulletStatusWatcher>().BulletInit(targetObj);
        }
        if (targetManager.activeCase != null)
        {
            targetObj = targetManager.activeCase.GetComponent<ItemPickUp>().targetObj;
            selectionCanvas.GetComponent<BulletStatusWatcher>().CaseInit(targetObj);
        }
        if (targetManager.activePrimer != null)
            setImageEquipMenu();
    }

    public void closeUI()
    {
        selectionCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void AssignMainCamera()
    {
        // 現在のシーンの Main Camera を取得
        Camera mainCamera = Camera.main;
        Canvas canvas = selectionCanvas.GetComponent<Canvas>();
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

    protected void setImageEquipMenu()
    {
        if (gameObject.GetComponent<EquipManager>().activeBullet != null)
        {
            activeBullet.GetComponent<Image>().sprite = setImage(
                gameObject.GetComponent<EquipManager>().activeBullet
            );
        }
        if (gameObject.GetComponent<EquipManager>().activeCase != null)
        {
            activeCase.GetComponent<Image>().sprite = setImage(
                gameObject.GetComponent<EquipManager>().activeCase
            );
        }

        if (gameObject.GetComponent<EquipManager>().activePrimer != null)
        {
            activePrimer.GetComponent<Image>().sprite = setImage(
                gameObject.GetComponent<EquipManager>().activePrimer
            );
        }

        if (gameObject.GetComponent<EquipManager>().subBullet != null)
        {
            subBullet.GetComponent<Image>().sprite = setImage(
                gameObject.GetComponent<EquipManager>().subBullet
            );
        }

        if (gameObject.GetComponent<EquipManager>().subCase != null)
        {
            subCase.GetComponent<Image>().sprite = setImage(
                gameObject.GetComponent<EquipManager>().subCase
            );
        }

        if (gameObject.GetComponent<EquipManager>().subPrimer != null)
        {
            subPrimer.GetComponent<Image>().sprite = setImage(
                gameObject.GetComponent<EquipManager>().subPrimer
            );
        }
    }

    private Sprite setImage(GameObject targetUI)
    {
        return targetUI.GetComponent<SpriteRenderer>().sprite;
    }

    public void EquipItem(GameObject item)
    {
        // 装備が変更された際にイベントを発行
        string tmpCategory = item.GetComponent<ItemPickUp>().itemType;
        Sprite tmpSprite = item.GetComponent<SpriteRenderer>().sprite;

        bool subChanged = false;
        EquipStackDecide decideScript = gameObject.GetComponent<EquipStackDecide>();
        switch (tmpCategory)
        {
            case "Bullet":
                if (useMainEquip)
                    if (activeBullet == null)
                    {
                        activeBullet = item;
                        // 新しいスプライトを設定
                        imageChange(item);
                    }
                    else if (subBullet == null)
                        subBullet = item;
                    else
                        decideScript.StartSelection(activeBullet, subBullet, item);
                break;
            case "Case":
                if (useMainEquip)
                    if (activeCase == null)
                    {
                        // 新しいスプライトを設定

                        imageChange(item);
                        activeCase = item;
                    }
                    else if (subCase == null)
                        subCase = item;
                    else
                        decideScript.StartSelection(activeCase, subCase, item);
                break;
            case "Primer":
                if (useMainEquip)
                    if (activePrimer == null)
                    {
                        // 新しいスプライトを設定

                        imageChange(item);
                        activePrimer = item;
                    }
                    else if (subPrimer == null)
                        subPrimer = item;
                    else
                        decideScript.StartSelection(activePrimer, subPrimer, item);
                break;
            default:
                Debug.LogWarning("Invalid item type for equip");
                break;
        }
        if (subChanged == true)
        {
            return;
        }
    }

    public void imageChange(GameObject targetObj)
    {
        string tmpCategory = targetObj.GetComponent<ItemPickUp>().itemType;
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
        Sprite tmpSprite = targetObj.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, tmpSprite);
        foreach (UIImageChanger imageChanger in imageChangers)
        {
            if (imageChanger.itemCategory == tmpCategory)
            {
                imageChanger.newSprite = tmpSprite;
            }
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
