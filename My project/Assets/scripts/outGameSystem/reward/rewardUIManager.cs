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
    public int[] targetIndex;

    void Start()
    {
        targetRewardManager = GameObject.Find("GameManager").GetComponent<rewardManager>();
        // 対象のカメラをメインカメラに設定
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }
        else
        {
            Debug.LogError("Canvas component not found on this GameObject.");
        }
        freezeGame();
        // 配列から値を3つずつ取得してログに出力する処理
        setRewardUI(targetRewardManager.getRewardValues(RewardCount));
    }

    // Update is called once per frame
    void Update() { }

    public void ContinueGame()
    {
        UnfreezeGame();
    }

    protected void setRewardUI(int[] targetIndex)
    {
        GameObject[] targetObj = new GameObject[targetIndex.Length];
        for (int i = 0; i < targetIndex.Length; i++)
        {
            //   Random.InitState((int)Random.value);
            targetObj[i] = getObjByIndex(targetIndex[i]);
        }
        SetupSelection(targetObj[0], targetObj[1], targetObj[2], targetObj[3]);
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
        string createAmmoObj =
            "Objects/Reward/" + AmmoTypeArray[Random.Range(0, AmmoTypeArray.Length - 1)];
        createAmmoObj +=
            AmmoCategoryArray[Random.Range(0, AmmoCategoryArray.Length - 1)] + "_RewardObject";
        Debug.Log(createAmmoObj);
        switch (index)
        {
            case 1:
                //アンコモンアモ　弾のレアリティをここで決めるのか。4-6。
                rarelityValue = Random.Range(4, 7);

                break;
            case 2:
                //レアアモ レアリティを7-9
                rarelityValue = Random.Range(7, 10);
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
                break;
        }
        return ret;
    }

    public void setToButtonDestroy()
    {
        Destroy(this.gameObject);
    }

    // 選択を設定する
    public void SetupSelection(
        GameObject firstReward = null,
        GameObject secondReward = null,
        GameObject thirdReward = null,
        GameObject fourthReward = null
    )
    {
        if (selectionCanvas != null)
            selectionCanvas.SetActive(true); // 選択 UI を表示
        freezeGame();
        GameObject firstRewardImage = null;
        GameObject secondRewardImage = null;
        GameObject thirdRewardImage = null;
        GameObject fourthRewardImage = null;
        if (firstReward == null)
        {
            //報酬なしの表示ボードを出す処理
        }
        else if (firstReward != null && secondReward == null)
        {
            //報酬1こだけの処理
            CreateUIObj(firstRewardImage, firstReward, Vector2.zero);
        }
        else if (secondReward != null && thirdReward == null)
        {
            //報酬にこの処理
            CreateUIObj(firstRewardImage, firstReward, Vector2.left * 3);
            CreateUIObj(secondRewardImage, secondReward, Vector2.right * 3);
        }
        else if (thirdReward != null && fourthReward == null)
        {
            //報酬三この処理
            CreateUIObj(firstRewardImage, firstReward, Vector2.left * 4);
            CreateUIObj(secondRewardImage, secondReward, Vector3.zero);
            CreateUIObj(thirdRewardImage, thirdReward, Vector2.right * 4);
        }
        else if (fourthReward != null)
        {
            //報酬フルの処理
            CreateUIObj(
                firstRewardImage,
                firstReward,
                Vector2.left * 6,
                new Vector3(0.75f, 0.75f, 1f)
            );
            CreateUIObj(
                secondRewardImage,
                secondReward,
                Vector3.left * 3,
                new Vector3(0.75f, 0.75f, 1f)
            );
            CreateUIObj(
                thirdRewardImage,
                thirdReward,
                Vector2.right * 3,
                new Vector3(0.75f, 0.75f, 1f)
            );
            CreateUIObj(
                fourthRewardImage,
                fourthReward,
                Vector2.right * 5,
                new Vector3(0.75f, 0.75f, 1f)
            );
        }
    }

    // target を選択した際の処理

    void CreateUIObj(GameObject Obj, GameObject RObj, Vector2 pos, Vector3 scale = default)
    {
        if (scale == default)
        {
            scale = Vector3.one;
        }
        Obj = Instantiate(
            Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
            selectionCanvas.transform.position,
            Quaternion.identity
        );
        setAsChildUI(Obj, RObj, pos);
        Selecttarget(Obj, RObj);
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
        InventoryManager inventoryManager = gameObject.GetComponent<InventoryManager>();
        // キャンバスの子オブジェクトに設定
        targetUIObj.transform.SetParent(selectionCanvas.transform, false);

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
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 選択 UI を非表示にする
        }
    }

    //弾丸の種類をここに記そう
    public static string[] AmmoTypeArray = { "Normal", "Homing", "Pearcing", "Volt", "Explosion" }; //BloomBullet,OmniBulletは入れないかも
    public static string[] AmmoCategoryArray = { "Bullet", "Case", "Primer" };

    //仮レリックはコンボA,コンボB、単体で使うものを実装した。
    //コンボAは鈍足デメリットを無敵化で補い、コンボBは弾速すごいけど威力がカスになるのを固定ダメージで補うってもの
    public static string[] normalRelicArray = { "quipStep", "baseSkill", "mineralWater" }; //それぞれクイックステップ解禁、弾速修正、体力回復
    public static string[] UnCommonRelicArray = { "heavyBomb", "quickDraw", "powerBarrel" }; //走っていないとき威力修正、リロード修正、ダメージ補正
    public static string[] rareRelicArray = { "stoneWill", "aetherPowder", "enemyDevour" }; //走れない代わりにバリア展開、基本ダメージ0になるけど固定ダメージ追加、敵を倒したとき回復
}
