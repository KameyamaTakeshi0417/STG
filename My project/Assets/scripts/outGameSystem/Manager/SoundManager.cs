using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource UI_Decide;
    public AudioSource UI_Cancel;

    private Dictionary<string, AudioSource> audioSources; // 名前とAudioSourceを関連付ける辞書

    private static SoundManager instance;

    void Awake()
    {
        // シングルトンとして動作させる
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでもオブジェクトを保持
        }
        else
        {
            Destroy(gameObject); // 既存のインスタンスがあれば新しいインスタンスを破棄
            return;
        }

        // 辞書を初期化
        audioSources = new Dictionary<string, AudioSource>
        {
            { "UI_Decide", UI_Decide },
            { "UI_Cancel", UI_Cancel },
        };
    }

    public void Play(string targetName)
    {
        // 指定された名前のAudioSourceが存在するかチェック
        if (audioSources.ContainsKey(targetName))
        {
            AudioSource source = audioSources[targetName];
            if (source != null)
            {
                source.Play();
            }
            else
            {
                Debug.LogWarning($"AudioSource '{targetName}' is null.");
            }
        }
        else
        {
            Debug.LogWarning($"No AudioSource found with the name '{targetName}'.");
        }
    }
}
