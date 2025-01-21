using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipStackDecide : _PopUpDecideUI_Base
{
    public GameObject mainEquipImage;
    public GameObject subEquipImage;
    public GameObject targetEquipImage;

    void Start()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 初期状態で選択 UI を非表示にしておく
        }
    }

    // 選択画面を開始する
    public void StartSelection(GameObject mainObject, GameObject subtarget, GameObject targetObject)
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(true); // 選択 UI を表示
        }
        freezeGame();
        SetupSelection(mainObject, subtarget, targetObject);
    }

    // 選択を設定する
    private void SetupSelection(
        GameObject mainObject,
        GameObject subObject,
        GameObject targetObject
    )
    {
        EquipManager equipManager = gameObject.GetComponent<EquipManager>();
        if (equipManager != null)
        {
            //   equipManager.EquipItemtoMain(targetObject);
        }
        Selecttarget(mainEquipImage, mainObject);
        Selecttarget(subEquipImage, subObject);
        Selecttarget(targetEquipImage, targetObject);

        // ボタンにリスナーを追加
        Button mainButton = mainEquipImage.GetComponent<Button>();
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => Selecttarget(mainEquipImage, mainObject));
            mainButton.onClick.AddListener(() => equipManager.EquipItemtoMain(targetObject));
            mainButton.onClick.AddListener(() => continueGame());
        }

        Button subButton = subEquipImage.GetComponent<Button>();
        if (subButton != null)
        {
            subButton.onClick.RemoveAllListeners();
            subButton.onClick.AddListener(() => equipManager.EquipItemtoSub(targetObject));
            subButton.onClick.AddListener(() => continueGame());
        }

        Button targetButton = targetEquipImage.GetComponent<Button>();
        if (targetButton != null)
        {
            targetButton.onClick.RemoveAllListeners();
            // targetButton.onClick.AddListener(() => equipManager.EquipItemtoMain(targetObject));
            targetButton.onClick.AddListener(() => continueGame());
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
}
