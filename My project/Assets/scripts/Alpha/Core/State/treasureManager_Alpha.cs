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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PushOrb(OrbData_Alpha orb)
    {
        if (orb == null) return;
        orbQueue.Enqueue(orb);
        
        // 旧スタック表示との同期
        treasure.Push(orb.orbRarity);
        SyncToDisplay();

        Debug.Log($"[TreasureManager] Queued Orb. Total in queue: {orbQueue.Count}");
    }

    public void OpenAllOrbs()
    {
        if (orbQueue.Count == 0)
        {
            Debug.Log("[TreasureManager] No orbs to open.");
            return;
        }

        Debug.Log($"[TreasureManager] Opening {orbQueue.Count} orbs...");
        
        if (Alpha.UI.OrbSelectionUI_Alpha.Instance != null)
        {
            Alpha.UI.OrbSelectionUI_Alpha.Instance.StartOpeningOrbs(orbQueue);
        }
        else
        {
            Debug.LogWarning("[TreasureManager] OrbSelectionUI_Alpha instance not found. Just dequeueing.");
            while(orbQueue.Count > 0)
            {
                var orb = orbQueue.Dequeue();
                Debug.Log($"[TreasureManager] Opened Orb -> Rarity: {orb.orbRarity}, Source: {orb.source}");
            }
        }
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
