using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class EquipStackDecide : MonoBehaviour
{
    public GameObject mainEquipImage;
    public GameObject subEquipImage;
    public GameObject targetEquipImage;
    public GameObject selectionCanvas; // 選択用の Canvas

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
    private void SetupSelection(GameObject mainObject, GameObject subObject, GameObject targetObject)
    {
        EquipManager equipManager = gameObject.GetComponent<EquipManager>();
        if (equipManager != null)
        {
            equipManager.EquipItemtoMain(targetObject);
        }
        Selecttarget(mainEquipImage, mainObject);
        Selecttarget(subEquipImage, subObject);
        Selecttarget(targetEquipImage, targetObject);

        // ボタンにリスナーを追加
        Button mainButton = mainEquipImage.GetComponent<Button>();
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => equipManager.EquipItemtoMain(mainObject));
            mainButton.onClick.AddListener(() => continueGame());
        }

        Button subButton = subEquipImage.GetComponent<Button>();
        if (subButton != null)
        {
            subButton.onClick.RemoveAllListeners();
            subButton.onClick.AddListener(() => equipManager.EquipItemtoMain(subObject));
            subButton.onClick.AddListener(() => continueGame());
        }

        Button targetButton = targetEquipImage.GetComponent<Button>();
        if (targetButton != null)
        {
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(() => equipManager.EquipItemtoMain(targetObject));
            targetButton.onClick.AddListener(() => continueGame());
        }

        
    }

    public void freezeGame()
    {
        Time.timeScale = 0f; // ゲームの時間を停止
    }

    public void continueGame()
    {
        Time.timeScale = 1f; // ゲームの時間を再開
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 選択 UI を非表示にする
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
