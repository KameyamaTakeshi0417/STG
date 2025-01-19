using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public List<StoryData> storyDataList = new List<StoryData>();
    public GameObject fadeBoard;

    void Start()
    {
        LoadStoryData();
    }

    // ストーリーデータをロードするメソッド
    private void LoadStoryData()
    {
        // ここでストーリーデータを読み込みます（例: JSONやScriptableObjectなど）
        // とりあえずテスト用にハードコードで追加
        storyDataList.Add(
            new StoryData()
            {
                Name = "キャラA",
                Talk = "今日はいい天気ですね。",
                Left = "Sprites/CharA",
            }
        );
        storyDataList.Add(
            new StoryData()
            {
                Name = "キャラB",
                Talk = "本当にそうですね。",
                Right = "Sprites/CharB",
            }
        );
    }

    // ストーリーデータを返すメソッド
    public StoryData GetStoryData(int index)
    {
        if (index >= 0 && index < storyDataList.Count)
        {
            return storyDataList[index];
        }
        return null;
    }
}
