using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    public class UIBounceAutoAttacher_Alpha : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            GameObject attacher = new GameObject("UIBounceAutoAttacher_Alpha");
            DontDestroyOnLoad(attacher);
            attacher.AddComponent<UIBounceAutoAttacher_Alpha>();
        }

        private void Start()
        {
            InvokeRepeating(nameof(AttachToNewButtons), 0f, 1f); // 1秒ごとに新規生成されたボタンにアタッチ
        }

        private void AttachToNewButtons()
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var btn in buttons)
            {
                if (btn.GetComponent<IgnoreUIBounce_Alpha>() == null && btn.GetComponent<UIBounce_Alpha>() == null)
                {
                    btn.gameObject.AddComponent<UIBounce_Alpha>();
                }
            }
        }
    }
}
