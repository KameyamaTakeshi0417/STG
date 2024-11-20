using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class RewardStackDecide : _PopUpDecideUI_Base
{
   // public GameObject ImageBoardBase;

    void Start()
    {
        /*
       Transform parentTransform = GameObject.Find("GameManager").transform;
       Transform targetChild = parentTransform.Find("RewardSelectCanvas");
       selectionCanvas = targetChild.gameObject;
*/

    }

    // 選択画面を開始する
    public void StartSelection(
        GameObject firstReward = null,
        GameObject secondReward = null,
        GameObject thirdReward = null,
        GameObject fourthReward = null
    )
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(true); // 選択 UI を表示
        }
        freezeGame();
        SetupSelection(firstReward, secondReward, thirdReward, fourthReward);
    }

    // 選択を設定する
    public void SetupSelection(
        GameObject firstReward = null,
        GameObject secondReward = null,
        GameObject thirdReward = null,
        GameObject fourthReward = null
    )
    {
        GameObject firstRewardImage;
        GameObject secondRewardImage;
        GameObject thirdRewardImage;
        GameObject fourthRewardImage;
        if (firstReward == null)
        {
            //報酬なしの表示ボードを出す処理
        }
        else if (firstReward != null && secondReward == null)
        {
            //報酬1こだけの処理
            firstRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(firstRewardImage, firstReward, Vector2.zero);
        }
        else if (secondReward != null && thirdReward == null)
        {
            //報酬にこの処理
            firstRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(firstRewardImage, firstReward, new Vector2(-3, 0));
            secondRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(secondRewardImage, secondReward, new Vector2(3, 0));
        }
        else if (thirdReward != null && fourthReward == null)
        {
            //報酬三この処理
            firstRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(firstRewardImage, firstReward, Vector2.left * 4);

            secondRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(secondRewardImage, secondReward, Vector3.zero);
            Selecttarget(secondRewardImage, secondReward);

            thirdRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(thirdRewardImage, thirdReward, Vector2.right * 4);
            Selecttarget(thirdRewardImage, thirdReward);
        }
        else if (fourthReward != null)
        {
            //報酬フルの処理
            firstRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(
                firstRewardImage,
                firstReward,
                Vector2.left * 6,
                new Vector3(0.75f, 0.75f, 1f)
            );

            secondRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(
                secondRewardImage,
                secondReward,
                Vector3.left * 3,
                new Vector3(0.75f, 0.75f, 1f)
            );

            thirdRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(
                thirdRewardImage,
                thirdReward,
                Vector2.right * 3,
                new Vector3(0.75f, 0.75f, 1f)
            );

            fourthRewardImage = Instantiate(
                Resources.Load<GameObject>("Objects/Reward/RewardBasicBoard"),
                selectionCanvas.transform.position,
                Quaternion.identity
            );
            setAsChildUI(
                fourthRewardImage,
                fourthReward,
                Vector2.right * 6,
                new Vector3(0.75f, 0.75f, 1f)
            );
        }
   
    }

    // target を選択した際の処理
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
}
