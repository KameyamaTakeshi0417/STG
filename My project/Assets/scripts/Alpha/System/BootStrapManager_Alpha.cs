using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Alpha.Core
{
    public class BootStrapManager_Alpha : MonoBehaviour
    {
        [Tooltip("ブートストラップ完了後に読み込むシーン名")]
        public string nextSceneName = "Title_Alpha";

        void Start()
        {
            // 全てのグローバルマネージャーの Awake (DontDestroyOnLoad) が終わった後、1フレーム待ってからタイトルへ
            StartCoroutine(LoadNextScene());
        }

        private IEnumerator LoadNextScene()
        {
            yield return null; // 1フレーム待機
            
            Debug.Log("[BootStrap] Global systems initialized. Transitioning to Title...");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
