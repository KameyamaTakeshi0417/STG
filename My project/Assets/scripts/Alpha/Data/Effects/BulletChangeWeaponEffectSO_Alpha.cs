using UnityEngine;
using Alpha.Data;

namespace Alpha.Data
{
    public enum BulletChangeTier
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Divine = 3
    }

    [CreateAssetMenu(fileName = "NewBulletChangeEffect", menuName = "Alpha/Bullet Change Effect")]
    public class BulletChangeWeaponEffectSO_Alpha : WeaponEffectSO_Alpha
    {
        [Header("Bullet Override Settings")]
        [Tooltip("このエフェクトが適用された時に発射される弾のプレハブ")]
        public GameObject bulletPrefab;

        [Tooltip("弾変更の優先度を決定するシリーズの格")]
        public BulletChangeTier seriesTier = BulletChangeTier.Common;

        private void Reset()
        {
            effectType = WeaponEffectType_Alpha.BulletChange;
        }
    }
}
