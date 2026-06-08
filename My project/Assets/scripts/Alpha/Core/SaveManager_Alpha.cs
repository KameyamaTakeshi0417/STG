using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Alpha.Core
{
    [System.Serializable]
    public class SaveData_Alpha
    {
        public bool hasSaveData = false;
        
        // Stage Progress
        public int currentStageIndex = 0;
        public int currentStateValue = 0; // StageState_Alpha enum casted to int
        
        // Player Status
        public float currentHP = 100f;
        public int currentExp = 0;

        // Player Inventory & Equipments (saved by names or IDs)
        // 今回はとりあえず空でもOKですが、将来的にアイテムのIDリストを持たせます
        public string activeBulletName = "";
        public string activeCaseName = "";
        public string activePrimerName = "";
        public List<string> inventoryItemNames = new List<string>();
    }

    public class SaveManager_Alpha : MonoBehaviour
    {
        public static SaveManager_Alpha Instance { get; private set; }

        public SaveData_Alpha currentSaveData = new SaveData_Alpha();
        private string saveFilePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            saveFilePath = Path.Combine(Application.persistentDataPath, "savedata_alpha.json");
            LoadGame();
        }

        public void SaveGame()
        {
            currentSaveData.hasSaveData = true;

            // ---- ここで現在のゲーム状態を SaveData_Alpha に収集する ----
            if (Alpha.Flow.StageManager_Alpha.Instance != null)
            {
                currentSaveData.currentStageIndex = Alpha.Flow.StageManager_Alpha.Instance.currentStageIndex;
                currentSaveData.currentStateValue = (int)Alpha.Flow.StageManager_Alpha.Instance.currentState;
            }

            if (playerStatusManager_Alpha.Instance != null)
            {
                currentSaveData.currentHP = playerStatusManager_Alpha.Instance.currentHP;
                currentSaveData.currentExp = playerStatusManager_Alpha.Instance.currentExp;
            }

            // ---- 保存処理 ----
            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("[SaveManager] Game Saved: " + saveFilePath);
        }

        public void LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<SaveData_Alpha>(json);
                Debug.Log("[SaveManager] Game Loaded");
            }
            else
            {
                currentSaveData = new SaveData_Alpha();
                Debug.Log("[SaveManager] No Save Data Found");
            }
        }

        public void ClearSaveData()
        {
            currentSaveData = new SaveData_Alpha();
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
            Debug.Log("[SaveManager] Save Data Cleared");
        }

        public bool HasSaveData()
        {
            return currentSaveData != null && currentSaveData.hasSaveData;
        }
    }
}
