using System.Collections.Generic;
using UnityEngine;
using System;

namespace Alpha.Core
{
    public enum ActionType_Alpha
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Shoot,
        Special,
        SlowFocus,
        Dash,
        WaveSkip,
        Pause,
        Submit
    }

    public class InputManager_Alpha : MonoBehaviour
    {
        public static InputManager_Alpha Instance { get; private set; }

        // 繝・ヵ繧ｩ繝ｫ繝医・繧ｭ繝ｼ繧｢繧ｵ繧､繝ｳ
        private Dictionary<ActionType_Alpha, KeyCode> defaultKeys = new Dictionary<ActionType_Alpha, KeyCode>
        {
            { ActionType_Alpha.MoveUp, KeyCode.W },
            { ActionType_Alpha.MoveDown, KeyCode.S },
            { ActionType_Alpha.MoveLeft, KeyCode.A },
            { ActionType_Alpha.MoveRight, KeyCode.D },
            { ActionType_Alpha.Shoot, KeyCode.Mouse0 },
            { ActionType_Alpha.Special, KeyCode.Mouse1 },
            { ActionType_Alpha.SlowFocus, KeyCode.LeftShift },
            { ActionType_Alpha.Dash, KeyCode.Space },
            { ActionType_Alpha.WaveSkip, KeyCode.Space },
            { ActionType_Alpha.Pause, KeyCode.Escape },
            { ActionType_Alpha.Submit, KeyCode.Return }
        };

        private Dictionary<ActionType_Alpha, KeyCode> currentKeys = new Dictionary<ActionType_Alpha, KeyCode>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKeys();
        }

        public void LoadKeys()
        {
            foreach (ActionType_Alpha action in Enum.GetValues(typeof(ActionType_Alpha)))
            {
                string keyName = PlayerPrefs.GetString("Key_" + action.ToString(), "");
                if (!string.IsNullOrEmpty(keyName) && Enum.TryParse(keyName, out KeyCode parsedKey))
                {
                    currentKeys[action] = parsedKey;
                }
                else
                {
                    currentKeys[action] = defaultKeys[action];
                }
            }
        }

        public void SaveKeys()
        {
            foreach (var kvp in currentKeys)
            {
                PlayerPrefs.SetString("Key_" + kvp.Key.ToString(), kvp.Value.ToString());
            }
            PlayerPrefs.Save();
        }

        public KeyCode GetKeyForAction(ActionType_Alpha action)
        {
            if (currentKeys.TryGetValue(action, out KeyCode code))
                return code;
            return KeyCode.None;
        }

        public void SetKeyForAction(ActionType_Alpha action, KeyCode key)
        {
            currentKeys[action] = key;
        }

        public bool IsKeyUsed(KeyCode key, out ActionType_Alpha usedAction)
        {
            foreach (var kvp in currentKeys)
            {
                if (kvp.Value == key)
                {
                    usedAction = kvp.Key;
                    return true;
                }
            }
            usedAction = ActionType_Alpha.MoveUp;
            return false;
        }

        // --- 螳滄圀縺ｮ蜈･蜉帛叙蠕励Γ繧ｽ繝・ラ ---

        public bool GetAction(ActionType_Alpha action)
        {
            KeyCode code = GetKeyForAction(action);
            return Input.GetKey(code);
        }

        public bool GetActionDown(ActionType_Alpha action)
        {
            KeyCode code = GetKeyForAction(action);
            return Input.GetKeyDown(code);
        }

        public bool GetActionUp(ActionType_Alpha action)
        {
            KeyCode code = GetKeyForAction(action);
            return Input.GetKeyUp(code);
        }

        // Horizontal, Vertical 縺ｮ謫ｬ莨ｼ Axis (-1.0f ・・1.0f) 繧定ｿ斐☆
        public float GetAxisRaw(string axisName)
        {
            float val = 0f;
            if (axisName == "Horizontal")
            {
                if (GetAction(ActionType_Alpha.MoveLeft)) val -= 1f;
                if (GetAction(ActionType_Alpha.MoveRight)) val += 1f;
            }
            else if (axisName == "Vertical")
            {
                if (GetAction(ActionType_Alpha.MoveDown)) val -= 1f;
                if (GetAction(ActionType_Alpha.MoveUp)) val += 1f;
            }
            return val;
        }

        // 繧ｹ繝繝ｼ繧ｸ繝ｳ繧ｰ縺ｪ縺励〒濶ｯ縺代ｌ縺ｰGetAxisRaw縺ｨ蜷後§縺ｧ霑斐☆
        public float GetAxis(string axisName)
        {
            return GetAxisRaw(axisName);
        }
    }
}
