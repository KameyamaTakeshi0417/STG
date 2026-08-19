using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Data
{
    [CreateAssetMenu(fileName = "NewCompositeEffect", menuName = "Alpha/Composite Weapon Effect")]
    public class CompositeWeaponEffectSO_Alpha : WeaponEffectSO_Alpha
    {
        [Tooltip("List of sub-effects for this composite skill")]
        public List<WeaponEffectSO_Alpha> subEffects = new List<WeaponEffectSO_Alpha>();

        private void OnValidate()
        {
            effectType = WeaponEffectType_Alpha.Composite;
        }
    }
}
