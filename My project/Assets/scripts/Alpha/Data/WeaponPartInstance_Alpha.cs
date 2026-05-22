using System.Collections.Generic;

namespace Alpha.Data
{
    [System.Serializable]
    public class WeaponPartInstance_Alpha
    {
        public WeaponSeriesData_Alpha series;
        public WeaponPartType_Alpha partType;
        public int quality; // 1〜4

        public List<WeaponEffectSO_Alpha> currentEffects = new List<WeaponEffectSO_Alpha>();

        public WeaponPartInstance_Alpha(WeaponSeriesData_Alpha series, WeaponPartType_Alpha partType, int quality)
        {
            this.series = series;
            this.partType = partType;
            this.quality = quality;
        }
    }
}
