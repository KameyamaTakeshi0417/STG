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

        // 初めてオーブを拾った時のチュートリアル表示
        if (Alpha.UI.TutorialManager_Alpha.Instance != null)
        {
            Alpha.UI.TutorialManager_Alpha.Instance.ShowTutorial("Tutorial_Orb", true, 3f);
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
                PushOrb(orb);
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
