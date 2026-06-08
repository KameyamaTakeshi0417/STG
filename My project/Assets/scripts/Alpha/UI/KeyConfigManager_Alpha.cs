using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Alpha.Core;
using TMPro;

namespace Alpha.UI
{
    public class KeyConfigManager_Alpha : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject keyConfigPanel;
        public GameObject waitInputPopup;
        public GameObject swapWarningPopup;
        public GameObject unassignedWarningPopup;

        [Header("Wait Input UI")]
        public TextMeshProUGUI waitInputTitleText; // e.g. "Shoot キーを入力してください"

        [Header("Swap Warning UI")]
        public TextMeshProUGUI swapWarningText; // e.g. "SpaceキーをBombからShootに変更しました"
        public Button swapWarningOkButton;

        [Header("Unassigned Warning UI")]
        public TextMeshProUGUI unassignedWarningText;
        public Button unassignedCompleteButton;
        public Button unassignedBackButton;

        [Header("Buttons")]
        public Button backButton;

        // 内部ステート
        private ActionType_Alpha actionWaitingForInput;
        private bool isWaitingForInput = false;

        private void Start()
        {
            if (keyConfigPanel != null) keyConfigPanel.SetActive(false);
            if (waitInputPopup != null) waitInputPopup.SetActive(false);
            if (swapWarningPopup != null) swapWarningPopup.SetActive(false);
            if (unassignedWarningPopup != null) unassignedWarningPopup.SetActive(false);

            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            if (swapWarningOkButton != null) swapWarningOkButton.onClick.AddListener(() => ClosePopup(swapWarningPopup));
            
            if (unassignedCompleteButton != null) unassignedCompleteButton.onClick.AddListener(ConfirmExit);
            if (unassignedBackButton != null) unassignedBackButton.onClick.AddListener(() => ClosePopup(unassignedWarningPopup));
        }

        public void OpenKeyConfig()
        {
            if (keyConfigPanel != null) keyConfigPanel.SetActive(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            // TODO: UIのボタンテキストなどを現在の InputManager_Alpha のアサインで更新する。
            // インスペクタで各Actionに対応するボタンを配置し、それを更新する想定。
            // 今回はロジックのみ構築。
        }

        public void StartWaitingForInput(ActionType_Alpha action)
        {
            actionWaitingForInput = action;
            isWaitingForInput = true;
            
            if (waitInputPopup != null)
            {
                waitInputPopup.SetActive(true);
                if (waitInputTitleText != null)
                    waitInputTitleText.text = $"{action} の新しいキーを入力してください";
            }
        }

        private void Update()
        {
            if (isWaitingForInput)
            {
                if (Input.anyKeyDown)
                {
                    // マウスやキーボードの入力を判定
                    foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(keyCode))
                        {
                            // ESCキーはキャンセル扱い
                            if (keyCode == KeyCode.Escape)
                            {
                                CancelWaitInput();
                                return;
                            }

                            AssignKey(keyCode);
                            return;
                        }
                    }
                }
            }
        }

        private void CancelWaitInput()
        {
            isWaitingForInput = false;
            if (waitInputPopup != null) waitInputPopup.SetActive(false);
        }

        private void AssignKey(KeyCode newKey)
        {
            isWaitingForInput = false;
            if (waitInputPopup != null) waitInputPopup.SetActive(false);

            if (InputManager_Alpha.Instance == null) return;

            // 重複チェック
            if (InputManager_Alpha.Instance.IsKeyUsed(newKey, out ActionType_Alpha oldAction))
            {
                if (oldAction != actionWaitingForInput)
                {
                    // Swap (古いアクションを未割当=Noneにするか、スワップするか。今回は未割当にする)
                    InputManager_Alpha.Instance.SetKeyForAction(oldAction, KeyCode.None);
                    InputManager_Alpha.Instance.SetKeyForAction(actionWaitingForInput, newKey);

                    ShowSwapWarning(newKey, oldAction, actionWaitingForInput);
                }
            }
            else
            {
                InputManager_Alpha.Instance.SetKeyForAction(actionWaitingForInput, newKey);
            }

            InputManager_Alpha.Instance.SaveKeys();
            RefreshUI();
        }

        private void ShowSwapWarning(KeyCode key, ActionType_Alpha oldAction, ActionType_Alpha newAction)
        {
            if (swapWarningPopup != null)
            {
                swapWarningPopup.SetActive(true);
                if (swapWarningText != null)
                {
                    swapWarningText.text = $"{key}キーを {oldAction} から {newAction} に割り当て変更しました。";
                }
            }
        }

        private void OnBackClicked()
        {
            if (InputManager_Alpha.Instance == null)
            {
                ConfirmExit();
                return;
            }

            // 未割り当てチェック
            List<ActionType_Alpha> unassigned = new List<ActionType_Alpha>();
            foreach (ActionType_Alpha action in Enum.GetValues(typeof(ActionType_Alpha)))
            {
                if (InputManager_Alpha.Instance.GetKeyForAction(action) == KeyCode.None)
                {
                    unassigned.Add(action);
                }
            }

            if (unassigned.Count > 0)
            {
                if (unassignedWarningPopup != null)
                {
                    unassignedWarningPopup.SetActive(true);
                    if (unassignedWarningText != null)
                    {
                        unassignedWarningText.text = "未割当のアクションがあります！\n" + string.Join(", ", unassigned) + "\n設定を完了してよろしいですか？";
                    }
                }
                else
                {
                    ConfirmExit();
                }
            }
            else
            {
                ConfirmExit();
            }
        }

        private void ConfirmExit()
        {
            if (unassignedWarningPopup != null) unassignedWarningPopup.SetActive(false);
            if (keyConfigPanel != null) keyConfigPanel.SetActive(false);
        }

        private void ClosePopup(GameObject popup)
        {
            if (popup != null) popup.SetActive(false);
        }
    }
}
