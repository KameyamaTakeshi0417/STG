using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Data;
using Alpha.Flow;

namespace Alpha.Core
{
    [Serializable]
    public class SavedPlayerStatus
    {
        public float currentHP = 100f;
        public float maxHP = 100f;
        public int currentExp = 0;
        public int currentPetals = 0;
        public int executionerTier = 0;
    }

    [Serializable]
    public class SavedEquipSlot
    {
        public int slotIndex;
        public string seriesName;
        public string defId;
        public int partType;
        public int rarity;
        public int originalRarity;
        public List<string> effectNames = new List<string>();
        public string setBonusEffectName = "";
    }

    [Serializable]
    public class SavedOrbData
    {
        public int orbRarity;
        public int source; // OrbSource_Alpha casted to int
        public string bossId = "";

        public SavedOrbData() { }
        public SavedOrbData(OrbData_Alpha orb)
        {
            if (orb != null)
            {
                orbRarity = orb.orbRarity;
                source = (int)orb.source;
                bossId = orb.bossId ?? "";
            }
        }

        public OrbData_Alpha ToOrbData()
        {
            return new OrbData_Alpha(orbRarity, (OrbSource_Alpha)source, bossId);
        }
    }

    [Serializable]
    public class SaveData_Alpha
    {
        public bool hasSaveData = false;
        public string saveTimestamp = "";

        // Stage Progress
        public int currentStageIndex = 0;
        public bool isHalfwayCheckpoint = false; // true: 前半終了後（中ボス後）, false: 後半終了後（大ボス後）
        public int savedStageState = 0;

        // Player Status
        public SavedPlayerStatus playerStatus = new SavedPlayerStatus();

        // Player Inventory & Equipments
        public List<SavedEquipSlot> inventorySlots = new List<SavedEquipSlot>();

        // Pending Orbs (未獲得報酬・オーブ)
        public List<SavedOrbData> pendingOrbs = new List<SavedOrbData>();
    }

    public class SaveManager_Alpha : MonoBehaviour
    {
        public static SaveManager_Alpha Instance { get; private set; }

        public SaveData_Alpha currentSaveData = new SaveData_Alpha();
        private string saveFilePath;

        // オートセーブ通知用のイベント
        public static event Action<string> OnAutoSaveNotification;

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

        /// <summary>
        /// ステージ前半終了時（中ボス撃破・オーブ回収直後、報酬獲得前）のオートセーブ
        /// </summary>
        public void AutoSaveAtMidBossClear()
        {
            CaptureCurrentGameState(isHalfway: true);
            SaveToFile();
            NotifyAutoSave("オートセーブ完了（ステージ前半終了）");
        }

        /// <summary>
        /// ステージ後半終了時（ボス撃破・自動売却直後、報酬獲得前）のオートセーブ
        /// </summary>
        public void AutoSaveAtBossClear()
        {
            CaptureCurrentGameState(isHalfway: false);
            SaveToFile();
            NotifyAutoSave("オートセーブ完了（ステージ後半終了）");
        }

        /// <summary>
        /// 現在のゲーム内状態（ステータス、装備、オーブキュー）を SaveData_Alpha に収集する
        /// </summary>
        public void CaptureCurrentGameState(bool isHalfway)
        {
            currentSaveData.hasSaveData = true;
            currentSaveData.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentSaveData.isHalfwayCheckpoint = isHalfway;

            // 1. ステージ進行状態
            if (StageManager_Alpha.Instance != null)
            {
                currentSaveData.currentStageIndex = StageManager_Alpha.Instance.currentStageIndex;
                currentSaveData.savedStageState = (int)StageManager_Alpha.Instance.currentState;
            }

            // 2. プレイヤーステータス
            if (playerStatusManager_Alpha.Instance != null)
            {
                currentSaveData.playerStatus.currentHP = playerStatusManager_Alpha.Instance.currentHP;
                currentSaveData.playerStatus.maxHP = playerStatusManager_Alpha.Instance.HP;
                currentSaveData.playerStatus.currentExp = playerStatusManager_Alpha.Instance.currentExp;
                currentSaveData.playerStatus.currentPetals = playerStatusManager_Alpha.Instance.currentPetals;
                currentSaveData.playerStatus.executionerTier = playerStatusManager_Alpha.Instance.executionerTier;
            }

            // 3. インベントリ & 装備
            currentSaveData.inventorySlots.Clear();
            if (InventoryManager_Alpha.Instance != null)
            {
                var equips = InventoryManager_Alpha.Instance.equipInstance;
                for (int i = 0; i < equips.Count; i++)
                {
                    var eq = equips[i];
                    if (eq.series == null && string.IsNullOrEmpty(eq.defId)) continue;

                    SavedEquipSlot slot = new SavedEquipSlot();
                    slot.slotIndex = i;
                    slot.seriesName = eq.series != null ? eq.series.seriesName : (eq.defId ?? "");
                    slot.defId = eq.defId ?? "";
                    slot.partType = (int)eq.partType;
                    slot.rarity = eq.rarity;
                    slot.originalRarity = eq.originalRarity;

                    if (eq.currentEffects != null)
                    {
                        foreach (var eff in eq.currentEffects)
                        {
                            if (eff != null) slot.effectNames.Add(eff.name);
                        }
                    }
                    if (eq.setBonusEffect != null)
                    {
                        slot.setBonusEffectName = eq.setBonusEffect.name;
                    }

                    currentSaveData.inventorySlots.Add(slot);
                }
            }

            // 4. 未獲得オーブキュー (treasureManager_Alpha)
            currentSaveData.pendingOrbs.Clear();
            if (treasureManager_Alpha.Instance != null)
            {
                List<OrbData_Alpha> currentOrbs = treasureManager_Alpha.Instance.GetCurrentOrbs();
                foreach (var orb in currentOrbs)
                {
                    currentSaveData.pendingOrbs.Add(new SavedOrbData(orb));
                }
            }

            Debug.Log($"[SaveManager] Captured game state. Stage: {currentSaveData.currentStageIndex}, Halfway: {isHalfway}, Orbs: {currentSaveData.pendingOrbs.Count}, Equips: {currentSaveData.inventorySlots.Count}");
        }

        public void SaveToFile()
        {
            try
            {
                string json = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log("[SaveManager] Save data written to: " + saveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[SaveManager] Failed to write save file: " + ex.Message);
            }
        }

        public void LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    currentSaveData = JsonUtility.FromJson<SaveData_Alpha>(json);
                    Debug.Log("[SaveManager] Save data loaded from file. Timestamp: " + currentSaveData.saveTimestamp);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[SaveManager] Failed to parse save file: " + ex.Message);
                    currentSaveData = new SaveData_Alpha();
                }
            }
            else
            {
                currentSaveData = new SaveData_Alpha();
                Debug.Log("[SaveManager] No existing save data found.");
            }
        }

        /// <summary>
        /// セーブデータの内容をゲーム内へ復元する
        /// </summary>
        public void RestoreGameState(Action onComplete = null)
        {
            if (!HasSaveData())
            {
                Debug.LogWarning("[SaveManager] Cannot restore: No valid save data.");
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[SaveManager] Restoring game state. Stage: {currentSaveData.currentStageIndex}, Halfway: {currentSaveData.isHalfwayCheckpoint}");

            // 1. プレイヤーステータス復元
            if (playerStatusManager_Alpha.Instance != null)
            {
                playerStatusManager_Alpha.Instance.HP = currentSaveData.playerStatus.maxHP;
                playerStatusManager_Alpha.Instance.currentHP = currentSaveData.playerStatus.currentHP;
                playerStatusManager_Alpha.Instance.currentExp = currentSaveData.playerStatus.currentExp;
                playerStatusManager_Alpha.Instance.currentPetals = currentSaveData.playerStatus.currentPetals;
                playerStatusManager_Alpha.Instance.executionerTier = currentSaveData.playerStatus.executionerTier;
            }

            // 2. インベントリ & 装備復元
            if (InventoryManager_Alpha.Instance != null)
            {
                // 全スロットを一旦クリア
                for (int i = 0; i < InventoryManager_Alpha.Instance.equipInstance.Count; i++)
                {
                    InventoryManager_Alpha.Instance.equipInstance[i] = new InventoryManager_Alpha.EquipInstance();
                }

                // シリーズ定義およびエフェクト定義のキャッシュ辞書を作成
                Dictionary<string, WeaponSeriesData_Alpha> seriesDict = new Dictionary<string, WeaponSeriesData_Alpha>();
                if (WeaponGenerator_Alpha.Instance != null && WeaponGenerator_Alpha.Instance.allSeriesPool != null)
                {
                    foreach (var s in WeaponGenerator_Alpha.Instance.allSeriesPool)
                    {
                        if (s != null && !seriesDict.ContainsKey(s.seriesName))
                        {
                            seriesDict.Add(s.seriesName, s);
                        }
                    }
                }

                Dictionary<string, WeaponEffectSO_Alpha> effectDict = new Dictionary<string, WeaponEffectSO_Alpha>();
                if (WeaponGenerator_Alpha.Instance != null)
                {
                    if (WeaponGenerator_Alpha.Instance.globalBuffEffects != null)
                    {
                        foreach (var eff in WeaponGenerator_Alpha.Instance.globalBuffEffects)
                        {
                            if (eff != null && !effectDict.ContainsKey(eff.name)) effectDict.Add(eff.name, eff);
                        }
                    }
                    if (WeaponGenerator_Alpha.Instance.globalDebuffEffects != null)
                    {
                        foreach (var eff in WeaponGenerator_Alpha.Instance.globalDebuffEffects)
                        {
                            if (eff != null && !effectDict.ContainsKey(eff.name)) effectDict.Add(eff.name, eff);
                        }
                    }
                }

                foreach (var slot in currentSaveData.inventorySlots)
                {
                    InventoryManager_Alpha.EquipInstance eq = new InventoryManager_Alpha.EquipInstance();
                    eq.defId = slot.defId;
                    eq.rarity = slot.rarity;
                    eq.originalRarity = slot.originalRarity;
                    eq.partType = (WeaponPartType_Alpha)slot.partType;

                    // シリーズの解決
                    if (!string.IsNullOrEmpty(slot.seriesName) && seriesDict.TryGetValue(slot.seriesName, out var seriesSO))
                    {
                        eq.series = seriesSO;
                    }

                    // エフェクトの解決
                    eq.currentEffects = new List<WeaponEffectSO_Alpha>();
                    if (slot.effectNames != null)
                    {
                        foreach (var effName in slot.effectNames)
                        {
                            if (effectDict.TryGetValue(effName, out var foundEff))
                            {
                                eq.currentEffects.Add(foundEff);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(slot.setBonusEffectName) && effectDict.TryGetValue(slot.setBonusEffectName, out var bonusEff))
                    {
                        eq.setBonusEffect = bonusEff;
                    }

                    InventoryManager_Alpha.Instance.SetByIndex(slot.slotIndex, eq);
                }
            }

            // 3. オーブキューの復元
            if (treasureManager_Alpha.Instance != null)
            {
                List<OrbData_Alpha> orbsToRestore = new List<OrbData_Alpha>();
                foreach (var savedOrb in currentSaveData.pendingOrbs)
                {
                    orbsToRestore.Add(savedOrb.ToOrbData());
                }
                treasureManager_Alpha.Instance.RestoreOrbs(orbsToRestore);
            }

            Debug.Log("[SaveManager] Game state successfully restored!");
            onComplete?.Invoke();
        }

        public void ClearSaveData()
        {
            currentSaveData = new SaveData_Alpha();
            if (File.Exists(saveFilePath))
            {
                try
                {
                    File.Delete(saveFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[SaveManager] Failed to delete save file: " + ex.Message);
                }
            }
            Debug.Log("[SaveManager] Save data cleared.");
        }

        public bool HasSaveData()
        {
            return currentSaveData != null && currentSaveData.hasSaveData;
        }

        private void NotifyAutoSave(string message)
        {
            Debug.Log($"[SaveManager] >>> {message} <<<");
            OnAutoSaveNotification?.Invoke(message);
        }
    }
}
