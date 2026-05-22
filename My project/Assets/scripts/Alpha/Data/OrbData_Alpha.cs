namespace Alpha.Data
{
    public enum OrbSource_Alpha
    {
        Mob,
        Skip,
        MidBoss,
        Boss
    }

    [System.Serializable]
    public class OrbData_Alpha
    {
        public int orbRarity; // 1〜4
        public OrbSource_Alpha source;
        public string bossId; // ボス産の場合のシリーズ紐付け用

        public OrbData_Alpha(int orbRarity, OrbSource_Alpha source, string bossId = "")
        {
            this.orbRarity = orbRarity;
            this.source = source;
            this.bossId = bossId;
        }
    }
}
