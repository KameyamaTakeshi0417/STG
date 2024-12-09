using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalkWindow : MonoBehaviour
{
    // 名前のテキスト.
    [SerializeField]
    TextMeshProUGUI nameText = null;

    // 会話内容テキスト.
    [SerializeField]
    TextMeshProUGUI talkText = null;

    // 次に進むボタン
    [SerializeField]
    Button nextButton = null;

    // キャラクター立ち絵
    [SerializeField]
    Image leftCharacter = null;

    [SerializeField]
    Image rightCharacter = null;

    [SerializeField]
    public List<StoryData> storyDataList = new List<StoryData>();
    private int currentStoryIndex = 0;

    void Start()
    {
        // nextButtonのリスナーにメソッドを追加
        nextButton.onClick.AddListener(OnNextButtonClicked);

        // テスト用会話開始 (後で削除します)
        TalkStart();
    }

    // 会話の開始.
    public void TalkStart()
    {
        //   Time.timeScale = 0f; // ゲームの時間を停止
        DisplayCurrentStory();
    }

    // 現在のストーリーを表示
    private void DisplayCurrentStory()
    {
        if (currentStoryIndex < storyDataList.Count)
        {
            StoryData currentStory = storyDataList[currentStoryIndex];

            // 名前と会話内容を設定
            nameText.text = currentStory.Name;
            talkText.text = currentStory.Talk;

            // キャラクター立ち絵の表示を設定 (必要なら背景なども)
            UpdateCharacterSprites(currentStory);
        }
        else
        {
            // 全ての会話が終わった場合
            EndConversation();
        }
    }

    // キャラクターのスプライトを更新
    private void UpdateCharacterSprites(StoryData storyData)
    {
        // ここで `storyData.Left` や `storyData.Right` に基づいてキャラの立ち絵を設定します
        if (!string.IsNullOrEmpty(storyData.Left))
        {
            // 左側キャラクターの画像を設定 (リソースからロードなど)
            leftCharacter.sprite = Resources.Load<Sprite>(storyData.Left);
            leftCharacter.gameObject.SetActive(true);
        }
        else
        {
            leftCharacter.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(storyData.Right))
        {
            rightCharacter.sprite = Resources.Load<Sprite>(storyData.Right);
            rightCharacter.gameObject.SetActive(true);
        }
        else
        {
            rightCharacter.gameObject.SetActive(false);
        }
    }

    // 次のボタンが押されたときの処理
    private void OnNextButtonClicked()
    {
        currentStoryIndex++;
        DisplayCurrentStory();
    }

    // 会話の終了処理
    private void EndConversation()
    {
        transform.root.gameObject.SetActive(false);
        //  Time.timeScale = 1f; // ゲームの時間を停止
        // 会話終了後の処理 (ウィンドウを閉じる、次のイベントに進むなど)
        Debug.Log("会話が終了しました");
    }

    public StoryData GetStoryData(int index)
    {
        if (index >= 0 && index < storyDataList.Count)
        {
            return storyDataList[index];
        }
        return null;
    }
}
