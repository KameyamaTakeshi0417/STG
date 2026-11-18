using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Data;

public class treasureManager_Alpha : MonoBehaviour
{
    public static treasureManager_Alpha Instance { get; private set; }

    private Queue<OrbData_Alpha> orbQueue = new Queue<OrbData_Alpha>();

    [SerializeField]
    private List<int> stackDisplayList = new List<int>();
    public Stack<int> treasure = new Stack<int>();

    [Header("Stage 1 First Half Auto-Equip")]
    [Tooltip("ステージ1前半で最初に取得する3つのオーブを自動で開封・装備（消費扱い）するか")]
    public bool enableStage1FirstHalfAutoEquip = true;
    [Tooltip("自動装備する対象の武器グループ（0=枠0-2, 1=枠3-5, 2=枠6-8）。デフォルトは枠3-5")]
    public int autoEquipTargetGroup = 1;

    // 1個目:弾頭(Bullet/枠5) → 2個目:薬莢(Casing/枠4) → 3個目:雷管(Primer/枠3)
    private const int AutoEquipMaxOrbCount = 3;
    private int autoEquipCursor = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ステージ1前半の自動装備進行度をリセットします（リスタート時など）
    /// </summary>
    public void ResetAutoEquipProgress()
    {
        autoEquipCursor = 0;
    }

    public void PushOrb(OrbData_Alpha orb)
    {
        if (orb == null) return;

        // ステージ1前半の雑魚・報酬ゲージ由来オーブ（最初の3個）は自動開封＆自動装備（消費扱い）
        if (ShouldAutoEquipStage1FirstHalf(orb) && TryAutoEquipFromOrb(orb))
        {
            return;
        }

        EnqueueOrbInternal(orb, showTutorial: true);
    }

    private void EnqueueOrbInternal(OrbData_Alpha orb, bool showTutorial)
    {
        orbQueue.Enqueue(orb);
        
        // 旧スタック表示との同期
        treasure.Push(orb.orbRarity);
        SyncToDisplay();

        Debug.Log($"[TreasureManager] Queued Orb. Total in queue: {orbQueue.Count}");

        // 初めて通常オーブ（報酬フェーズ開封用）を拾った時のチュートリアル表示
        if (showTutorial && Alpha.UI.TutorialManager_Alpha.Instance != null)
        {
            Alpha.UI.TutorialManager_Alpha.Instance.ShowTutorial("Tutorial_Orb", true, 3f);
        }
    }

    private bool ShouldAutoEquipStage1FirstHalf(OrbData_Alpha orb)
    {
        if (!enableStage1FirstHalfAutoEquip) return false;
        if (orb.source != OrbSource_Alpha.Mob) return false;
        if (autoEquipCursor >= AutoEquipMaxOrbCount) return false;

        var stageMgr = Alpha.Flow.StageManager_Alpha.Instance;
        if (stageMgr == null) return false;
        if (stageMgr.currentStageIndex != 0) return false;

        var state = stageMgr.currentState;
        bool isFirstHalfPhase =
            state == Alpha.Flow.StageState_Alpha.FirstHalf ||
            state == Alpha.Flow.StageState_Alpha.MidBossWait ||
            state == Alpha.Flow.StageState_Alpha.MidBossFight;

        return isFirstHalfPhase;
    }

    private bool TryAutoEquipFromOrb(OrbData_Alpha orb)
    {
        var inv = InventoryManager_Alpha.Instance;
        var gen = Alpha.Flow.WeaponGenerator_Alpha.Instance;
        if (inv == null || gen == null)
        {
            Debug.LogWarning("[TreasureManager] InventoryManager or WeaponGenerator is missing. Skipping auto-equip.");
            return false;
        }

        int targetGroup = Mathf.Clamp(autoEquipTargetGroup, 0, 2);
        int targetRarity = Mathf.Clamp(orb.orbRarity, 1, 4);

        // 1個目:弾頭(Bullet -> +2), 2個目:薬莢(Casing -> +1), 3個目:雷管(Primer -> +0)
        // InventoryManager のスロット構造: x=0:Primer(雷管), x=1:Casing(薬莢), x=2:Bullet(弾頭)
        WeaponPartType_Alpha partType;
        int slotOffset;
        switch (autoEquipCursor)
        {
            case 0:
                partType = WeaponPartType_Alpha.Bullet;
                slotOffset = 2;
                break;
            case 1:
                partType = WeaponPartType_Alpha.Casing;
                slotOffset = 1;
                break;
            default:
                partType = WeaponPartType_Alpha.Primer;
                slotOffset = 0;
                break;
        }

        int slotIndex = targetGroup * 3 + slotOffset;

        WeaponSeriesData_Alpha series = gen.GetRandomSeriesFromPool(targetRarity);
        if (series == null)
        {
            Debug.LogWarning($"[TreasureManager] No series found in drop pool for rarity {targetRarity}.");
            return false;
        }

        WeaponPartInstance_Alpha partInst = gen.CreatePartInstanceWithType(series, partType, targetRarity);
        if (partInst == null) return false;

        InventoryManager_Alpha.EquipInstance newEquip = new InventoryManager_Alpha.EquipInstance
        {
            series = partInst.series,
            partType = partInst.partType,
            rarity = partInst.quality,
            currentEffects = partInst.currentEffects,
            setBonusEffect = partInst.setBonusEffect,
            defId = partInst.series.seriesName
        };

        inv.SetByIndex(slotIndex, newEquip);
        ApplyAutoEquipHPGaugePlusIfNeeded(newEquip);

        bool isFirstOrb = (autoEquipCursor == 0);
        autoEquipCursor++;

        Debug.Log($"<color=#33ffbb>[TreasureManager]</color> Stage 1 FirstHalf Auto-Equipped ({autoEquipCursor}/{AutoEquipMaxOrbCount}, Orb consumed): {partType}({series.seriesName} R{targetRarity}) -> Slot[{slotIndex}]");

        // オーブ初取得時にパーツ・自動装備チュートリアルを表示
        if (isFirstOrb && Alpha.UI.TutorialManager_Alpha.Instance != null)
        {
            Alpha.UI.TutorialManager_Alpha.Instance.ShowTutorial("Tutorial_Orb_AutoEquip", true, 5.5f);
        }

        return true;
    }

    private void ApplyAutoEquipHPGaugePlusIfNeeded(InventoryManager_Alpha.EquipInstance item)
    {
        if (item.series == null || playerStatusManager_Alpha.Instance == null) return;

        float healPercent = 0f;
        System.Action<WeaponEffectSO_Alpha> checkHeal = (effectSO) =>
        {
            if (effectSO != null && effectSO.effectType == WeaponEffectType_Alpha.HPGaugePlus)
            {
                float percent = effectSO.GetValue(item.rarity);
                if (percent > healPercent) healPercent = percent;
            }
        };

        if (item.series.passiveEffects != null)
        {
            foreach (var e in item.series.passiveEffects) checkHeal(e.effect);
        }
        if (item.currentEffects != null)
        {
            foreach (var e in item.currentEffects) checkHeal(e);
        }

        if (healPercent > 0f)
        {
            float healAmount = playerStatusManager_Alpha.Instance.HP * (healPercent / 100f);
            playerStatusManager_Alpha.Instance.Heal(healAmount);
        }
    }

    /// <summary>
    /// 現在溜まっているオーブのキューを取得してクリアする
    /// </summary>
    public Queue<OrbData_Alpha> FlushOrbQueue()
    {
        // 現在のキューをコピーして返し、元のキューはクリアする
        Queue<OrbData_Alpha> currentOrbs = new Queue<OrbData_Alpha>(orbQueue);
        orbQueue.Clear();
        treasure.Clear();
        SyncToDisplay();
        return currentOrbs;
    }

    /// <summary>
    /// 現在のオーブキューの内容を複製して取得する（セーブ用：キューを破壊しない）
    /// </summary>
    public List<OrbData_Alpha> GetCurrentOrbs()
    {
        return new List<OrbData_Alpha>(orbQueue);
    }

    /// <summary>
    /// セーブデータ等からオーブのリストを一括復元する
    /// </summary>
    public void RestoreOrbs(IEnumerable<OrbData_Alpha> orbs)
    {
        orbQueue.Clear();
        treasure.Clear();
        if (orbs != null)
        {
            foreach (var orb in orbs)
            {
                if (orb != null) EnqueueOrbInternal(orb, showTutorial: false);
            }
        }
        SyncToDisplay();
    }

    // 既存の OrbControll_Alpha などからのアクセス用
    public void GetTreasure(int rarelity) {
        // 新しい仕様に合わせてPush
        PushOrb(new OrbData_Alpha(rarelity, OrbSource_Alpha.Mob));
    }

    public void PopItem()
    {
        if (treasure.Count > 0)
        {
            treasure.Pop();
            SyncToDisplay();
        }
    }

    private void SyncToDisplay()
    {
        stackDisplayList.Clear();
        foreach (var item in treasure)
        {
            stackDisplayList.Add(item);
        }
    }
}
