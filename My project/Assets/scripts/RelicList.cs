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
    }

    // 遺物の状態をロード
    public void LoadRelics()
    {
        for (int i = 0; i < Relics.Length; i++)
        {
            Relics[i] = PlayerPrefs.GetInt("Relic" + i, 0) == 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // テスト用：スペースキーを押すと保存、Lキーを押すとロード
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveRelics();
            Debug.Log("Relics saved");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadRelics();
            Debug.Log("Relics loaded");
        }
    }
}