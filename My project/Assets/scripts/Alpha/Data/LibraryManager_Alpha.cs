using System;
using System.Collections.Generic;
using UnityEngine;
using Alpha.UI;

namespace Alpha.Data
{
    /// <summary>
    /// ライブラリ（チュートリアル、敵、弾、エフェクト等）のデータ管理およびアンロック管理マネージャー
    /// </summary>
    public class LibraryManager_Alpha : MonoBehaviour
    {
        private static LibraryManager_Alpha _instance;
        public static LibraryManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LibraryManager_Alpha>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LibraryManager_Alpha");
                        _instance = go.AddComponent<LibraryManager_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public enum LibraryTabType
        {
            Tutorial,
            Enemy,
            Bullet,
            Effect,
            Custom
        }

        [Serializable]
        public class TutorialItemData
        {
            public string id;
            public string title;
            public string category; // "基本操作" or "ゲームシステム"
            [TextArea(2, 5)] public string keyboardDesc;
            [TextArea(2, 5)] public string gamepadDesc;
            [TextArea(2, 4)] public string tips;
            public string shortcutHint;
        }

        [Header("Tutorial Master Data")]
        public List<TutorialItemData> tutorialDatabase = new List<TutorialItemData>();

        [Header("State")]
        [Tooltip("エディタ実行時に毎回セッション初回として通知をテストするかどうか")]
        public bool testNotificationInEditor = true;

        private HashSet<string> unlockedItemIds = new HashSet<string>();
        private HashSet<string> seenInSessionIds = new HashSet<string>();

        // イベント
        public event Action<string, string> OnItemUnlocked; // category, itemName

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDefaultTutorials();
            LoadUnlockedState();
        }

        private void InitializeDefaultTutorials()
        {
            if (tutorialDatabase.Count > 0) return;

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "001",
                title = "基本移動",
                category = "基本操作",
                keyboardDesc = "ようこそ！\nまずはWASDキーで移動してみましょう！",
                gamepadDesc = "ようこそ！\nまずは左スティックで移動してみましょう！",
                tips = "敵の攻撃や障害物を避けながら、安全な位置取りを心がけましょう。",
                shortcutHint = "移動: WASD / 左スティック"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "002",
                title = "照準と射撃",
                category = "基本操作",
                keyboardDesc = "マウスで敵にターゲット！\n左クリックで弾を発射出来ます。",
                gamepadDesc = "右スティックで敵にターゲット！\nLB または RBで弾を発射出来ます。",
                tips = "敵に照準を合わせ続けることで連続して効率よくダメージを与えられます。",
                shortcutHint = "射撃: マウス左クリック / LB・RB"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "00-",
                title = "装備換装",
                category = "基本操作",
                keyboardDesc = "1,3キーで現在の装備を換装できます！\nパーツをたくさん見つけて装備していきましょう！",
                gamepadDesc = "LT / RT（トリガー）で現在の装備を換装できます！\nパーツをたくさん見つけて装備していきましょう！",
                tips = "敵の種類や状況に合わせて最適な装備に切り替えて戦いましょう。",
                shortcutHint = "換装: 1, 3キー / LT・RT"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "003",
                title = "ウェーブ短縮",
                category = "基本操作",
                keyboardDesc = "敵が来なくて退屈ですか？\nShiftキーを入力すると、次ウェーブのエネミーが出てきます！\nウェーブ進行が早まるうえに報酬も増えますが、\n敵の増やし過ぎにはご注意。",
                gamepadDesc = "敵が来なくて退屈ですか？\nShift / Spaceキーで次ウェーブのエネミーが出現！\nウェーブ進行が早まるうえに報酬も増えますが、\n敵の増やし過ぎにはご注意。",
                tips = "自信があるときは積極的にスキップして、報酬とスコアを短時間で稼ぎましょう！",
                shortcutHint = "次ウェーブ呼出: Shift / Space"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "004",
                title = "報酬ゲージとオーブ",
                category = "ゲームシステム",
                keyboardDesc = "報酬ゲージが溜まるとオーブを獲得！\n敵を倒したり、進行ショートカットを使うことでゲージが溜まります。",
                gamepadDesc = "報酬ゲージが溜まるとオーブを獲得！\n敵を倒したり、進行ショートカットを使うことでゲージが溜まります。",
                tips = "敵を素早く撃破したりウェーブをスキップすることで効率よくゲージを蓄積できます。",
                shortcutHint = "ゲージ満タンでオーブ獲得"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "Tutorial_Rush",
                title = "エネミーラッシュ",
                category = "ゲームシステム",
                keyboardDesc = "エネミーラッシュはスキップできません。\n大量に出てくる敵を沢山倒して沢山報酬を獲得しましょう！",
                gamepadDesc = "エネミーラッシュはスキップできません。\n大量に出てくる敵を沢山倒して沢山報酬を獲得しましょう！",
                tips = "ラッシュ中は敵が絶え間なく大量出現するため、オーブゲージを一気に溜めるチャンスです！",
                shortcutHint = "スキップ不可・大量討伐チャンス"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "Tutorial_Orb_AutoEquip",
                title = "オーブとパーツの自動装備",
                category = "ゲームシステム",
                keyboardDesc = "オーブからはパーツが手に入ります！\n発射時効果が変わる雷管\n航行時効果が変わる薬莢\n着弾時効果が変わる弾頭\n最初に手に入れる3つのオーブは自動で開封、装備されます！",
                gamepadDesc = "オーブからはパーツが手に入ります！\n発射時効果が変わる雷管\n航行時効果が変わる薬莢\n着弾時効果が変わる弾頭\n最初に手に入れる3つのオーブは自動で開封、装備されます！",
                tips = "ステージ1前半で最初に拾う3つのオーブは、弾頭・薬莢・雷管の順で自動的に装備枠へ装備されます。",
                shortcutHint = "最初の3個は自動開封＆装備"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "Tutorial_Orb",
                title = "オーブドロップ",
                category = "ゲームシステム",
                keyboardDesc = "オーブを拾いました！こうして偶に敵からドロップします。\n中身はウェーブ終了時に獲得できます。\n生き残れ！",
                gamepadDesc = "オーブを拾いました！こうして偶に敵からドロップします。\n中身はウェーブ終了時に獲得できます。\n生き残れ！",
                tips = "ドロップしたオーブはステージ上に残ります。見逃さず確実に回収しましょう。",
                shortcutHint = "敵撃破時に低確率ドロップ"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "Tutorial_Elite",
                title = "エリートエネミー",
                category = "ゲームシステム",
                keyboardDesc = "強敵には体力バーが複数本あります。\nゲージを削ると攻撃が強くなるので注意！\nしかも時間経過で逃げてしまいます！\n倒して豪華報酬をゲット！",
                gamepadDesc = "強敵には体力バーが複数本あります。\nゲージを削ると攻撃が強くなるので注意！\nしかも時間経過で逃げてしまいます！\n倒して豪華報酬をゲット！",
                tips = "逃走バー（Flee Bar）が満タンになる前に集中砲火で倒しきりましょう。",
                shortcutHint = "逃走前に撃破で豪華報酬"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "Tutorial_Reward",
                title = "報酬フェーズ",
                category = "ゲームシステム",
                keyboardDesc = "待ちに待った報酬フェーズです！\nオーブを開封して\n報酬装備をゲットしていきましょう！",
                gamepadDesc = "待ちに待った報酬フェーズです！\nオーブを開封して\n報酬装備をゲットしていきましょう！",
                tips = "集めたオーブを一気に開封し、強力な装備パーツを手に入れましょう。",
                shortcutHint = "ウェーブ合間の強化チャンス"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "Tutorial_Equip",
                title = "装備とセットボーナス",
                category = "ゲームシステム",
                keyboardDesc = "待ちに待った装備画面です。ポーズ画面でもあります\n装備変更は報酬獲得後しか行えないので注意してください。\n縦スロットに同じ紋章を揃えるとセットボーナスが発動します！",
                gamepadDesc = "待ちに待った装備画面です。ポーズ画面でもあります\n装備変更は報酬獲得後しか行えないので注意してください。\n縦スロットに同じ紋章を揃えるとセットボーナスが発動します！",
                tips = "縦一列に同一シンボルを揃えると強力なシナジー効果が発動します。",
                shortcutHint = "縦スロット同紋章でボーナス発動"
            });

            tutorialDatabase.Add(new TutorialItemData
            {
                id = "005",
                title = "ボス戦予告と回復",
                category = "ゲームシステム",
                keyboardDesc = "エリート戦お疲れ様でした！\n休む間もなく訪れたこのウェーブを抜けたらボス戦です。\nボス戦前には体力回復報酬フェーズがあるので頑張って！",
                gamepadDesc = "エリート戦お疲れ様でした！\n休む間もなく訪れたこのウェーブを抜けたらボス戦です。\nボス戦前には体力回復報酬フェーズがあるので頑張って！",
                tips = "ボス直前の休憩ウェーブで万全の状態に立て直しましょう。",
                shortcutHint = "ボス前の回復フェーズ"
            });
        }

        private void LoadUnlockedState()
        {
            unlockedItemIds.Clear();
            seenInSessionIds.Clear();

            // 保存済みアンロック情報のロード
            foreach (var item in tutorialDatabase)
            {
                if (PlayerPrefs.GetInt($"Lib_Unlock_{item.id}", 0) == 1)
                {
                    unlockedItemIds.Add(item.id);
                }
            }

            if (PlayerPrefs.GetInt("Tutorial_Seen_001", 0) == 1) unlockedItemIds.Add("001");
            if (PlayerPrefs.GetInt("Tutorial_Seen_002", 0) == 1) unlockedItemIds.Add("002");
        }

        /// <summary>
        /// 項目をアンロックし、未アンロックだった場合は画面右端通知を発火
        /// </summary>
        public bool UnlockItem(string category, string itemId, string displayName)
        {
            bool isAlreadyUnlocked = unlockedItemIds.Contains(itemId);

            // エディタテスト時は、セッション初回遭遇であれば通知を出す
            bool shouldNotify = false;
            if (Application.isEditor && testNotificationInEditor)
            {
                if (!seenInSessionIds.Contains(itemId))
                {
                    seenInSessionIds.Add(itemId);
                    shouldNotify = true;
                }
            }
            else
            {
                shouldNotify = !isAlreadyUnlocked;
            }

            if (!isAlreadyUnlocked)
            {
                unlockedItemIds.Add(itemId);
                PlayerPrefs.SetInt($"Lib_Unlock_{itemId}", 1);
                PlayerPrefs.Save();
                OnItemUnlocked?.Invoke(category, displayName);
            }

            if (shouldNotify)
            {
                Debug.Log($"<color=#33ffbb>[LibraryManager]</color> Notify Unlock: [{category}] {displayName} (ID: {itemId})");
                if (LibraryNotificationManager_Alpha.Instance != null)
                {
                    LibraryNotificationManager_Alpha.Instance.EnqueueNotification(category, displayName);
                }
            }

            return !isAlreadyUnlocked;
        }

        /// <summary>
        /// チュートリアルIDに対応するアンロック処理
        /// </summary>
        public void UnlockTutorial(string tutorialId)
        {
            var data = tutorialDatabase.Find(t => t.id == tutorialId);
            string title = data != null ? data.title : tutorialId;
            UnlockItem("チュートリアル", tutorialId, title);
        }

        [ContextMenu("Reset All Library Progress")]
        public void ResetAllLibraryProgress()
        {
            foreach (var item in tutorialDatabase)
            {
                PlayerPrefs.DeleteKey($"Lib_Unlock_{item.id}");
                PlayerPrefs.DeleteKey($"Tutorial_Seen_{item.id}");
            }
            unlockedItemIds.Clear();
            seenInSessionIds.Clear();
            PlayerPrefs.Save();
            Debug.Log("[LibraryManager] All library save data reset!");
        }

        public bool IsItemUnlocked(string itemId)
        {
            return unlockedItemIds.Contains(itemId);
        }

        public TutorialItemData GetTutorialData(string id)
        {
            return tutorialDatabase.Find(t => t.id == id);
        }
    }
}
