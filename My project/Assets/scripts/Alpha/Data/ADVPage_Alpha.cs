using UnityEngine;

namespace Alpha.Data
{
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

        [Header("Backgrounds (Optional)")]
        public Sprite backgroundImage;
        [Tooltip("一枚絵。これが設定されている場合は立ち絵や背景より優先して全画面表示されます")]
        public Sprite eventCG;
    }
}
