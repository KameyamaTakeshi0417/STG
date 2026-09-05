using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

class Program
{
    static void Main()
    {
        string p1 = @"C:\Users\kanin\Documents\STG\My project\Assets\scripts\Alpha\Data\WeaponSeriesData_Alpha.cs";
        string t1 = File.ReadAllText(p1, Encoding.UTF8);
        t1 = Regex.Replace(t1, 
            @"\[Header\(""Part Specific Buffs""\)\].*?public float speedBonus = 100\.0f;", 
            @"[Header(""Part Specific Buffs (Per Rarity 1-4)"")]
        [Tooltip(""基礎攻撃力（レアリティ1〜4）"")]
        public float[] basePowerBonus = new float[4] { 1.0f, 2.0f, 3.0f, 4.0f };
        
        [Tooltip(""リロード速度（レアリティ1〜4）"")]
        public float[] reloadSpeedBonus = new float[4] { 0.1f, 0.2f, 0.3f, 0.4f };
        
        [Tooltip(""弾速（レアリティ1〜4）"")]
        public float[] bulletSpeedBonus = new float[4] { 100.0f, 150.0f, 200.0f, 250.0f };", 
            RegexOptions.Singleline);
        File.WriteAllText(p1, t1, new UTF8Encoding(true));

        string p2 = @"C:\Users\kanin\Documents\STG\My project\Assets\scripts\Alpha\Core\State\InventoryManager_Alpha.cs";
        string t2 = File.ReadAllText(p2, Encoding.UTF8);
        t2 = Regex.Replace(t2,
            @"public float GetPowerBonus\(\) => series != null \? series\.basePowerBonus \* GetMultiplier\(WeaponPartType_Alpha\.Bullet\) : 0f;\s*public float GetSurvivalBonus\(\) => series != null \? series\.survivalTimeBonus \* GetMultiplier\(WeaponPartType_Alpha\.Casing\) : 0f;\s*public float GetSpeedBonus\(\) => series != null \? series\.speedBonus \* GetMultiplier\(WeaponPartType_Alpha\.Primer\) : 0f;",
            @"public float GetPowerBonus() => series != null ? series.basePowerBonus[Mathf.Clamp(rarity - 1, 0, 3)] * GetMultiplier(WeaponPartType_Alpha.Bullet) : 0f;
        public float GetReloadBonus() => series != null ? series.reloadSpeedBonus[Mathf.Clamp(rarity - 1, 0, 3)] * GetMultiplier(WeaponPartType_Alpha.Casing) : 0f;
        public float GetSpeedBonus() => series != null ? series.bulletSpeedBonus[Mathf.Clamp(rarity - 1, 0, 3)] * GetMultiplier(WeaponPartType_Alpha.Primer) : 0f;",
            RegexOptions.Singleline);
        File.WriteAllText(p2, t2, new UTF8Encoding(true));

        string p3 = @"C:\Users\kanin\Documents\STG\My project\Assets\scripts\Alpha\Core\State\playerStatusManager_Alpha.cs";
        string t3 = File.ReadAllText(p3, Encoding.UTF8);
        t3 = Regex.Replace(t3,
            @"float additionalPower = 0f;\s*float additionalSurvivalTime = 0f;\s*float additionalSpeed = 0f;.*?additionalPower = instA\.GetPowerBonus\(\) \+ instB\.GetPowerBonus\(\) \+ instC\.GetPowerBonus\(\);\s*}\s*DamageAdd \+= additionalPower;\s*//[^\n]*\s*bulletSpeedMag \+= \(additionalSpeed / 500f\);\s*//[^\n]*\s*bulletLifeMag \+= \(additionalSurvivalTime / 2\.0f\);",
            @"float additionalPower = 0f;
        float additionalReload = 0f;
        float additionalSpeed = 0f;

        if (groupToPass >= 0 && groupToPass <= 2)
        {
            var instA = inv.Get(0, groupToPass);
            var instB = inv.Get(1, groupToPass);
            var instC = inv.Get(2, groupToPass);
            
            additionalSpeed = instA.GetSpeedBonus() + instB.GetSpeedBonus() + instC.GetSpeedBonus();
            additionalReload = instA.GetReloadBonus() + instB.GetReloadBonus() + instC.GetReloadBonus();
            additionalPower = instA.GetPowerBonus() + instB.GetPowerBonus() + instC.GetPowerBonus();
        }

        DamageAdd += additionalPower;
        float speedMagIncrease = (additionalSpeed / 500f);
        bulletSpeedMag += speedMagIncrease;
        DamageMag += (speedMagIncrease * 0.5f);
        BulletSpanMag -= additionalReload;",
            RegexOptions.Singleline);
        File.WriteAllText(p3, t3, new UTF8Encoding(true));
    }
}
