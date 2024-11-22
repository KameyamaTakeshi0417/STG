using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rewardUIManager : _Manager_Base
{
    // Start is called before the first frame update
    public Camera targetCamera; // 対象のカメラ
    public rewardManager targetRewardManager; // RewardManagerの参照
    private int RewardCount = 3; // 現在のインデックス
    public GameObject spriteObj;
    public int[] targetIndex;
    public List<GameObject> temporaryList = new List<GameObject>();
    public List<GameObject> rewardObjects = new List<GameObject>();

    void Start()
    {
        targetRewardManager = GameObject.Find("GameManager").GetComponent<rewardManager>();
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
        temporaryList.Clear();
        freezeGame();
        // 配列から値を3つずつ取得してログに出力する処理
        setRewardUI(targetRewardManager.getRewardValues(RewardCount));
    }

    // Update is called once per frame

    protected void setRewardUI(int[] targetIndex)
    {
        rewardObjects.Clear();
        for (int i = 0; i < targetIndex.Length; i++)
        {
            GameObject rewardObject = getObjByIndex(targetIndex[i]);
            if (rewardObject != null)
            {
                rewardObjects.Add(rewardObject);
            }
        }
        SetupSelection(rewardObjects);
    }

    public GameObject getObjByIndex(int index)
    {
        GameObject ret = null;
        int rarelityValue = 0;
        // 0=コモンAmmo
        //1=アンコモンAmmo
        //2=レアAmmo
        //3=ノーマルレリック
        //4=アンコモンレリック
        //5=レアレリック
        Random.InitState((int)System.DateTime.Now.Ticks);
        string createAmmoObj =
            "Objects/Reward/" + AmmoTypeArray[Random.Range(0, AmmoTypeArray.Length)];
        createAmmoObj +=
            AmmoCategoryArray[Random.Range(0, AmmoCategoryArray.Length)] + "_RewardObject";

        GameObject prefab = Resources.Load<GameObject>(createAmmoObj);
        temporaryList.Add(prefab);
        switch (index)
        {
            case 1:
                //アンコモンアモ　弾のレアリティをここで決めるのか。4-6。
                rarelityValue = Random.Range(4, 7);
                prefab.GetComponent<ItemPickUp>().itemRarelity = rarelityValue;
                break;
            case 2:
                //レアアモ レアリティを7-9
                rarelityValue = Random.Range(7, 10);
                prefab.GetComponent<ItemPickUp>().itemRarelity = rarelityValue;
                break;
            //いったんバレットのみの出力を試す。これできたらレリック作る
            case 3: //ノーマルレリック
            //break;
            case 4: //アンコモンレリック
            //break;
            case 5: //レアレリック
            // break;
            case 0:
            default:
                //コモンアモ作成 1-3の値を付与する
                rarelityValue = Random.Range(1, 4);
                prefab.GetComponent<ItemPickUp>().itemRarelity = rarelityValue;
                break;
        }
        Debug.Log(createAmmoObj + "rarelity" + prefab.GetComponent<ItemPickUp>().itemRarelity);
        ret = prefab;
        return ret;
    }

    // 選択を設定する
    public void SetupSelection(List<GameObject> rewards)
    {
        GameObject ui = Instantiate(
            spriteObj,
            this.gameObject.transform.position,
            Quaternion.identity
        );
        Debug.Log("setupSelectionStart" + ui.name);
        freezeGame();

        Vector3 scaleMag=Vector3.one*500f;
        int LayoutMag=100;
        switch (rewards.Count)
        {
            case 1:
                CreateUIObj(rewards[0], Vector2.zero,scaleMag);
                break;
            case 2:
                CreateUIObj(rewards[0], Vector2.left * 3*LayoutMag,scaleMag);
                CreateUIObj(rewards[1], Vector2.right * 3*LayoutMag,scaleMag);
                break;
            case 3:
                CreateUIObj(rewards[0], Vector2.left * 4*LayoutMag,scaleMag);
                CreateUIObj(rewards[1], Vector3.zero,scaleMag);
                CreateUIObj(rewards[2], Vector2.right * 4*LayoutMag,scaleMag);
                Debug.Log("setupSelection_End");
                break;
            case 4:
                CreateUIObj(rewards[0], Vector2.left * 6*LayoutMag, scaleMag*0.75f);
                CreateUIObj(rewards[1], Vector3.left * 3*LayoutMag, scaleMag*0.75f);
                CreateUIObj(rewards[2], Vector2.right * 3*LayoutMag, scaleMag*0.75f);
                CreateUIObj(rewards[3], Vector2.right * 5*LayoutMag, scaleMag*0.75f);
                break;
        }
    }

    // target を選択した際の処理

    GameObject CreateUIObj(GameObject RObj, Vector2 pos, Vector3 scale = default)
    {
        Debug.Log("CreateUIObj_Start");
        GameObject Obj = null;
        if (scale == default)
        {
            scale = Vector3.one;
        }
        Obj = Instantiate(
            Resources.Load<GameObject>("Objects/Reward/Relic/RewardBasicBoard"),
            this.gameObject.transform.localPosition,
            Quaternion.identity
        );
        if (Obj == null)
        {
            Debug.LogError("Prefab could not be loaded. Please check the path.");
            return null;
        }
        Debug.Log("CreateUIObj_End");
        setAsChildUI(Obj, RObj, pos);
        Selecttarget(Obj, RObj);
        return Obj;
    }

    private void setAsChildUI(
        GameObject targetUIObj,
        GameObject targetRewardObj,
        Vector2 targetPosition,
        Vector3 targetScale = default
    )
    {
        // デフォルト引数として Vector3.one を使用
        if (targetScale == default)
        {
            targetScale = Vector3.one;
        }
        InventoryManager inventoryManager = GameObject.Find("GameManager").GetComponent<InventoryManager>();
        // キャンバスの子オブジェクトに設定
        targetUIObj.transform.SetParent(this.gameObject.transform, false);

        // RectTransformの初期設定を行う（キャンバス内で適切に配置されるようにする）
        RectTransform rectTransform = targetUIObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = targetPosition; // 初期位置を設定
            rectTransform.localScale = targetScale; // スケールを設定
        }
        Selecttarget(targetUIObj, targetRewardObj);
        // ボタンにリスナーを追加
        Button targetButton = targetUIObj.GetComponent<Button>();
        if (targetButton != null)
        {
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(() => Selecttarget(targetUIObj, targetRewardObj));
            targetButton.onClick.AddListener(() => inventoryManager.AddItem(targetUIObj));
            targetButton.onClick.AddListener(() => continueGame());
        }
    }

    private void Selecttarget(GameObject selectedObj, GameObject ImageObj)
    {
        // 選択した target を反映（必要な処理を行う）
        Debug.Log("Selected target: " + selectedObj.name);

        // 選択したターゲットのスプライトを変更する
        SpriteRenderer spriteRenderer = ImageObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            selectedObj.GetComponent<Image>().sprite = spriteRenderer.sprite;
        }
    }

    public void continueGame()
    {
        Time.timeScale = 1f; // ゲームの時間を再開

        Destroy(this.gameObject); // 選択 UI を非表示にする
    }

    public void setToButtonDestroy()
    {
        Destroy(this.gameObject);
    }

    //弾丸の種類をここに記そう
    public static string[] AmmoTypeArray = { "Normal", "Homing", "Piercing", "Volt", "Explosion" }; //BloomBullet,OmniBulletは入れないかも
    public static string[] AmmoCategoryArray = { "Bullet", "Case", "Primer" };

    //仮レリックはコンボA,コンボB、単体で使うものを実装した。
    //コンボAは鈍足デメリットを無敵化で補い、コンボBは弾速すごいけど威力がカスになるのを固定ダメージで補うってもの
    public static string[] normalRelicArray = { "quipStep", "baseSkill", "mineralWater" }; //それぞれクイックステップ解禁、弾速修正、体力回復
    public static string[] UnCommonRelicArray = { "heavyBomb", "quickDraw", "powerBarrel" }; //走っていないとき威力修正、リロード修正、ダメージ補正
    public static string[] rareRelicArray = { "stoneWill", "aetherPowder", "enemyDevour" }; //走れない代わりにバリア展開、基本ダメージ0になるけど固定ダメージ追加、敵を倒したとき回復
}