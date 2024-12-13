using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicList : MonoBehaviour
{
    public bool[] Relics;
    int relicCount = 50; // 適切な数に設定

    // Start is called before the first frame update
    void Start()
    {
        Relics = new bool[relicCount];
        LoadRelics(); // 遺物の状態をロード
    }

    // 遺物の状態を保存
    public void SaveRelics()
    {
        for (int i = 0; i < Relics.Length; i++)
        {
            PlayerPrefs.SetInt("Relic" + i, Relics[i] ? 1 : 0);
        }
        PlayerPrefs.Save();
        Debug.Log("Relics saved");
    }

    // 遺物の状態をロード
    public void LoadRelics()
    {
        for (int i = 0; i < Relics.Length; i++)
        {
            Relics[i] = PlayerPrefs.GetInt("Relic" + i, 0) == 1;
        }
        //  Debug.Log("Relics loaded");
    }

    // Update is called once per frame
    void Update() { }
}
