using UnityEngine;

namespace Alpha.Data
{
    public enum WeaponEffectType_Alpha
    {
        StatUp,
        BulletChange,
        Constraint,
        Other
    }

    [CreateAssetMenu(fileName = "NewWeaponEffect", menuName = "Alpha/Weapon Effect")]
    public class WeaponEffectSO_Alpha : ScriptableObject
    {
        public WeaponEffectType_Alpha effectType;
        public string effectName;
        public string description;

        [Tooltip("各品質(1〜4)における効果量。インデックス0=品質1、インデックス3=品質4")]
        public float[] qualityValues = new float[4];

        public float GetValue(int quality)
        {
            if (qualityValues == null || qualityValues.Length == 0) return 0f;
            int index = Mathf.Clamp(quality - 1, 0, qualityValues.Length - 1);
            return qualityValues[index];
        }
    }
}
