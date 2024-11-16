using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class EquipStackDecide : MonoBehaviour
{
    public GameObject selectionPanel; // target 選択用のパネル

    public GameObject mainEquipImage;
    public GameObject subEquipImage;
    public GameObject targetEquipImage;

    void Start()
    {
        selectionPanel.SetActive(false); // 初期状態で選択 UI を非表示にしておく
    }

    // 選択画面を開始する
    public void StartSelection(GameObject mainObject, GameObject subtarget, GameObject targetObject)
    {
        // target 選択処理を準備
        // 選択ボタンに、targetObject や activetarget を使った処理を割り当て
        freezeGame();
        SetupSelectionButtons(mainObject, subtarget, targetObject);
    }

    // 選択ボタンを設定する
    private void SetupSelectionButtons(
        GameObject mainObject,
        GameObject subObject,
        GameObject targetObject
    )
    {
        
        // ボタン1に activetarget を選択する処理を割り当てる
        Button activeObjectButton = selectionPanel
            .transform.Find("activeObjectButton")
            .GetComponent<Button>();
        activeObjectButton.onClick.AddListener(
            () => gameObject.GetComponent<EquipManager>().EquipItemtoMain(targetObject)
        );
        activeObjectButton.onClick.AddListener(() => continueGame());
        Selecttarget(mainEquipImage, mainObject);

        // ボタン2に subtarget を選択する処理を割り当てる
        Button subtargetButton = selectionPanel
            .transform.Find("subetargetButton")
            .GetComponent<Button>();
        subtargetButton.onClick.AddListener(
            () => gameObject.GetComponent<EquipManager>().EquipItemtoMain(targetObject)
        );
        subtargetButton.onClick.AddListener(() => continueGame());
        Selecttarget(subEquipImage, subObject);
        // ボタン2に subtarget を選択する処理を割り当てる
        Button changetargetButton = selectionPanel
            .transform.Find("changetargetButton")
            .GetComponent<Button>();
        subtargetButton.onClick.AddListener(() => continueGame());
        Selecttarget(targetEquipImage, targetObject);
    }

    public void freezeGame()
    {
        Time.timeScale = 0f; // ゲームの時間を停止
        selectionPanel.SetActive(true); // 選択 UI を表示
    }

    public void continueGame()
    {
        // ゲームの時間を再開
        Time.timeScale = 1f;

        // 選択 UI を非表示にする
        selectionPanel.SetActive(false);
    }

    // target を選択した際の処理
    private void Selecttarget(GameObject selectedtarget, GameObject ImageObj)
    {
        // 選択した target を反映（必要な処理を行う）
        Debug.Log("Selected target: " + selectedtarget.name);

        // 選択したターゲットのスプライトを変更する
        SpriteRenderer spriteRenderer = selectedtarget.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            ImageObj.GetComponent<Image>().sprite = spriteRenderer.sprite;
        }
    }
}
