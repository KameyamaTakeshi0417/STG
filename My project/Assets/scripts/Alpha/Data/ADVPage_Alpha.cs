using UnityEngine;

namespace Alpha.Data
{
    public enum ADVCharacterAnim
    {
        None,
        SlideInLeft,
        SlideInRight,
        SlideInBottom,
        SlideOutLeft,
        SlideOutRight,
        SlideOutBottom
    }

    [System.Serializable]
    public class ADVPage_Alpha
    {
        [Header("Dialogue")]
        public string characterName;
        [TextArea(3, 5)]
        public string dialogueText;

        [Header("Characters (Optional)")]
        public Sprite leftCharacter;
        public Sprite rightCharacter;
        public Sprite centerCharacter;

        [Header("Character Animations")]
        public ADVCharacterAnim leftCharacterAnim = ADVCharacterAnim.None;
        public ADVCharacterAnim centerCharacterAnim = ADVCharacterAnim.None;
        public ADVCharacterAnim rightCharacterAnim = ADVCharacterAnim.None;
        [Tooltip("アニメーションが完了するまでテキストの表示（タイプライター）を待機するかどうか")]
        public bool waitForAnimationToFinish = false;

        [Header("Backgrounds (Optional)")]
        public Sprite backgroundImage;
        [Tooltip("一枚絵。これが設定されている場合は立ち絵・背景を優先して全画面表示されます")]
        public Sprite eventCG;

        [Header("Sounds (Optional)")]
        [Tooltip("このページで再生を開始するBGM（設定すると曲が切り替わります）")]
        public AudioClip bgmClip;
        [Tooltip("このページが表示された瞬間に鳴らすSE")]
        public AudioClip seClip;

        [Header("Speaking Flags")]
        [Tooltip("左のキャラクターが話しているか")]
        public bool leftSpeaking = false;
        [Tooltip("中央のキャラクターが話しているか")]
        public bool centerSpeaking = false;
        [Tooltip("右のキャラクターが話しているか")]
        public bool rightSpeaking = false;
    }
}
