[System.Serializable]
public class AmmoInstance
{
    public ItemData ammoData; // 参照する元のScriptableObject
    public int rarity; // 各インスタンスのレアリティ
    public float itemHP;
    public float itemPower;
    public float itemSpeed;

    public AmmoInstance(ItemData data, int rarity)
    {
        this.ammoData = data;
        this.rarity = rarity;
        
        // レアリティに基づく値の設定
        this.itemHP = data.itemHP * rarity;
        this.itemPower = data.itemPower * rarity;
        this.itemSpeed = data.itemSpeed * rarity;
    }
}
