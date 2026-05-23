using System;
using UnityEngine;

namespace Alpha.Battle.Bullet
{
    public static class EffectFactory_Alpha
    {
        public static Alpha_Effect_Base CreateEffect(string className, int position, int rarity)
        {
            if (string.IsNullOrEmpty(className))
                return null;

            try
            {
                // 現在のアセンブリ内からクラス型を取得
                Type effectType = Type.GetType(className);
                
                // 見つからなければアセンブリ修飾名なしで探す等の工夫（同一アセンブリならGetTypeでOKなことが多い）
                if (effectType == null)
                {
                    // もし名前空間がある場合は手動で補完するか、すべてのアセンブリから検索する
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        effectType = asm.GetType(className);
                        if (effectType != null) break;
                    }
                }

                if (effectType != null && effectType.IsSubclassOf(typeof(Alpha_Effect_Base)))
                {
                    // Alpha_Effect_Base のコンストラクタは (int position, int rarity) を想定
                    return (Alpha_Effect_Base)Activator.CreateInstance(effectType, position, rarity);
                }
                else
                {
                    Debug.LogWarning($"[EffectFactory] Class '{className}' not found or is not a subclass of Alpha_Effect_Base.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EffectFactory] Error creating effect instance for '{className}': {e.Message}");
            }

            return null;
        }
    }
}
